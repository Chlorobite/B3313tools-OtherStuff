using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArmipsTrollGenerator {
    class FunctionData {
        public string fileName;
        public string methodName;
        public string function;
    }

    class Program {
        static string ExtractMethodName(string line) {
            foreach (string _s in line.Split(' ')) {
                string s = _s;

                if (s.Contains("(")) {
                    s = s.Remove(s.IndexOf("("));
                    
                    if (s.StartsWith("*")) {
                        s = s.Substring(1);
                    }

                    return s;
                }
            }

            return null;
        }
        
        public static string ReadNotWhitespace(string prompt) {
            string ret = "";

            while (string.IsNullOrWhiteSpace(ret)) {
                Console.Write(prompt);
                ret = Console.ReadLine();
            }

            return ret;
        }

        public static bool GetPointersForSymbol(string name, out ulong startPtr, out ulong endPtr) {
            name = name.ToLowerInvariant();

            StreamReader sr = new StreamReader("sm64.us.map");
            
            startPtr = 0;
            endPtr = 0;
            
            while (!sr.EndOfStream) {
                string[] words = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                ulong ptr = 0;

                foreach (string str in words) {
                    if (str.StartsWith("0x")) {
                        try
                        {
                            ptr = Math.Max(ptr, Convert.ToUInt64(str.Substring(2), 16));
                        }
                        catch {}
                    }
                }

                if (ptr != 0) {
                    if (startPtr == 0) { 
                        if (words.Any(w => w.ToLowerInvariant() == name)) {
                            startPtr = ptr;
                        }
                    }
                    else if (ptr > startPtr) {
                        endPtr = ptr;
                        break;
                    }
                }
            }
            sr.Close();
            
            return endPtr > 0;
        }

        public static void AutomatedBhvTroll(string dirPath, string outputName, IEnumerable<string> triggers) {
            Dictionary<string, FunctionData> functions = new Dictionary<string, FunctionData>();

            foreach (string file in Directory.GetFiles(dirPath)) {
                string fname = Path.GetFileName(file);
                if (fname.EndsWith(".c")) {
                    string[] lines = File.ReadAllLines(file);

                    string methodName = null;
                    string currFunction = "";
                    for (int i = 0; i < lines.Length; i++) {
                        if (methodName == null) {
                            methodName = ExtractMethodName(lines[i]);
                            if (string.IsNullOrWhiteSpace(methodName)) methodName = null;
                            if (methodName != null) {
                                if (lines[i].IndexOf("(") < lines[i].IndexOf(")") && (
                                    lines[i].EndsWith("{") || (i < lines.Length - 1 && lines[i + 1].EndsWith("{")))
                                ) {
                                    currFunction += lines[i] + "\n";
                                }
                                else {
                                    methodName = null;
                                }
                            }
                        }
                        else {
                            currFunction += lines[i] + "\n";

                            if (lines[i].StartsWith("}")) {
                                if (!functions.TryAdd(methodName, new FunctionData { fileName = fname, methodName = methodName, function = currFunction })) {
                                    Console.WriteLine($"Duplicate function found with name {methodName} in {fname}!");
                                }
                                methodName = null;
                                currFunction = "";
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Loaded {functions.Count} methods, time for the real trolling to begin");
            Directory.CreateDirectory($"output/{outputName}");

            if (!File.Exists($"output/{outputName}/{outputName}_headers.h")) {
                File.Create($"output/{outputName}/{outputName}_headers.h").Close();
            }
            StreamWriter asmWriter = new StreamWriter($"output/{outputName}/{outputName}.asm");
            foreach (var trol in functions) {
                string methodName = trol.Key;
                bool add = false;

                foreach (string trigger in triggers) {
                    if (trol.Value.function.Contains(trigger)) {
                        add = true;
                        break;
                    }
                }

                if (!add) continue;

                if (!functions.TryGetValue(methodName, out FunctionData func)) {
                    Console.WriteLine($"I did a skill issue and couldn't extract {methodName}");
                    continue;
                }

                StreamWriter cWriter = new StreamWriter($"output/{outputName}/{func.fileName}__{func.methodName}.c");
                cWriter.Write($"#include \"{outputName}_headers.h\"\n\n\n");
                cWriter.WriteLine(func.function);
                cWriter.Close();

                Console.WriteLine($"Generated {func.fileName}__{func.methodName}.c");

                if (GetPointersForSymbol(func.methodName, out ulong startPtr, out ulong endPtr)) {
                    asmWriter.WriteLine($".org 0x{startPtr.ToString("X8")}");
                    asmWriter.WriteLine($".area 0x{endPtr.ToString("X8")}-0x{startPtr.ToString("X8")}");
                    asmWriter.WriteLine($".importobj \"{outputName}/{func.fileName}__{func.methodName}.c.o\"");
                    asmWriter.WriteLine($".endarea");
                    asmWriter.WriteLine();
                    Console.WriteLine($"Added importobj to {outputName}.asm");
                }
                else {
                    asmWriter.WriteLine($".org 0x");
                    asmWriter.WriteLine($".area 0x-0x");
                    asmWriter.WriteLine($".importobj \"{outputName}/{func.fileName}__{func.methodName}.c.o\"");
                    asmWriter.WriteLine($".endarea");
                    asmWriter.WriteLine();
                    Console.WriteLine($"Failed to get start and end pointers from sm64.us.map, added an incomplete importobj to {outputName}.asm");
                }
            }
            asmWriter.Close();
        }

        static void Main(string[] args) {
            if (!File.Exists("sm64.us.map")) {
                Console.WriteLine("I require a vanilla sm64.us.map");
                return;
            }
            
            if (Progame.Progaming()) {
                return;
            }
            if (Trollge.Trolle()) {
                return;
            }

            string dirPath = ReadNotWhitespace("Directory of .c files: ");
            if (!Directory.Exists(dirPath)) {
                Console.WriteLine("That directory does not exist!");
                return;
            }
            Console.WriteLine("Holy shit the directory is real");
            string outputName = ReadNotWhitespace("Output subdirectory name: ");

            Dictionary<string, FunctionData> functions = new Dictionary<string, FunctionData>();

            foreach (string file in Directory.GetFiles(dirPath)) {
                string fname = Path.GetFileName(file);
                if (fname.EndsWith(".c")) {
                    string[] lines = File.ReadAllLines(file);

                    string methodName = null;
                    string currFunction = "";
                    for (int i = 0; i < lines.Length; i++) {
                        if (methodName == null) {
                            methodName = ExtractMethodName(lines[i]);
                            if (string.IsNullOrWhiteSpace(methodName)) methodName = null;
                            if (methodName != null) {
                                if (lines[i].IndexOf("(") < lines[i].IndexOf(")") && (
                                    lines[i].EndsWith("{") || (i < lines.Length - 1 && lines[i + 1].EndsWith("{")))
                                ) {
                                    currFunction += lines[i] + "\n";
                                }
                                else {
                                    methodName = null;
                                }
                            }
                        }
                        else {
                            currFunction += lines[i] + "\n";

                            if (lines[i].StartsWith("}")) {
                                if (!functions.TryAdd(methodName, new FunctionData { fileName = fname, methodName = methodName, function = currFunction })) {
                                    Console.WriteLine($"Duplicate function found with name {methodName} in {fname}!");
                                }
                                methodName = null;
                                currFunction = "";
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Loaded {functions.Count} methods, time for the real trolling to begin");
            Directory.CreateDirectory($"output/{outputName}");
            Console.WriteLine($"Type method names and I'll extract the funny methods to a .c file. End the list with YAHA");

            if (!File.Exists($"output/{outputName}/{outputName}_headers.h")) {
                File.Create($"output/{outputName}/{outputName}_headers.h").Close();
            }
            StreamWriter asmWriter = new StreamWriter($"output/{outputName}/{outputName}.asm");
            while (true) {
                string methodName = Console.ReadLine();
                if (methodName == "YAHA") {
                    break;
                }

                if (!functions.TryGetValue(methodName, out FunctionData func)) {
                    Console.WriteLine($"I did a skill issue and couldn't extract {methodName}");
                    continue;
                }

                StreamWriter cWriter = new StreamWriter($"output/{outputName}/{func.fileName}__{func.methodName}.c");
                cWriter.Write($"#include \"{outputName}_headers.h\"\n\n\n");
                cWriter.WriteLine(func.function);
                cWriter.Close();

                Console.WriteLine($"Generated {func.fileName}__{func.methodName}.c");

                if (GetPointersForSymbol(func.methodName, out ulong startPtr, out ulong endPtr)) {
                    asmWriter.WriteLine($".org 0x{startPtr.ToString("X8")}");
                    asmWriter.WriteLine($".area 0x{endPtr.ToString("X8")}-0x{startPtr.ToString("X8")}");
                    asmWriter.WriteLine($".importobj \"{outputName}/{func.fileName}__{func.methodName}.c.o\"");
                    asmWriter.WriteLine($".endarea");
                    asmWriter.WriteLine();
                    Console.WriteLine($"Added importobj to {outputName}.asm");
                }
                else {
                    asmWriter.WriteLine($".org 0x");
                    asmWriter.WriteLine($".area 0x-0x");
                    asmWriter.WriteLine($".importobj \"{outputName}/{func.fileName}__{func.methodName}.c.o\"");
                    asmWriter.WriteLine($".endarea");
                    asmWriter.WriteLine();
                    Console.WriteLine($"Failed to get start and end pointers from sm64.us.map, added an incomplete importobj to {outputName}.asm");
                }
            }
            asmWriter.Close();
            
            Console.WriteLine($"Troll complete, returning to HQ");
            Console.WriteLine($"Oh yeah you gotta fill up output/{outputName}/{outputName}_headers.h and tweak the .asm to get it to work");
            Console.WriteLine($"that's literally it though lol");
            Console.WriteLine($"enjoyment");
        }
    }
}
