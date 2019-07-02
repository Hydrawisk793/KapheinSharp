using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace KapheinSharp.Collection
{
    public class ByteBuffer
        : IEnumerable<byte>
    {
        public class Enumerator
            : IEnumerator<byte>
        {
            internal Enumerator(
                ByteBuffer buffer
            )
            {
                if(buffer == null) {
                    throw new ArgumentNullException("buffer");
                }
                
                buffer_ = buffer;

                Reset();
            }

            public bool IsDisposed
            {
                get
                {
                    return buffer_ == null;
                }
            }

            public void Dispose()
            {
                buffer_ = null;

                current_ = -1;
            }
            
            public byte Current
            {
                get
                {
                    return buffer_.bytes_[current_];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                var next = current_ + 1;
                var len = buffer_.bytes_.Length;

                if(next >= len) {
                    next -= len;
                }

                var hasNext = next != buffer_.in_;
                if(hasNext) {
                    current_ = next;
                }

                return hasNext;
            }

            public void Reset()
            {
                current_ = buffer_.out_ - 1;
            }
            
            private ByteBuffer buffer_;

            private int current_;
        }

        public ByteBuffer()
            : this(7)
        {
            
        }

        public ByteBuffer(
            ByteBuffer src
        )
        {
            if(src == null) {
                throw new ArgumentNullException("src");
            }

            bytes_ = new byte[src.bytes_.Length];
            Buffer.BlockCopy(src.bytes_, 0, bytes_, 0, bytes_.Length);
            in_ = src.in_;
            out_ = src.out_;
            isAutoExpansionEnabled_ = src.isAutoExpansionEnabled_;
        }
        
        public ByteBuffer(
            int capacity
        )
            : this(capacity, false)
        {
            
        }

        public ByteBuffer(
            int capacity
            , bool isAutoExpansionEnabled
        )
        {
            if(capacity < 0) {
                throw new ArgumentOutOfRangeException("count");
            }

            bytes_ = new byte[capacity + 1];
            in_ = 0;
            out_ = 0;
            isAutoExpansionEnabled_ = isAutoExpansionEnabled;
        }

        public int Capacity
        {
            get
            {
                return bytes_.Length - 1;
            }
        }

        public int Count
        {
            get
            {
                var diff = in_ - out_;

                return (
                    diff >= 0
                    ? diff
                    : bytes_.Length + diff
                );
            }
        }

        public int Available
        {
            get
            {
                return Capacity - Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return in_ == out_;
            }
        }

        public bool IsFull
        {
            get
            {
                return GetNextPosition(in_, 1) == out_;
            }
        }

        public int InputIndex
        {
            get
            {
                return in_;
            }
        }

        public int OutputIndex
        {
            get
            {
                return out_;
            }
        }
        
        public bool IsAutoExpansionEnabled
        {
            get
            {
                return isAutoExpansionEnabled_;
            }

            set
            {
                isAutoExpansionEnabled_ = value;
            }
        }

        public int Expand()
        {
            var byteArraySize = bytes_.Length;
            
            if(byteArraySize < int.MaxValue) {
                if(byteArraySize < (int.MaxValue >> 1)) {
                    byteArraySize <<= 1;
                }
                else {
                    byteArraySize = int.MaxValue - 1;
                }

                var existingBytes = ToByteArray();
                bytes_ = new byte[byteArraySize];
                Buffer.BlockCopy(existingBytes, 0, bytes_, 0, existingBytes.Length);
                in_ = existingBytes.Length;
                out_ = 0;
            }

            return byteArraySize - 1;
        }

        public int Enqueue(
            byte[] bytes
        )
        {
            return Enqueue(bytes, 0, bytes.Length);
        }

        public int Enqueue(
            byte[] src
            , int srcStart
            , int srcSize
        )
        {
            if(src == null) {
                throw new ArgumentNullException("src");
            }

            if(srcStart < 0) {
                throw new ArgumentOutOfRangeException("srcStart");
            }

            if(srcSize < 0 || srcSize > src.Length + srcStart) {
                throw new ArgumentOutOfRangeException("srcSize");
            }

            while(isAutoExpansionEnabled_ && Available < srcSize) {
                Expand();
            }

            var frontCount = System.Math.Min(srcSize, bytes_.Length - in_);
            var rearCount = System.Math.Min(srcSize - frontCount, out_);
            int actualCount = frontCount + rearCount;

            Buffer.BlockCopy(src, srcStart, bytes_, in_, frontCount);
            Buffer.BlockCopy(src, srcStart + frontCount, bytes_, 0, rearCount);

            in_ += frontCount;
            if(in_ >= bytes_.Length) {
                in_ = rearCount;
            }

            return actualCount;
        }

        public byte[] Dequeue()
        {
            return Dequeue(Count);
        }

        public byte[] Dequeue(
            int count
        )
        {
            var bytes = new byte[count];
            byte[] resultBytes = bytes;
            
            int actualCount = Dequeue(bytes, 0, bytes.Length);

            if(actualCount != count) {
                var actualBytes = new byte[actualCount];
                Buffer.BlockCopy(bytes, 0, actualBytes, 0, actualCount);
                resultBytes = actualBytes;
            }

            return resultBytes;
        }

        public int Dequeue(
            byte[] dest
        )
        {
            return Dequeue(dest, 0, dest.Length);
        }

        public int Dequeue(
            byte[] dest
            , int destStart
            , int destSize
        )
        {
            if(dest == null) {
                throw new ArgumentNullException("dest");
            }

            if(destStart < 0) {
                throw new ArgumentOutOfRangeException("destStart");
            }

            if(destSize < 0 || destSize > dest.Length + destStart) {
                throw new ArgumentOutOfRangeException("destSize");
            }
            
            var frontCount = System.Math.Min(destSize, bytes_.Length - out_);
            var rearCount = System.Math.Min(destSize - frontCount, in_);
            int actualCount = frontCount + rearCount;

            Buffer.BlockCopy(bytes_, out_, dest, destStart, frontCount);
            //kaphein_mem_fill(out_, frontCount, 0xFF, frontCount);
            Buffer.BlockCopy(bytes_, 0, dest, destStart + frontCount, rearCount);
            //kaphein_mem_fill(thisObj->byteArray_ + frontCount, rearCount, 0xFF, rearCount);

            out_ += frontCount;
            if(out_ >= bytes_.Length) {
                out_ = rearCount;
            }

            return actualCount;
        }

        public void Clear()
        {
            in_ = 0;
            out_ = 0;
        }

        private int GetNextPosition(
            int position
            , int increment
        )
        {
            position += increment;
            while(position >= bytes_.Length) {
                position -= bytes_.Length;
            }

            return position;
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return ToString(16);
        }

        public byte[] ToByteArray()
        {
            int i = 0;
            var byteArray = new byte[Count];

            foreach(var value in this) {
                byteArray[i++] = value;
            }

            return byteArray;
        }

        public string ToString(
            int radix
        )
        {
            var sb = new StringBuilder();
            
            sb.Append('[');

            int count = 0;

            foreach(var value in this) {
                if(count > 0) {
                    sb.Append(',');
                }
                
                switch(radix) {
                case 16:
                    sb.Append(string.Format("{0:X2}", value));
                break;
                default:
                    throw new NotSupportedException();
                //break;
                }

                ++count;
            }
            
            sb.Append(']');

            return sb.ToString();
        }

        private byte[] bytes_;

        private int in_;

        private int out_;

        private bool isAutoExpansionEnabled_;
    }
}
