using System;
using System.Collections.Generic;
using System.Text;

namespace KapheinSharp.Text
{
    public static class Utils
    {
        public static string Join<T>(
            string separator
            , IEnumerable<T> values
        )
        {
            if(values == null) {
                throw new ArgumentNullException("values");
            }
            
            var sb = new StringBuilder();

            var enumerator = values.GetEnumerator();
            for(var isFirst = true; enumerator.MoveNext(); ) {
                if(!isFirst) {
                    sb.Append(separator);
                }

                sb.Append(enumerator.Current);
                
                isFirst = false;
            }

            return sb.ToString();
        }
    }
}
