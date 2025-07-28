using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;

namespace ApplyRMTweak {
  class Tweak {
    public uint pointer;
    public byte[] bytesToWrite;

    public Tweak(string str) {
      string[] words = str.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);

      if (words.Length <= 1) {
        throw new Exception("bad, just bad.");
      }

      bytesToWrite = new byte[words.Length - 1];
      try {
        pointer = Convert.ToUInt32(words[0].Trim(':'), 16);

        for (int i = 1; i < words.Length; i++) {
          bytesToWrite[i - 1] = Convert.ToByte(words[i], 16);
        }
      }
      catch (Exception e) {
        throw new Exception("bad number\n" + e);
      }
    }

    public void Apply(byte[] file) {
      for (int i = 0; i < bytesToWrite.Length && pointer + i < file.Length; i++) {
        file[pointer + i] = bytesToWrite[i];
      }
    }

    public byte? GetByteAt(uint ptr) {
      if (ptr < pointer || ptr >= pointer + bytesToWrite.Length) return null;

      return bytesToWrite[ptr - pointer];
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append($"{pointer.ToString("X")}:");
      foreach (byte b in bytesToWrite) {
        sb.Append(" " + b.ToString("X2"));
      }

      return sb.ToString();
    }

    public bool TryMergeWith(Tweak nextTweak) {
      uint nPointer = Math.Min(pointer, nextTweak.pointer);
      uint end = Math.Max(pointer + (uint)bytesToWrite.Length, nextTweak.pointer + (uint)nextTweak.bytesToWrite.Length);
      List<byte> nBytesToWrite = new List<byte>();

      for (uint i = nPointer; i < end; i++) {
        byte? b1 = GetByteAt(i);
        byte? b2 = nextTweak.GetByteAt(i);

        if (b1 == null && b2 == null) return false;

        nBytesToWrite.Add(b2 ?? b1.Value);
      }

      pointer = nPointer;
      bytesToWrite = nBytesToWrite.ToArray();
      return true;
    }
  }

  class MainClass {
    public static void Main(string[] args) {
      if (args.Length < 2) {
        Console.WriteLine("ApplyRMTweak.exe <ROM file> <funny binary tweak string>");
        return;
      }

      string fname = args[0];
      string tweakString = args[1];

      if (!File.Exists(fname)) {
        if (File.Exists(tweakString)) {
          Console.WriteLine("The ROM file provided does not exist! Sorting the tweaks");

          List<Tweak> script = new List<Tweak>();
          int ln = 0;
          try {
            foreach (string _str in File.ReadAllText(tweakString).Split('\n')) {
              ln++;
              string str = _str.Trim();
              if (!string.IsNullOrEmpty(str)) {
                Tweak tweak = new Tweak(str);
                script.Add(tweak);
              }
            }
          }
          catch (Exception e) {
            Console.WriteLine("At line " + ln + ": ");
            throw e;
          }

          Console.WriteLine("Loaded " + script.Count + " tweaks");

          for (int i = 0; i < script.Count; i++) {
            Tweak t1 = script[i];
            for (int j = i + 1; j < script.Count; j++) {
              if (t1.TryMergeWith(script[j])) {
                script.RemoveAt(j--);
              }
            }
          }

          script = script.OrderBy(t => t.pointer).ToList();

          foreach (Tweak t in script) {
            Console.WriteLine(t);
          }

          return;
        }

        Console.WriteLine("The ROM file provided does not exist! Yey yey yey");
        return;
      }
      byte[] bytes = File.ReadAllBytes(fname);

      if (File.Exists(tweakString)) {
        int ln = 0;
        try {
          foreach (string _str in File.ReadAllText(tweakString).Split('\n')) {
            ln++;
            string str = _str.Trim();
            if (!string.IsNullOrEmpty(str)) {
              Tweak tweak = new Tweak(str);
              tweak.Apply(bytes);
            }
          }
        }
        catch (Exception e) {
          Console.WriteLine("At line " + ln + ": ");
          throw e;
        }
      }
      else {
        Tweak tweak = new Tweak(tweakString);
        tweak.Apply(bytes);
      }

      File.WriteAllBytes(fname, bytes);
      Console.WriteLine("Patch applied successfully! Updating checksum with rn64crc");
      Process p = Process.Start("wine", $"rn64crc.exe \"{fname}\" -u");
      p.WaitForExit();
    }
  }
}
