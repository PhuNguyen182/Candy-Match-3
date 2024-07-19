using System;

namespace GlobalScripts.Utils
{
    public static class NumericUtils
    {
        public static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] LongToByte(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static int BytesToInt(byte[] code)
        {
            return BitConverter.ToInt32(code, 0);
        }

        public static long BytesToLong(byte[] code)
        {
            return BitConverter.ToInt64(code, 0);
        }
    }
}
