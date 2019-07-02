using System;
using System.Text;

namespace KapheinSharp.Net
{
    public class IpV4Header
    {
        public const int MinimumHeaderLength = (5 << 2);

        public const int MaximumFragmentOffset = (1 << 13) - 1;

        public const int MaximumPayloadLength = (1 << 16) - 1;

        public IpV4Header()
        {
            versionAndHeaderLength_ = 0x45;
            typeOfService_ = 0;
            totalLength_ = (ushort)HeaderLength;
            fragmentId_ = 0;
            fragmentFlagsAndFragmentOffset_ = 0;
            timeToLive_ = 0;
            protocol_ = 0;
            checksum_ = 0;
            srcAddress_ = new byte[sizeof(UInt32)];
            destAddress_ = new byte[sizeof(UInt32)];
            options_ = new byte[0];
        }
        
        public IpV4Header(
            IpV4Header src
        )
            : this()
        {
            if(src == null) {
                throw new ArgumentNullException("src");
            }

            versionAndHeaderLength_ = src.versionAndHeaderLength_;
            typeOfService_ = src.typeOfService_;
            totalLength_ = src.totalLength_;
            fragmentId_ = src.fragmentId_;
            fragmentFlagsAndFragmentOffset_ = src.fragmentFlagsAndFragmentOffset_;
            timeToLive_ = src.timeToLive_;
            protocol_ = src.protocol_;
            checksum_ = src.checksum_;
            SourceAddress = src.SourceAddress;
            DestinationAddress = src.DestinationAddress;
            Options = src.Options;
        }

        public int Version
        {
            get
            {
                return ((versionAndHeaderLength_ & 0xF0) >> 4);
            }
        }

        public int HeaderLength
        {
            get
            {
                return (versionAndHeaderLength_ & 0xF) << 2;
            }
        }

        public int OptionLength
        {
            get
            {
                return HeaderLength - IpV4Header.MinimumHeaderLength;
            }
        }

        public Byte TypeOfService
        {
            get
            {
                return typeOfService_;
            }
        }

        public int TotalLength
        {
            get
            {
                return totalLength_;
            }
        }

        public int PayloadLength
        {
            get
            {
                return totalLength_ - HeaderLength;
            }
        }

        public UInt16 FragmentId
        {
            get
            {
                return fragmentId_;
            }
        }

        public bool IsFragmented
        {
            get
            {
                var result = false;

                switch(FragmentFlags) {
                case 0x00:
                    result = FragmentOffset > 0;
                break;
                case 0x01:
                    result = true;
                break;
                case 0x02:
                break;
                }

                return result;
            }
        }

        public Byte FragmentFlags
        {
            get
            {
                return (Byte)((fragmentFlagsAndFragmentOffset_ & 0xE000) >> 13);
            }
        }

        public UInt16 FragmentOffset
        {
            get
            {
                return (UInt16)(fragmentFlagsAndFragmentOffset_ & 0x1FFF);
            }
        }

        public int TimeToLive
        {
            get
            {
                return timeToLive_;
            }

            set
            {
                if(value < byte.MinValue || value > byte.MaxValue) {
                    throw new ArgumentOutOfRangeException("value");
                }

                timeToLive_ = (byte)(value & 0xFF);
            }
        }

        public int Protocol
        {
            get
            {
                return protocol_;
            }

            set
            {
                if(value < byte.MinValue || value > byte.MaxValue) {
                    throw new ArgumentOutOfRangeException("value");
                }

                protocol_ = (byte)(value & 0xFF);
            }
        }

        public UInt16 Checksum
        {
            get
            {
                return checksum_;
            }
        }

        public byte[] SourceAddress
        {
            get
            {
                var result = new byte[srcAddress_.Length];

                Buffer.BlockCopy(srcAddress_, 0, result, 0, result.Length);

                return result;
            }

            set
            {
                if(value == null) {
                    for(int i = srcAddress_.Length; i > 0; ) {
                        --i;
                        srcAddress_[i] = 0;
                    }
                }
                else {
                    if(value.Length != srcAddress_.Length) {
                        throw new ArgumentException();
                    }

                    Buffer.BlockCopy(value, 0, srcAddress_, 0, srcAddress_.Length);
                }
            }
        }

        public byte[] DestinationAddress
        {
            get
            {
                var result = new byte[destAddress_.Length];

                Buffer.BlockCopy(destAddress_, 0, result, 0, result.Length);

                return result;
            }

            set
            {
                if(value == null) {
                    for(int i = destAddress_.Length; i > 0; ) {
                        --i;
                        destAddress_[i] = 0;
                    }
                }
                else {
                    if(value.Length != destAddress_.Length) {
                        throw new ArgumentException();
                    }

                    Buffer.BlockCopy(value, 0, destAddress_, 0, destAddress_.Length);
                }
            }
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
                var optionLength = OptionLength;
                if(options_ == null || options_.Length != optionLength) {
                    options_ = new byte[optionLength];
                }

                if(value == null) {
                    for(int i = options_.Length; i > 0; ) {
                        --i;
                        options_[i] = 0;
                    }
                }
                else {
                    if(value.Length != options_.Length) {
                        throw new ArgumentException();
                    }

                    Buffer.BlockCopy(value, 0, options_, 0, optionLength);
                }
            }
        }

        public byte[] UniqueFragmentId
        {
            get
            {
                var uid = new byte[(sizeof(UInt32) << 1) + sizeof(Byte) + sizeof(UInt16)];

                int destOffset = 0;

                Buffer.BlockCopy(srcAddress_, 0, uid, destOffset, sizeof(UInt32));
                destOffset += sizeof(UInt32);

                Buffer.BlockCopy(destAddress_, 0, uid, destOffset, sizeof(UInt32));
                destOffset += sizeof(UInt32);

                uid[destOffset++] = protocol_;

                uid[destOffset++] = (Byte)((fragmentId_ & 0xFF00) >> 8);
                uid[destOffset++] = (Byte)(fragmentId_ & 0x00FF);

                return uid;
            }
        }

        public int Unserialize(
            byte[] bytes
            , int offset
            , int count
        )
        {
            int current = offset;

            versionAndHeaderLength_ = bytes[current++];
            typeOfService_ = bytes[current++];
            totalLength_ = BitUtils.ToUInt16(bytes, current); current += 2;
            
            fragmentId_ = BitUtils.ToUInt16(bytes, current); current += 2;
            fragmentFlagsAndFragmentOffset_ = BitUtils.ToUInt16(bytes, current); current += 2;

            timeToLive_ = bytes[current++];
            protocol_ = bytes[current++];
            checksum_ = BitUtils.ToUInt16(bytes, current); current += 2;

            Buffer.BlockCopy(bytes, current, srcAddress_, 0, 4); current += 4;
            Buffer.BlockCopy(bytes, current, destAddress_, 0, 4); current += 4;

            var optionLength = OptionLength;
            if(count - current >= optionLength) {
                Options = null;
                Buffer.BlockCopy(bytes, current, options_, 0, optionLength);
                current += optionLength;
            }

            return current;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            sb.AppendFormat("\"{0}\":{1}", "ver", Version);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "hdrLen", HeaderLength);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "tos", TypeOfService);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "totalLen", TotalLength);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "id", FragmentId);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "fragFlags", FragmentFlags);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "fragOff", FragmentOffset);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "ttl", TimeToLive);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "proto", Protocol);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "chk", Checksum);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":\"{1}\"", "src", KapheinSharp.Text.Utils.Join(".", SourceAddress));
            sb.Append(',');
            sb.AppendFormat("\"{0}\":\"{1}\"", "dest", KapheinSharp.Text.Utils.Join(".", DestinationAddress));
            sb.Append(',');
            sb.AppendFormat(
                "\"{0}\":[{1}]", "options"
                , KapheinSharp.Text.Utils.Join(",", Options)
            );
            sb.Append('}');

            return sb.ToString();
        }

        private Byte versionAndHeaderLength_;

        private Byte typeOfService_;

        private UInt16 totalLength_;

        private UInt16 fragmentId_;

        private UInt16 fragmentFlagsAndFragmentOffset_;

        private Byte timeToLive_;

        private Byte protocol_;

        private UInt16 checksum_;

        private byte[] srcAddress_;

        private byte[] destAddress_;

        private byte[] options_;
    }
}
