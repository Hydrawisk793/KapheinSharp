using System;

namespace KapheinSharp.Net
{
    public class TcpHeader
    {
        public TcpHeader()
        {
            srcPort_ = 0;
            destPort_ = 0;
            seq_ = 0;
            ack_ = 0;
            flags1_ = 0;
            flags2_ = 0;
            windowSize_ = 0;
            checksum_ = 0;
            urgentPointer_ = 0;
            Options = null;
        }

        public TcpHeader(
            TcpHeader src
        )
        {
            if(src == null) {
                throw new ArgumentNullException("src");
            }

            srcPort_ = src.srcPort_;
            destPort_ = src.destPort_;
            seq_ = src.seq_;
            ack_ = src.ack_;
            flags1_ = src.flags1_;
            flags2_ = src.flags2_;
            windowSize_ = src.windowSize_;
            checksum_ = src.checksum_;
            urgentPointer_ = src.urgentPointer_;
            Options = src.Options;
        }

        public int Unserialize(
            byte[] bytes
            , int offset
            , int count
        )
        {
            int current = offset;

            srcPort_ = BitUtils.ToUInt16(bytes, current); current += 2;
            destPort_ = BitUtils.ToUInt16(bytes, current); current += 2;

            seq_ = BitUtils.ToUInt32(bytes, current); current += 4;

            ack_ = BitUtils.ToUInt32(bytes, current); current += 4;

            flags1_ = bytes[current++];
            flags2_ = bytes[current++];
            windowSize_ = BitUtils.ToUInt16(bytes, current); current += 2;

            checksum_ = BitUtils.ToUInt16(bytes, current); current += 2;
            urgentPointer_ = BitUtils.ToUInt16(bytes, current); current += 2;

            //TODO : Write some proper codes...
            //var optionLength = OptionLength;
            //if(count - current >= optionLength) {
            //    Options = null;
            //    Buffer.BlockCopy(bytes, current, options_, 0, optionLength);
            //    current += optionLength;
            //}

            return current;
        }
        
        public byte[] Options
        {
            get
            {
                var result = new byte[options_.Length];

                Buffer.BlockCopy(options_, 0, result, 0, result.Length);

                return result;
            }

            set
            {
                var optionsLength = 0;
                if(value != null) {
                    optionsLength = value.Length;
                }
                
                if(options_ == null) {
                    options_ = new byte[optionsLength];
                }

                if(value == null) {
                    for(int i = optionsLength; i > 0; ) {
                        --i;
                        options_[i] = 0;
                    }
                }
                else {
                    Buffer.BlockCopy(value, 0, options_, 0, optionsLength);
                }
            }
        }

        private UInt16 srcPort_;

        private UInt16 destPort_;

        private UInt32 seq_;

        private UInt32 ack_;

        private Byte flags1_;
        
        private Byte flags2_;
        
        private UInt16 windowSize_;

        private UInt16 checksum_;

        private UInt16 urgentPointer_;

        private byte[] options_;
    }
}
