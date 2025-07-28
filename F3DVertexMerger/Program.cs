using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace F3DVertexMerger {
    struct Vertex {
        public int x, y, z,
            unknown,
            u, v;
        public byte r, g, b, a; // 4 bytes that can be used for vertex color or normal

        public Vertex(string line) {
            line = line.Trim().Replace(" ", "").Replace("{", "").Replace("}", "").Replace("{", " ");
            string[] split = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            x = ParseCInt.Parse(split[0]);
            y = ParseCInt.Parse(split[1]);
            z = ParseCInt.Parse(split[2]);
            unknown = ParseCInt.Parse(split[3]);
            u = ParseCInt.Parse(split[4]);
            v = ParseCInt.Parse(split[5]);

            r = (byte)ParseCInt.Parse(split[6]);
            g = (byte)ParseCInt.Parse(split[7]);
            b = (byte)ParseCInt.Parse(split[8]);
            a = (byte)ParseCInt.Parse(split[9]);
        }

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    struct GSCommand {
        public int tri11, tri12, tri13, unk1,
            tri21, tri22, tri23, unk2;

        public GSCommand(string line) {
            line = line.Trim();
            line = line.Substring(line.Split('(')[0].Length + 1);
            line = line.Replace("(", "").Replace(")", "");

            string[] split = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            tri11 = ParseCInt.Parse(split[0]);
            tri12 = ParseCInt.Parse(split[1]);
            tri13 = ParseCInt.Parse(split[2]);
            unk1 = ParseCInt.Parse(split[3]);

            if (split.Length > 4) {
                tri21 = ParseCInt.Parse(split[4]);
                tri22 = ParseCInt.Parse(split[5]);
                tri23 = ParseCInt.Parse(split[6]);
                unk2 = ParseCInt.Parse(split[7]);
            }
            else {
                tri21 = -1;
                tri22 = -1;
                tri23 = -1;
                unk2 = -1;
            }
        }

        // TODO: add StartIndex when creating GSCommand, so that display lists that read from the middle of vertex data are easy to not break
        public GSCommand ApplyRemapTable(Dictionary<int, int> kvp, int startIndex) {
            string str = "(";

            foreach (FieldInfo fieldInfo in GetType().GetFields()) {
                bool written = false;
                if (fieldInfo.Name.StartsWith("tri", StringComparison.Ordinal)) {
                    int originalIndex = (int)fieldInfo.GetValue(this);

                    if (originalIndex >= 0) {
                        if (kvp.ContainsKey(originalIndex + startIndex)) {
                            str += kvp[originalIndex + startIndex] + ",";
                            written = true;
                        }
                    }
                }

                if (!written) {
                    str += fieldInfo.GetValue(this) + ",";
                }
            }

            return new GSCommand(str);
        }

        public override string ToString() {
            if (tri21 == -1) {
                return "gsSP1Triangle (" + tri11 + "," + tri12 + "," + tri13 + ", " + unk1 + "),";
            }
            return "gsSP2Triangles(" + tri11 + "," + tri12 + "," + tri13 + ", " + unk1 + 
            ", " + tri21 + "," + tri22 + "," + tri23 + ", " + unk2 + "),";
        }
    }

    struct Merge {
        public string key1, key2;
        public int newVertexCount;

        public Merge(string key1, string key2, int newVertexCount) {
            this.key1 = key1;
            this.key2 = key2;
            this.newVertexCount = newVertexCount;
        }
    }

    class MainClass {
        public static void Main(string[] args) {
            bool f3dex = true; // false = 16 vertices, true = 32 vertices
            int maxVertices = f3dex ? 32 : 16;

            StreamReader reader = new StreamReader("data.c");
            Dictionary<string, List<Vertex>> vertexData = new Dictionary<string, List<Vertex>>();
            List<Dictionary<string, List<GSCommand>>> allGfxData = new List<Dictionary<string, List<GSCommand>>>();

            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                if (line.StartsWith("static const Vtx", StringComparison.Ordinal)) {
                    string arrayName = line.Split(' ')[3].TrimEnd(new[] { '[', ']' });
                    List<Vertex> vertices = new List<Vertex>();
                    
                    while (!line.Contains("};")) {
                        line = reader.ReadLine();
                        if (line.EndsWith(",", StringComparison.Ordinal)) {
                            Vertex vertex = new Vertex(line);
                            vertices.Add(vertex);
                        }
                    }

                    vertexData.Add(arrayName, vertices);
                }
                else if (line.StartsWith("const Gfx", StringComparison.Ordinal)) {
                    Dictionary<string, List<GSCommand>> gfxData = new Dictionary<string, List<GSCommand>>();
                    string currentArrayName = null;

                    while (!line.Contains("};")) {
                        line = reader.ReadLine().Replace(" ", "").Trim();
                        if (line.EndsWith(",", StringComparison.Ordinal)) {
                            if (line.StartsWith("gsSPVertex(", StringComparison.Ordinal)) {
                                currentArrayName = line.Substring(line.Split('(')[0].Length + 1).Split(',')[0];
                                Console.WriteLine("Vertices: " + currentArrayName);
                                if (!gfxData.ContainsKey(currentArrayName)) {
                                    gfxData.Add(currentArrayName, new List<GSCommand>());
                                }
                            }
                            else if (line.StartsWith("gsSP1Triangle(", StringComparison.Ordinal) || line.StartsWith("gsSP2Triangles(", StringComparison.Ordinal)) {
                                Console.WriteLine(line + " -> " + currentArrayName);
                                if (currentArrayName != null) {
                                    GSCommand command = new GSCommand(line);
                                    gfxData[currentArrayName].Add(command);
                                }
                            }
                        }
                    }

                    allGfxData.Add(gfxData);
                }
            }

            StreamWriter writer = new StreamWriter("out.c");
            foreach (Dictionary<string, List<GSCommand>> gfxData in allGfxData) {
                List<string> keys = new List<string>(gfxData.Keys);

                // Attempt to merge vertex data per Gfx
                bool merging = false;
                do {
                    merging = false;
                    for (int i = 0; i < keys.Count; i++) {
                        for (int j = i + 1; j < keys.Count; j++) {
                            string key1 = keys[i];
                            string key2 = keys[j];

                            List<Vertex> mergedVertices = new List<Vertex>(vertexData[key1]);
                            mergedVertices.AddRange(vertexData[key2]);
                            mergedVertices = MergeVertices(mergedVertices, out Dictionary<int, int> remapTable);
                            bool couldMerge = mergedVertices.Count <= maxVertices;

                            Console.WriteLine("Merging vertices " + key1 + " {");
                            foreach (Vertex v in vertexData[key1]) {
                                Console.WriteLine("\t" + v.ToString());
                            }
                            Console.WriteLine("} and " + key2 + " {");
                            foreach (Vertex v in vertexData[key2]) {
                                Console.WriteLine("\t" + v.ToString());
                            }
                            Console.WriteLine("}: {");
                            foreach (Vertex v in mergedVertices) {
                                Console.WriteLine("\t" + v.ToString());
                            }
                            Console.WriteLine("}");
                            Console.WriteLine("Can merge (" + (f3dex ? "F3DEX" : "F3D") + "): " + couldMerge);
                            Console.WriteLine();

                            if (couldMerge) {
                                writer.WriteLine("// " + key1 + " + " + key2);
                                writer.WriteLine("static const Vtx " + key1 + "[] = {");
                                foreach (Vertex v in mergedVertices) {
                                    writer.WriteLine("    {{{" + v.x + ", " + v.y + ", " + v.z + "}, " + v.unknown + ", {" + v.u + ", " + v.v + "}, {" + v.r + ", " + v.g + ", " + v.b + ", " + v.a + "}}},");
                                    Console.WriteLine("\t" + v.ToString());
                                }
                                writer.WriteLine("};\n");

                                Console.WriteLine("Remap table:");
                                foreach (KeyValuePair<int, int> kvp in remapTable) {
                                    Console.WriteLine(kvp.Key + " -> " + kvp.Value);
                                }

                                List<GSCommand> additionalTris = new List<GSCommand>();
                                for (int cmdi = 0; cmdi < gfxData[key2].Count; cmdi++) {
                                    additionalTris.Add(gfxData[key2][cmdi].ApplyRemapTable(remapTable, vertexData[key1].Count));
                                }

                                writer.WriteLine("// Display list triangles");
                                writer.WriteLine("    gsSPVertex(" + key1 + ", " + mergedVertices.Count + ", 0),");
                                foreach (GSCommand command in gfxData[key1]) {
                                    writer.WriteLine("    " + command.ToString());
                                }
                                foreach (GSCommand command in additionalTris) {
                                    writer.WriteLine("    " + command.ToString());
                                }
                                writer.WriteLine();
                            }
                        }
                    }
                } while (merging);
            }
            writer.Close();
        }

        public static List<Vertex> MergeVertices(List<Vertex> vertices, out Dictionary<int, int> remapTable) {
            List<Vertex> internalVertices = new List<Vertex>();

            remapTable = new Dictionary<int, int>();
            int j = 0;
            for (int i = 0; i < vertices.Count; i++) {
                int originalIndex = internalVertices.IndexOf(vertices[i]);

                if (originalIndex != -1) {
                    remapTable.Add(i, originalIndex);
                }
                else {
                    internalVertices.Add(vertices[i]);
                    remapTable.Add(i, j);
                    j++;
                }
            }

            return new List<Vertex>(internalVertices);
        }
    }
}
