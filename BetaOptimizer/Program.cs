using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class BetaOptimizer {
    static string[] MergeTrianglesPass(string[] lines, ref bool mutated) {
        List<string> linesList = new List<string>(lines);

        bool processVertices = false;
        string tri1 = null;
        for (int i = 0; i < linesList.Count; i++) {
            if (processVertices) {
                if (linesList[i].TrimStart().StartsWith("gsSP1Triangle(")) {
                    string parameters = linesList[i].Split('(')[1].Split(')')[0];
                    if (tri1 == null) {
                        tri1 = parameters;
                    }
                    else {
                        mutated = true;
                        linesList.RemoveAt(i--);
                        linesList[i] = $"{linesList[i].Remove(linesList[i].Length - linesList[i].TrimStart().Length)}gsSP2Triangles({tri1},{parameters}),";
                        tri1 = null;
                    }
                    processVertices = true;
                }
                else {
                    processVertices = false;
                    tri1 = null;
                }
            }
            else {
                if (linesList[i].TrimStart().StartsWith("gsSPVertex(")) {
                    processVertices = true;
                }
            }
        }

        return linesList.ToArray();
    }

    static string[] FixAnimationsPass(string[] lines, ref bool mutated) {
        List<string> linesList = new List<string>(lines);

        int animStructStart = -1;
        for (int i = 0; i < linesList.Count; i++) {
            //Console.WriteLine($"{animStructStart} | {i}: {linesList[i]}");
            if (animStructStart == -1) {
                if (linesList[i].Contains("struct Animation") && linesList[i].TrimEnd().EndsWith("{")) {
                    //Console.WriteLine("struct Animation found");
                    animStructStart = i;
                }
            }
            else {
                if (linesList[i].Trim() == "};") {
                    //Console.WriteLine("}; found");
                    if (i - animStructStart == 10) {
                        //Console.WriteLine("excessive length, mutating");
                        linesList[animStructStart + 6] = linesList[animStructStart + 9];
                        linesList.RemoveAt(animStructStart + 9);
                        mutated = true;
                        i--; // removed 1 prior line
                    }

                    animStructStart = -1;
                }
            }
        }

        return linesList.ToArray();
    }

    static void DirectorySearch(string dir) {
        string[] allowedExtensions = { ".c", ".sou" };
        
        foreach (string file in Directory.GetFiles(dir)) {
            if (allowedExtensions.Contains(Path.GetExtension(file))) {
                bool mutated = false;
                string[] lines = File.ReadAllLines(file);

                lines = MergeTrianglesPass(lines, ref mutated);
                lines = FixAnimationsPass(lines, ref mutated);

                if (mutated) {
                    Console.WriteLine($"Optimized {file}");
                    File.WriteAllLines(file, lines);
                }
            }
        }

        foreach (string subdir in Directory.GetDirectories(dir)) {
            DirectorySearch(subdir);
        }
    }

    static void Main(string[] args) {
        DirectorySearch("./");
    }
}
