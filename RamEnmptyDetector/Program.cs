using System;
using System.IO;

namespace RamEnmptyDetector {
    class Program {
        static void Main(string[] args) {
            byte[] bytes = File.ReadAllBytes("ram.bin");

            int nullStart = -1;
            int i = 0;
            for (; i < bytes.Length; i++) {
                if (bytes[i] == 0) {
                    if (nullStart == -1)
                        nullStart = i;
                }
                else if (nullStart != -1) {
                    if (nullStart < i - 0x100) {
                        Console.WriteLine($"Enmpty: 0x{nullStart:X8}-0x{i - 1:X8} - size 0x{i - nullStart:X}");
                    }

                    nullStart = -1;
                }
            }

            if (nullStart != -1) {
                if (nullStart < i - 0x100) {
                    Console.WriteLine($"Enmpty: 0x{nullStart:X8}-0x{i - 1:X8} - size 0x{i - nullStart:X}");
                }

                nullStart = -1;
            }
        }
    }
}