using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace hexsplitting {
  class MainClass {
    static void HexSplitting() {
      while (true) {
        string[] hex = Console.ReadLine().Split(' ');
        foreach (string s in hex) {
          //if (int.TryParse(s, out int _a)) {
          //  short a = (short)_a;
          //  Console.Write(a.ToString("X4").Substring(0, 2) + " " + a.ToString("X4").Substring(2, 2) + " ");
          //}
          if (s.Length == 8) {
            Console.Write(s.Substring(0, 2) + " " + s.Substring(2, 2) + " " + s.Substring(4, 2) + " " + s.Substring(6, 2) + " ");
          }
          else if (s.Length == 4) {
            //Console.Write(s.Substring(0, 2) + " " + s.Substring(2, 2) + " ");
            Console.Write(s.Substring(2, 2) + " " + s.Substring(0, 2) + " ");
          }
        }
        Console.WriteLine();
      }
    }

    static void BinaryDiff() {
      Console.WriteLine("Save output to file [stdout]: ");
      string outfile = Console.ReadLine();
      StreamWriter sw = outfile == "" ? (StreamWriter)Console.Out : new StreamWriter(outfile);

      string fname1 = "test_35.z64";
      string fname2 = "test_36.z64";

      byte[] bytes1 = File.ReadAllBytes(fname1);
      byte[] bytes2 = File.ReadAllBytes(fname2);
      int diffStart = -1;

      int start = 0;
      int end = Math.Min(bytes1.Length, bytes2.Length);
      //int start = 0xFD410;
      //int end = 0xFD800;

      for (int i = start; i < end - 3; i += 4) {
        uint i1 = BitConverter.ToUInt32(bytes1, i);
        uint i2 = BitConverter.ToUInt32(bytes2, i);

        if (i1 != i2) {
          if (diffStart == -1) {
            diffStart = i;
          }
        }
        else if (diffStart != -1) {
          sw.WriteLine($"Diff ({diffStart.ToString("X8")} - {i.ToString("X8")}):");
          sw.Write($"{fname1} ");
          for (int j = diffStart; j < i; j += 4) {
            uint _i = (uint)((bytes1[j] << 24) + (bytes1[j + 1] << 16) + (bytes1[j + 2] << 8) + (bytes1[j + 3]));
            sw.Write(_i.ToString("X8") + " ");
          }
          sw.Write($"\n{fname2} ");
          for (int j = diffStart; j < i; j += 4) {
            uint _i = (uint)((bytes2[j] << 24) + (bytes2[j + 1] << 16) + (bytes2[j + 2] << 8) + (bytes2[j + 3]));
            sw.Write(_i.ToString("X8") + " ");
          }
          sw.WriteLine();
          /*Console.Write($"{diffStart.ToString("X2")}: ");
          for (int j = diffStart; j < i; j++) {
            Console.Write(bytes2[j].ToString("X2") + " ");
          }
          Console.WriteLine();*/
          diffStart = -1;
        }
      }

      if (outfile != "") {
        sw.Close();
      }
      Console.ReadKey(true);
    }

    static void SearchAndReplace() {
      Console.WriteLine("Save output to file [stdout]: ");
      string outfile = Console.ReadLine();
      StreamWriter sw = outfile == "" ? (StreamWriter)Console.Out : new StreamWriter(outfile);

      string fname = "with object.z64";

      //byte[] searchBytes = { 0xB7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x03, 0x86, 0x00, 0x10, 0x03, 0x00, 0x9D, 0x00,
      //                       0x03, 0x88, 0x00, 0x10, 0x03, 0x00, 0x9C, 0xF8, 0x06, 0x01, 0x00, 0x00, 0x03, 0x01, 0x48, 0x88 };
      byte[] searchBytes = { 0x17, 0x0C, 0x00, 0x13, 0x00, 0x21, 0x9E, 0x00, 0x00, 0x21, 0xF4, 0xC0 };
      int offset = 0;
      string newDataTrol = "17 0C 00 13 03 01 D2 90 03 02 2C 30";

      byte[] bytes = File.ReadAllBytes(fname);
      for (int i = 0; i < bytes.Length - searchBytes.Length + 1; i++) {
        bool foundReelBeytah = true;
        for (int j = 0; j < searchBytes.Length; j++) {
          if (searchBytes[j] != bytes[i + j]) {
            foundReelBeytah = false;
            break;
          }
        }

        if (foundReelBeytah) {
          sw.WriteLine((i + offset).ToString("X2") + ": " + newDataTrol);
        }
      }

      if (outfile != "") {
        sw.Close();
      }
      Console.ReadKey(true);
    }

    static void BinToTweak() {
      Console.WriteLine("Save output to file [stdout]: ");
      string outfile = Console.ReadLine();
      StreamWriter sw = outfile == "" ? (StreamWriter)Console.Out : new StreamWriter(outfile);

      string fname = "group7.bin";

      byte[] bytes = File.ReadAllBytes(fname);
      int[] binranges = { 0x79FC, 0x7DCC + 0x18, 0x87D8, 0x8B5C + 0x18, 0x7708, 0x79E4 + 0x18, 0x7DE4, 0x87C0 + 0x18 };
      int segromstart = 0x8FB8B0;

      for (int _i = 0; _i < binranges.Length - 1; _i += 2) {
        int start = binranges[_i];
        int end = binranges[_i + 1];

        sw.Write((segromstart + start).ToString("X2") + ": ");
        for (int i = start; i < end; i++) {
          sw.Write(bytes[i].ToString("X2") + " ");
        }
        sw.WriteLine();
      }

      if (outfile != "") {
        sw.Close();
      }
      Console.ReadKey(true);
    }

    static void VertexTroll() {
            string[] vertex = {
              "    {{{  -6750, Y_1, 1322}, 0, {    0,    0}, {0xff, 0x80, 0x80, 0xff}}},",
              "    {{{  -6750, Y_1, 1642}, 0, {  -990,    0}, {0xff, 0x80, 0x80, 0xff}}},",
              "    {{{  -6750, Y_2, 1322}, 0, {    0,  -990}, {0xff, 0x80, 0x80, 0xff}}},",
              "    {{{  -6750, Y_2, 1642}, 0, {  -990,  -990}, {0xff, 0x80, 0x80, 0xff}}},"
            };

            int y1 = 1826;
            int y2 = 1820;
            int writes = 0;

            while (y2 >= 1586) {
              foreach (string str in vertex) {
                Console.WriteLine(str.Replace("Y_1", y1.ToString()).Replace("Y_2", y2.ToString()));
                if (++writes >= 16) {
                  Console.WriteLine();
                  writes -= 16;
                }
              }

              y1 -= 6;
              y2 -= 6;
            }
      /*string[] dl = {
        "    gsDPSetTextureImage(G_IM_FMT_RGBA, G_IM_SIZ_16b, 1, 0x0038f800 + FB_I),\n    gsDPLoadSync(),\n    gsDPLoadBlock(G_TX_LOADTILE, 0, 0, 320 * 6 - 1, CALC_DXT(320, G_IM_SIZ_16b_BYTES)),\n    gsSPVertex(lgtvVtx + VTX_I, 4, 0),\n    gsSP2Triangles( 0,  1,  2, 0x0,  1,  3,  2, 0x0),",
      };


      for (int i = 0; i < 240/6; i++) {
        foreach (string str in dl) {
          Console.WriteLine(str.Replace("FB_I", (i * 320 * 6).ToString()).Replace("VTX_I", (i * 4).ToString()));
        }
        Console.WriteLine();
      }*/
    }

    static void DefineLabelTroll() {
      List<string[]> strings = new List<string[]>();
      while (true) {
        string[] str = Console.ReadLine().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

        if (str.Length > 1) {
          strings.Add(str);
        }
        else {
          foreach (string[] _str in strings) {
            Console.WriteLine($".definelabel {_str[1]}, {_str[0].Replace("0x00000000", "0x")}");
          }
          strings.Clear();
        }
      }
    }

    public static void Main(string[] args) {
      Console.WriteLine("Choose a gamemode");
      Console.WriteLine("0: hexsplitting");
      Console.WriteLine("1: binary diff");
      Console.WriteLine("2: search and replace (outputs a tweak)");
      Console.WriteLine("3: binary file ranges to tweak");
      Console.WriteLine("4: vertex troll");
      Console.WriteLine("5: define label troll");
      while (true) {
        switch (Console.ReadKey(true).KeyChar) {
          case '0':
            HexSplitting();
            break;
          case '1':
            BinaryDiff();
            break;
          case '2':
            SearchAndReplace();
            break;
          case '3':
            BinToTweak();
            break;
          case '4':
            VertexTroll();
            break;
          case '5':
            DefineLabelTroll();
            break;
        }
      }

      // misc utilities idk if to keep

      /*StreamReader sr = new StreamReader("all 7 letter words.txt");
      StreamWriter sw = new StreamWriter("half1.txt");
      string s = "bcdfghjklm";
      while (!sr.EndOfStream) {
        string str = sr.ReadLine();
        if (str.Length == 7 && str[2] == 't' && str[5] == 'u' && s.Contains(str[0].ToString())) {
          sw.WriteLine(str);
        }
      }
      sr.Close();
      sw.Close();*/

      /*int startAddress = 0xA86A24;
      int vertexCount = 74;
      string newColorTrol = "FF FF FF FF";

      int address = startAddress;
      for (int i = 0; i < vertexCount; i++) {
        Console.WriteLine(address.ToString("X2") + ": " + newColorTrol);
        address += 0x10;
      }*/
    }
  }
}
