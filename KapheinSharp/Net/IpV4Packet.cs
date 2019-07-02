using System;
using System.Text;

namespace KapheinSharp.Net
{
    public class IpV4Packet
    {
        public IpV4Packet()
        {
            header_ = new IpV4Header();
            payload_ = new byte[0];
        }
        
        public IpV4Packet(
            IpV4Header header
            , byte[] payload
        )
        {
            header_ = new IpV4Header(header);
            payload_ = payload;
        }

        public IpV4Header Header
        {
            get
            {
                return new IpV4Header(header_);
            }

            set
            {
                if(value == null) {
                    header_ = new IpV4Header();
                }
                else {
                    header_ = new IpV4Header(value);
                }
            }
        }

        public byte[] Payload
        {
            get
            {
                var result = new byte[payload_.Length];
                Buffer.BlockCopy(payload_, 0, result, 0, payload_.Length);

                return result;
            }

            set
            {
                int payloadLength = 0;
                if(value != null) {
                    payloadLength = value.Length;
                }
                
                if(payload_ == null || payload_.Length != payloadLength) {
                    payload_ = new byte[payloadLength];
                }

                if(value == null) {
                    for(int i = payloadLength; i > 0; ) {
                        --i;
                        payload_[i] = 0;
                    }
                }
                else {
                    Buffer.BlockCopy(value, 0, payload_, 0, payloadLength);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('{');
            sb.AppendFormat("\"{0}\":{1}", "header", header_);
            sb.Append(',');
            sb.AppendFormat(
                "\"{0}\":[{1}]"
                , "payload"
                , KapheinSharp.Text.Utils.Join(",", Payload)//Payload.Select((v) => {return string.Format("0x{0:X2}", v);}))
            );
            sb.Append('}');

            return sb.ToString();
        }

        private IpV4Header header_;

        private byte[] payload_;
    }
}
