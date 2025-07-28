using System;
using System.IO;
using System.Text;
using System.Threading;

namespace dmalog {
  class MainClass {
    public static void Main(string[] args) {
      StreamReader sr = new StreamReader("dmalog.csv");
      BinaryReader br = new BinaryReader(File.OpenRead("test.z64"));
      Thread.Sleep(6969);
      uint startTimestamp = 0x0;
      DateTime start = DateTime.UtcNow;

      while (!sr.EndOfStream) {
        string[] line = sr.ReadLine().Split(',');
        uint[] data = new uint[line.Length];

        if (line.Length >= 4) {
          bool bad = false;
          for (int i = 0; i < line.Length; i++) {
            try {
              data[i] = Convert.ToUInt32(line[i].Substring(2), 16);
            }
            catch {
              bad = true;
              break;
            }
          }

          if (!bad) {
            uint timestamp = data[3];
            if (startTimestamp == 0) {
              startTimestamp = timestamp;
              start = DateTime.UtcNow;
            }
            else {
              uint now;
              do {
                now = (uint)(startTimestamp + DateTime.UtcNow.Subtract(start).TotalMilliseconds);
                Thread.Sleep(1);
              }
              while (now < timestamp);
            }

            br.BaseStream.Seek(data[0], SeekOrigin.Begin);
            byte[] bytes = br.ReadBytes((int)data[2]);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) {
              sb.Append(b.ToString("X2"));
            }
            Console.WriteLine(data[0].ToString("X8") + ": " + sb.ToString());
          }
        }
      }
    }
  }
}

