using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArmipsTrollGenerator {
    class Progame {
        public static bool Progaming() {
            Console.WriteLine("WTF ??? SECRET SEX MODE ???");
            Console.WriteLine("Type YAHAAA for SECRET SEX MODE!!!");
            if (Console.ReadLine() != "YAHAAA") return false;

            Console.WriteLine("SECRET SEX MODE UNLOCKED WTF");
            Console.WriteLine("Enter armips error output that includes 'Undefined external symbol'-s");

            HashSet<string> foundSymbols = new HashSet<string>();
            while (true) {
                string ln = Console.ReadLine();

                int i = ln.IndexOf("Undefined external symbol");
                if (i != -1) {
                    ln = ln.Substring(i + "Undefined external symbol".Length);
                    string symbol = ln.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                    if (foundSymbols.Add(symbol)) {
                        if (Program.GetPointersForSymbol(symbol, out ulong startPtr, out ulong endPtr)) {
                            Console.WriteLine($".definelabel {symbol}, 0x{startPtr.ToString("X8")}");
                        }
                        else {
                            Console.WriteLine($".definelabel {symbol}, ");
                        }
                    }
                }

                if (ln == "Aborting.") return true;
            }

            return true;
        }
    }
}