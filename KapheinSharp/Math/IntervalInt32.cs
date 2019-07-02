using System;
using System.Collections.Generic;
using System.Text;

namespace KapheinSharp.Math
{
    public struct IntervalInt32
    {
        public static IntervalInt32[] Disjoin(
            IEnumerable<IntervalInt32> intervals
            , bool mergePoints = false
        )
        {
            var disjoinedIntervals = new List<IntervalInt32>();
            
            int j = 0, sortedPointMaxIndex = 0, endOfClosureIndex = 0;
            var sortedPoints = new List<Int32>();
            var sortedListSet = CreateSortedIntervalListSet(intervals);
            for(int i = 0, len = sortedListSet.Count; i < len; ) {
                j = 0;

                endOfClosureIndex = FindEndOfClosureIndex(sortedListSet, i);
                sortedPoints.Clear();
                for(j = i; j < endOfClosureIndex; ++j) {
                    var neighbor = sortedListSet[j];
                    InsertIfNotExistAndSort(
                        sortedPoints
                        , neighbor.min_
                        , new FunctionToComparaerAdapter<Int32>(CompareInt32)
                    );
                    InsertIfNotExistAndSort(
                        sortedPoints
                        , neighbor.max_
                        , new FunctionToComparaerAdapter<Int32>(CompareInt32)
                    );
                }

                sortedPointMaxIndex = sortedPoints.Count - 1;
                if(mergePoints) {
                    disjoinedIntervals.Add(new IntervalInt32(sortedPoints[0], sortedPoints[sortedPointMaxIndex]));
                }
                else {
                    j = 0;
                    do {
                        disjoinedIntervals.Add(new IntervalInt32(sortedPoints[j], sortedPoints[j + 1]));
                        ++j;
                    }
                    while(j < sortedPointMaxIndex);
                }

                i = endOfClosureIndex;
            }
            
            return disjoinedIntervals.ToArray();
        }
        
        public static IntervalInt32[] Merge(
            IEnumerable<IntervalInt32> intervals
        )
        {
            throw new NotImplementedException();
        }
        
        public static IntervalInt32[] Negate(
            IEnumerable<IntervalInt32> intervals
        )
        {
            throw new NotImplementedException();
        }

        public static IntervalInt32[] FindClosure(
            IEnumerable<IntervalInt32> intervals
        )
        {
            throw new NotImplementedException();
        }

        public IntervalInt32(
            Int32 start
            , Int32 end
        )
        {
            if(end < start) {
                throw new ArgumentOutOfRangeException("end < start");
            }

            min_ = start;
            max_ = end;
        }

        public Int32 Minimum
        {
            get
            {
                return min_;
            }
        }

        public Int32 Maximum
        {
            get
            {
                return max_;
            }
        }

        public Int32 Length
        {
            get
            {
                return max_ - min_;
            }
        }

        public bool IntersectsWith(
            IntervalInt32 rhs
        )
        {
            var result = false;
            
            if(min_ < rhs.min_) {
                result = min_ <= rhs.max_ && max_ >= rhs.min_;
            }
            else {
                result = rhs.min_ <= max_ && rhs.max_ >= min_;
            }

            return result;
        }

        public bool Contains(
            Int32 value
        )
        {
            return value >= min_ && value <= max_;
        }

        public bool Contains(
            IntervalInt32 rhs
        )
        {
            var result = false;
            
            if(Length >= rhs.Length) {
                result = Contains(rhs.min_)
                    && Contains(rhs.max_)
                ;
            }
            else {
                result = rhs.Contains(min_)
                    && rhs.Contains(max_)
                ;
            }

            return result;
        }

        public IntervalInt32[] Split(
            Int32 pivot
        )
        {
            if(!Contains(pivot)) {
                throw new ArgumentOutOfRangeException();
            }

            return new IntervalInt32[] {
                new IntervalInt32(min_, pivot)
                , new IntervalInt32(pivot, max_)
            };
        }

        public IntervalInt32[] Split(
            IntervalInt32 pivot
        )
        {
            if(!Contains(pivot)) {
                throw new ArgumentException();
            }

            return new IntervalInt32[] {
                new IntervalInt32(min_, pivot.max_)
                , new IntervalInt32(pivot.min_, max_)
            };
        }

        public override bool Equals(
            object obj
        )
        {
            var result = obj is IntervalInt32;

            if(result) {
                var rhs = (IntervalInt32)obj;

                result = min_ == rhs.min_
                    && max_ == rhs.max_
                ;
            }

            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ min_ ^ max_;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('{');
            sb.AppendFormat("\"{0}\":{1}", "min", min_);
            sb.Append(',');
            sb.AppendFormat("\"{0}\":{1}", "max", max_);
            sb.Append('}');

            return sb.ToString();
        }

        private static int FindEndOfClosureIndex(
            List<IntervalInt32> sortedListSet
            , int startIndex
        )
        {
            var endOfClosureIndex = startIndex + 1;

            for(
                int i = startIndex, len = sortedListSet.Count;
                i < endOfClosureIndex && i < len;
                ++i
            ) {
                var current = sortedListSet[i];

                var loopJ = true;
                var endOfNeighborIndex = i + 1;
                for(var j = endOfNeighborIndex; loopJ && j < len; ) {
                    var other = sortedListSet[j];

                    if(current.max_ < other.min_) {
                        endOfNeighborIndex = j;
                        loopJ = false;
                    }
                    else {
                        ++j;
                    }
                }
                if(loopJ) {
                    endOfNeighborIndex = len;
                }

                endOfClosureIndex = (
                    endOfClosureIndex < endOfNeighborIndex
                    ? endOfNeighborIndex
                    : endOfClosureIndex
                );
            }

            return endOfClosureIndex;
        }

        private static List<IntervalInt32> CreateSortedIntervalListSet(
            IEnumerable<IntervalInt32> intervals
        )
        {
            var sortedIntervals = new List<IntervalInt32>();
            foreach(var interval in intervals) {
                InsertIfNotExistAndSort(
                    sortedIntervals
                    , interval
                    , new FunctionToComparaerAdapter<IntervalInt32>(CompareIntervalsForSort)
                );
            }

            return sortedIntervals;
        }

        private static bool InsertIfNotExistAndSort<T>(
            List<T> list
            , T o
            , IComparer<T> comparer
        )
        {
            var result = true;
            var loop = true;

            for(var i = 0; loop && i < list.Count; ) {
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

            return result;
        }
        
        private static int CompareIntervalsForSort(
            IntervalInt32 lhs
            , IntervalInt32 rhs
        )
        {
            var diff = lhs.min_ - rhs.min_;

            return (diff == 0 ? (lhs.Equals(rhs) ? 0 : -1) : diff);
        }
        
        private static int CompareInt32(
            Int32 lhs
            , Int32 rhs
        )
        {
            return lhs - rhs;
        }

        private Int32 min_;

        private Int32 max_;
    }
}
