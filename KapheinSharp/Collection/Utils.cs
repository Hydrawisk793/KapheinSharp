using System;
using System.Collections.Generic;

namespace KapheinSharp.Collection
{
    public static class Utils
    {
        public static void Fill(
            this byte[] arr
            , byte value
        )
        {
            if(arr == null) {
                throw new ArgumentNullException("arr");
            }

            for(int i = arr.Length; i > 0; ) {
                --i;
                arr[i] = value;
            }
        }
        
        public static bool Equals(
            this byte[] lhs
            , byte[] rhs
        )
        {
            if(lhs == null) {
                throw new ArgumentNullException("lhs");
            }

            if(rhs == null) {
                throw new ArgumentNullException("rhs");
            }
            
            var lhsLen = lhs.Length;
            var rhsLen = rhs.Length;
            var result = lhsLen == rhsLen;

            if(result) {
                for(int i = lhsLen; i > 0; ) {
                    --i;
                    if(lhs[i] != rhs[i]) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        public static bool FindKeyLessThenOrEqualTo<TKey, TValue>(
            this SortedList<TKey, TValue> dict
            , TKey key
            , out TKey foundKey
        )
        {
            var comparaer = dict.Comparer;
            
            int foundKeyIndex = -1;
            var keys = dict.Keys;
            for(int i = keys.Count; i > 0; ) {
                --i;

                if(comparaer.Compare(keys[i], key) <= 0) {
                    foundKeyIndex = i;
                    break;
                }
            }

            if(foundKeyIndex >= 0) {
                foundKey = keys[foundKeyIndex];
            }
            else {
                foundKey = default(TKey);
            }
            
            return foundKeyIndex >= 0;
        }

        public static bool FindKeyGreaterThenOrEqualTo<TKey, TValue>(
            this SortedList<TKey, TValue> dict
            , TKey key
            , out TKey foundKey
        )
        {
            var comparaer = dict.Comparer;
            
            int foundKeyIndex = -1;
            var keys = dict.Keys;
            for(int i = 0; i < keys.Count; ++i) {
                if(comparaer.Compare(keys[i], key) >= 0) {
                    foundKeyIndex = i;
                    break;
                }
            }

            if(foundKeyIndex >= 0) {
                foundKey = keys[foundKeyIndex];
            }
            else {
                foundKey = default(TKey);
            }

            return foundKeyIndex >= 0;
        }
    }
}
