using System;

namespace KapheinSharp
{
    public class ByteArrayProperty
    {
        public ByteArrayProperty()
        {
            value_ = null;
        }

        public ByteArrayProperty(
            ByteArrayProperty src
        )
            : this(src.value_, 0, src.value_.Length)
        {
            
        }

        public ByteArrayProperty(
            int length
        )
        {
            value_ = new byte[length];
        }

        public ByteArrayProperty(
            byte[] src
        )
            : this(src, 0, src.Length)
        {
            
        }

        public ByteArrayProperty(
            byte[] src
            , int offset
            , int count
        )
        {
            if(src == null) {
                value_ = null;
            }
            else {
                value_ = new byte[count];

                Buffer.BlockCopy(src, offset, value_, 0, count);
            }
        }

        public int Length
        {
            get
            {
                return (value_ == null ? 0 : value_.Length);
            }
        }

        public byte[] Value
        {
            get
            {
                byte[] result = null;
                
                if(value_ != null) {
                    result = new byte[value_.Length];

                    Buffer.BlockCopy(value_, 0, result, 0, result.Length);
                }

                return result;
            }

            set
            {
                if(value == null) {
                    value_ = null;
                }
                else {
                    if(value.Length != value_.Length) {
                        value_ = new byte[value.Length];
                    }

                    Buffer.BlockCopy(value, 0, value_, 0, value_.Length);
                }
            }
        }

        public void Get(
            byte[] dest
            , int destOffset
            , int srcOffset
            , int count
        )
        {
            if(value_ != null) {
                Buffer.BlockCopy(value_, srcOffset, dest, destOffset, count);
            }
        }

        public void Set(
            byte[] src
            , int srcOffset
            , int destOffset
            , int count
        )
        {
            if(value_ != null) {
                Buffer.BlockCopy(src, srcOffset, value_, destOffset, count);
            }
        }

        private byte[] value_;
    }
}
