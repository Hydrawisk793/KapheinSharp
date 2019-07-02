using System;

namespace KapheinSharp
{
    public static class BitUtils
    {
        public static ushort ToUInt16(
            byte[] values
            , int offset
        )
        {
            ushort v;
            
            if(BitConverter.IsLittleEndian) {
                v = BitConverter.ToUInt16(new byte[] {values[offset + 1], values[offset]}, 0);
            }
            else {
                v = BitConverter.ToUInt16(values, offset);
            }

            return v;
        }

        public static uint ToUInt32(
            byte[] values
            , int offset
        )
        {
            uint v;
            
            if(BitConverter.IsLittleEndian) {
                v = BitConverter.ToUInt32(new byte[] {values[offset + 3], values[offset + 2], values[offset + 1], values[offset]}, 0);
            }
            else {
                v = BitConverter.ToUInt32(values, offset);
            }

            return v;
        }
    }
}
