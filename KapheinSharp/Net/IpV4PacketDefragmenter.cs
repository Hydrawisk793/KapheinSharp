using KapheinSharp.Collection;
using KapheinSharp.Math;
using System;
using System.Collections.Generic;

namespace KapheinSharp.Net
{
    /// <summary>
    /// TODO : Test this class.
    /// </summary>
    public class IpV4PacketDefragmenter
    {
        public class HoleList
        {
            public HoleList(
                IntervalInt32 range
            )
            {
                range_ = range;
                filledArea_ = new SortedList<Int32, Int32>();
            }

            public IntervalInt32 Range
            {
                get
                {
                    return range_;
                }
            }

            public IntervalInt32[] FilledIntervals
            {
                get
                {
                    var intervals = new List<IntervalInt32>();

                    foreach(var pair in filledArea_) {
                        intervals.Add(new IntervalInt32(pair.Key, pair.Value));
                    }

                    return intervals.ToArray();
                }
            }

            public bool HasNoMoreHoles
            {
                get
                {
                    var result = false;

                    if(filledArea_.Count == 1) {
                        var minimum = filledArea_.Keys[0];
                        var maximum = filledArea_[minimum];

                        result = new IntervalInt32(minimum, maximum).Equals(range_);
                    }
                    
                    return result;
                }
            }

            public void Fill(
                IntervalInt32 interval
            )
            {
                if(!range_.Contains(interval)) {
                    throw new ArgumentOutOfRangeException("");
                }
                
                var shouldAddNewInterval = true;

                IntervalInt32 ltInterval = default(IntervalInt32);
                Int32 ltIntervalMin;
                var isLtIntervalMinFound = filledArea_.FindKeyLessThenOrEqualTo(interval.Minimum, out ltIntervalMin);
                if(isLtIntervalMinFound) {
                    var ltIntervalMax = filledArea_[ltIntervalMin];
                    ltInterval = new IntervalInt32(ltIntervalMin, ltIntervalMax);
                    if(ltInterval.IntersectsWith(interval)) {
                        //Error! Discard all information of the original packet.
                        throw new ArgumentOutOfRangeException("");
                    }

                    if(interval.Minimum - ltInterval.Maximum == 1) {
                        filledArea_[ltInterval.Minimum] = interval.Maximum;
                        interval = new IntervalInt32(ltInterval.Minimum, interval.Maximum);

                        shouldAddNewInterval = false;
                    }
                 }
                
                IntervalInt32 gtInterval = default(IntervalInt32);
                Int32 gtIntervalMin;
                var isGtIntervalMinFound = filledArea_.FindKeyGreaterThenOrEqualTo(interval.Maximum, out gtIntervalMin);
                if(isGtIntervalMinFound) {
                    var gtIntervalMax = filledArea_[gtIntervalMin];
                    gtInterval = new IntervalInt32(gtIntervalMin, gtIntervalMax);
                    if(gtInterval.IntersectsWith(interval)) {
                        //Error! Discard all information of the original packet.
                        throw new ArgumentOutOfRangeException("");
                    }

                    if(gtInterval.Minimum - interval.Maximum == 1) {
                        filledArea_.Remove(interval.Minimum);
                        filledArea_.Remove(gtInterval.Minimum);
                        filledArea_.Add(interval.Minimum, gtInterval.Maximum);

                        shouldAddNewInterval = false;
                    }
                }

                if(shouldAddNewInterval) {
                    filledArea_.Add(interval.Minimum, interval.Maximum);
                }
            }

            public void Clear()
            {
                filledArea_.Clear();
            }
            
            private IntervalInt32 range_;

            private SortedList<Int32, Int32> filledArea_;
        }
        
        public class Agent
        {
            public struct PayloadPair
            {
                public PayloadPair(
                    IntervalInt32 offsetInterval
                    , byte[] payload
                )
                {
                    OffsetInterval = offsetInterval;
                    Payload = payload;
                }
                
                public IntervalInt32 OffsetInterval
                {
                    get; set;
                }

                public byte[] Payload
                {
                    get; set;
                }
            }
            
            public Agent()
            {
                firstFragmentHeader_ = null;
                lastFragmentHeader_ = null;
                holeList_ = new HoleList(new IntervalInt32(0, IpV4Header.MaximumPayloadLength));
                payloadMap_ = new SortedList<UInt16, byte[]>();

                Reset();
            }

            public IpV4Packet Consume(
                IpV4Packet packet
            )
            {
                IpV4Packet mergedPacket = null;
                
                if(packet == null) {
                    throw new ArgumentNullException("packet");
                }

                var header = packet.Header;

                var isNotFragmented = false;

                var offsetInterval = default(IntervalInt32);
                switch(header.FragmentFlags) {
                case 0x00:
                    if(header.FragmentOffset > 0) {
                        if(lastFragmentHeader_ != null) {
                            throw new Exception("Duplicated last fragment!");
                        }
                        lastFragmentHeader_ = packet.Header;

                        offsetInterval = new IntervalInt32(header.FragmentOffset, holeList_.Range.Maximum);
                    }
                    else {
                        isNotFragmented = true;
                    }
                break;
                case 0x01:
                    if(header.FragmentOffset == 0) {
                        if(firstFragmentHeader_ != null) {
                            throw new Exception("Duplicated first fragment!");
                        }
                        else {
                            firstFragmentHeader_ = packet.Header;
                        }
                    }

                    if((header.PayloadLength & ((1 << 3) - 1)) != 0) {
                        throw new FormatException("The length of non-final fragments must be multiple of 8.");
                    }

                    offsetInterval = new IntervalInt32(header.FragmentOffset, (header.PayloadLength > 0 ? header.PayloadLength - 1 : 0));
                break;
                case 0x02:
                    isNotFragmented = true;
                break;
                default:
                    throw new FormatException("The fragment flag has an invalid value.");
                }
                
                if(isNotFragmented) {
                    mergedPacket = packet;
                    Reset();
                }
                else {
                    try {
                        holeList_.Fill(offsetInterval);
                        payloadMap_.Add(header.FragmentOffset, packet.Payload);

                        if(holeList_.HasNoMoreHoles) {
                            if(lastFragmentHeader_ == null || firstFragmentHeader_ == null) {
                                throw new FormatException("");
                            }
                            
                            var mergedPayloadLength = lastFragmentHeader_.FragmentOffset + lastFragmentHeader_.PayloadLength;
                            var mergedPayloadBuffer = new ByteBuffer(lastFragmentHeader_.FragmentOffset + lastFragmentHeader_.PayloadLength);
                            foreach(var pair in payloadMap_) {
                                mergedPayloadBuffer.Enqueue(pair.Value);
                            }
                            if(mergedPayloadBuffer.Count != mergedPayloadLength) {
                                throw new FormatException("");
                            }

                            mergedPacket = new IpV4Packet(header, mergedPayloadBuffer.Dequeue());
                            
                            Reset();
                        }
                    }
                    catch(Exception e) {
                        Reset();
                        
                        throw e;
                    }
                }

                return mergedPacket;
            }

            public void Reset()
            {
                firstFragmentHeader_ = null;
                lastFragmentHeader_ = null;
                holeList_.Clear();
            }

            private static int InsertIfNotExistAndSort<T>(
                List<T> list
                , T o
                , IComparer<T> comparer
            )
            {
                var result = true;
                var loop = true;

                int i = 0;
                while(loop && i < list.Count) {
                    var cp = comparer.Compare(list[i], o);
                    if(cp == 0) {
                        result = false;
                        loop = false;
                    }
                    else if(cp > 0) {
                        list.Insert(i, o);
                        loop = false;
                    }
                    else {
                        ++i;
                    }
                }

                if(loop) {
                    list.Add(o);
                }

                return (result ? i : -1);
            }
            
            private static int CompareIntervalsForSort(
                IntervalInt32 lhs
                , IntervalInt32 rhs
            )
            {
                var diff = lhs.Minimum - rhs.Minimum;
                if(diff != 0) {
                    return diff;
                }

                return lhs.Maximum - rhs.Maximum;
            }
            
            private IpV4Header firstFragmentHeader_;
            
            private IpV4Header lastFragmentHeader_;
            
            private HoleList holeList_;

            private SortedList<UInt16, byte[]> payloadMap_;
        }
        
        public IpV4PacketDefragmenter()
        {
            agentMap_ = new SortedList<byte[], Agent>(
                new FunctionToComparaerAdapter<byte[]>((lhs, rhs) => {
                    var lhsLen = lhs.Length;
                    var rhsLen = rhs.Length;
                    var diff = 0;
                    var minLen = lhsLen;
                    var minLenTermIndex = 2;

                    if(lhsLen < rhsLen) {
                        minLenTermIndex = 0;
                        minLen = lhsLen;
                    }
                    else if(rhsLen < lhsLen) {
                        minLenTermIndex = 1;
                        minLen = rhsLen;
                    }

                    for(int i = 0; i < minLen; ++i) {
                        diff = lhs[i] - rhs[i];
                        if(diff != 0) {
                            break;
                        }
                    }

                    if(diff == 0) {
                        switch(minLenTermIndex) {
                        case 0:
                            diff = -rhs[minLen - 1];
                        break;
                        case 1:
                            diff = lhs[minLen - 1];
                        break;
                        }
                    }

                    return diff;
                })
            );
        }

        public IpV4Packet Defrag(
            IpV4Packet ipPacket
        )
        {
            var result = ipPacket;

            var header = ipPacket.Header;
            if(header.IsFragmented) {
                var ufid = ipPacket.Header.UniqueFragmentId;

                var agent = CreateOrGetAgent(ufid);
                result = agent.Consume(ipPacket);

                if(result != null) {
                    agentMap_.Remove(ufid);
                }
            }

            return result;
        }

        private Agent CreateOrGetAgent(
            byte[] ufid
        )
        {
            Agent agent = null;
            
            if(!agentMap_.ContainsKey(ufid)) {
                agent = new Agent();
                agentMap_[ufid] = agent;
            }
            else {
                agent = agentMap_[ufid];
            }

            return agent;
        }

        private SortedList<byte[], Agent> agentMap_;
    }
}
