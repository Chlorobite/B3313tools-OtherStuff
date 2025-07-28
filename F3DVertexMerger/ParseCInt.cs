using System;

namespace F3DVertexMerger {
    public static class ParseCInt {
        public static int Parse(string str) {
            bool hex = false;
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
                str = str.Substring(2);
                hex = true;
            }

            return Convert.ToInt32(str, hex ? 16 : 10);
        }
    }
}
