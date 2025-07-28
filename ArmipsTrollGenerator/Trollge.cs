using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArmipsTrollGenerator {
    class Trollge {
        static Dictionary<string, string> collisionNameToC = new Dictionary<string, string>();
        
        static string ExtractArrayName(string line, out string arrayType) {
            arrayType = null;
            if (line.StartsWith("const ") && line.EndsWith("[] = {")) {
                arrayType = line.Split(' ')[1];
                return line[("const  ".Length + arrayType.Length)..^("[] = {".Length)];
            }

            return null;
        }

        public static void CursivelyFindCollisions(string dir) {
            foreach (string subdir in Directory.GetDirectories(dir)) {
                CursivelyFindCollisions(subdir);
            }

            foreach (string fpath in Directory.GetFiles(dir)) {
                if (fpath.EndsWith(".c")) {
                    string[] lines = File.ReadAllLines(fpath);

                    string arrayName = null;
                    string currFunction = "";
                    for (int i = 0; i < lines.Length; i++) {
                        if (arrayName == null) {
                            arrayName = ExtractArrayName(lines[i], out string t);
                            if (arrayName != null) {
                                if (t == "Collision" && arrayName.Contains("seg7_collision") && !arrayName.EndsWith("_level")) {
                                    currFunction += lines[i] + "\n";
                                }
                                else {
                                    arrayName = null;
                                }
                            }
                        }
                        else {
                            currFunction += lines[i] + "\n";

                            if (lines[i].StartsWith("};")) {
                                if (!collisionNameToC.TryAdd(arrayName, currFunction)) {
                                    Console.WriteLine($"Duplicate collision found with name {arrayName} in {fpath}??");
                                }
                                arrayName = null;
                                currFunction = "";
                            }
                        }
                    }
                }
            }
        }

        public static bool Trolle() {
            Console.WriteLine("true the sex mode is old and boring");
            Console.WriteLine("HOLY SHIT THERES A NEW TROLL MODE");
            Console.WriteLine("THIS IS NOT A TROLL. THERE ACTUALLY IS A TROLL MODE :omegatroll: :omegatroll: :omegatroll:");
            Console.WriteLine("Type TROLLGE for new troll mode");
            if (Console.ReadLine() != "TROLLGE") return false;

            Console.WriteLine("SECRET TROLL MODE UNLOCKED");
            Console.WriteLine("Just give a decomp root directory to casually get all seg7 object collision");

            string dirPath = Program.ReadNotWhitespace("Directory of decomp: ");
            if (!Directory.Exists(dirPath)) {
                Console.WriteLine("That directory does not exist!");
                return false;
            }
            Console.WriteLine("Holy shit the directory is real, hol up real quick");

            CursivelyFindCollisions($"{dirPath}levels/");
            Console.WriteLine($"Found {collisionNameToC.Count} collisions");

            HashSet<string> usedCollisions = new HashSet<string>();

            {
                StreamReader sr = new StreamReader($"{dirPath}data/behavior_data.c");
                StreamWriter sw = new StreamWriter("output/bhvtroll.asm");
                sw.WriteLine(".headersize 0x13000000-ROMSTART");
                sw.WriteLine();

                Dictionary<string, int> cmdSizes = new Dictionary<string, int> {
                    { "CALL", 2 },
                    { "GOTO", 2 },
                    { "CALL_NATIVE", 2 },
                    { "SET_INT_RAND_RSHIFT", 2 },
                    { "SET_RANDOM_FLOAT", 2 },
                    { "SET_RANDOM_INT", 2 },
                    { "ADD_RANDOM_FLOAT", 2 },
                    { "ADD_INT_RAND_RSHIFT", 2 },
                    { "SPAWN_CHILD", 3 },
                    { "SET_HITBOX", 2 },
                    { "LOAD_ANIMATIONS", 2 },
                    { "SPAWN_CHILD_WITH_PARAM", 3 },
                    { "LOAD_COLLISION_DATA", 2 },
                    { "SET_HITBOX_WITH_OFFSET", 3 },
                    { "SPAWN_OBJ", 3 },
                    { "SET_HURTBOX", 2 },
                    { "SET_INTERACT_TYPE", 2 },
                    { "SET_OBJ_PHYSICS", 5 },
                    { "SET_INTERACT_SUBTYPE", 2 },
                    { "PARENT_BIT_CLEAR", 2 },
                    { "SET_INT_UNUSED", 2 },
                    { "SPAWN_WATER_DROPLET", 2 }
                };

                string currBehavior = null;
                ulong currBehaviorLocation = 0;
                int currBehaviorOffset = 0;
                while (!sr.EndOfStream) {
                    string ln = sr.ReadLine();

                    if (currBehavior == null) {
                        currBehavior = ExtractArrayName(ln, out string t);
                        currBehaviorOffset = 0;
                        if (currBehavior != null) {
                            if (t == "BehaviorScript") {
                                Program.GetPointersForSymbol(currBehavior, out currBehaviorLocation, out _);
                            }
                            else {
                                currBehavior = null;
                            }
                        }
                    }
                    else {
                        if (ln.StartsWith("};")) {

                            currBehavior = null;
                        }
                        else {
                            string cmd = ln.Trim().Split('(')[0];

                            if (cmd == "LOAD_COLLISION_DATA") {
                                string collisionName = ln.Split('(')[1].Split(')')[0];
                                if (collisionNameToC.ContainsKey(collisionName)) {
                                    sw.WriteLine($".org 0x{currBehaviorLocation:X8}+0x{(currBehaviorOffset + 1) * 4:X2} ; {currBehavior}");
                                    sw.WriteLine($".word {collisionName} & 0x00FFFFFF");
                                    usedCollisions.Add(collisionName);
                                }
                            }
                            if (cmd.Length > 0) {
                                currBehaviorOffset += cmdSizes.GetValueOrDefault(cmd, 1);
                            }
                        }
                    }
                }

                sw.Close();
                sr.Close();
                Console.WriteLine($"Created bhvtroll.asm to troll behavior scripts");
            }

            {
                foreach (string file in Directory.GetFiles($"{dirPath}src/game/behaviors/")) {
                    string fname = Path.GetFileName(file);
                    if (fname.EndsWith(".c")) {
                        string fileContents = File.ReadAllText(file);
                        foreach (string collision in collisionNameToC.Keys) {
                            if (fileContents.Contains(collision)) {
                                usedCollisions.Add(collision);
                            }
                        }
                    }
                }
                Program.AutomatedBhvTroll($"{dirPath}src/game/behaviors/", "bhv", collisionNameToC.Keys);
                Console.WriteLine($"Trolled bhv functions");
            }

            {
                Directory.CreateDirectory($"output");
                StreamWriter sw = new StreamWriter("output/collision.c");
                sw.WriteLine(@"#include ""types.h""
#include ""surface_terrains.h""
");
                foreach (string key in usedCollisions) {
                    sw.WriteLine(collisionNameToC[key]);
                }
                sw.Close();
                Console.WriteLine($"Extracted used collisions ({usedCollisions.Count}) to collision.c");
            }

            Console.WriteLine("Troll incomplete... a lot of seg7 collision is in arrays, so have fun :trol:");
            return true;
        }
    }
}