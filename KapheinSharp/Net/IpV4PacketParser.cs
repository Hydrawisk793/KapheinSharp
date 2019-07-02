using KapheinSharp.Collection;
using System;
using System.Collections.Generic;

namespace KapheinSharp.Net
{
    public class IpV4PacketParser
    {
        public IpV4PacketParser()
        {
            state_ = 0;
            buffer_ = new ByteBuffer(32, true);
            header_ = new IpV4Header();
        }

        public List<IpV4Packet> Consume(
            byte[] bytes
            , int offset
            , int count
        )
        {
            var slicedBytes = new byte[count];
            Buffer.BlockCopy(bytes, offset, slicedBytes, 0, slicedBytes.Length);
            
            return Consume(slicedBytes);
        }

        public List<IpV4Packet> Consume(
            byte[] bytes
        )
        {
            var payloads = new List<IpV4Packet>();

            if(bytes == null) {
                throw new ArgumentNullException("bytes");
            }

            buffer_.Enqueue(bytes);

            for(bool isLooping = true; isLooping && !buffer_.IsEmpty; ) {
                switch(state_) {
                case 0:
                    if(buffer_.Count >= IpV4Header.MinimumHeaderLength) {
                        var ipHeaderBytes = buffer_.Dequeue(IpV4Header.MinimumHeaderLength);
                        header_.Unserialize(ipHeaderBytes, 0, ipHeaderBytes.Length);

                        if(header_.OptionLength > 0) {
                            state_ = 1;
                        }
                        else {
                            state_ = 2;
                        }
                    }
                    else {
                        isLooping = false;
                    }
                break;
                case 1:
                    if(buffer_.Count >= header_.OptionLength) {
                        var options = buffer_.Dequeue(header_.OptionLength);
                        header_.Options = options;

                        state_ = 2;
                    }
                    else {
                        isLooping = false;
                    }
                break;
                case 2:
                    if(buffer_.Count >= header_.PayloadLength) {
                        var payload = buffer_.Dequeue(header_.PayloadLength);
                        payloads.Add(new IpV4Packet(header_, payload));

                        state_ = 0;
                    }
                    else {
                        isLooping = false;
                    }
                break;
                }
            }

            return payloads;
        }

        private int state_;

        private IpV4Header header_;
        
        private ByteBuffer buffer_;
    }
}
