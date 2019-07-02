using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KapheinSharp
{
    public class FunctionToComparaerAdapter<T>
        : IComparer<T>
    {
        public FunctionToComparaerAdapter(
            Func<T, T, int> func
        )
        {
            if(func == null) {
                throw new ArgumentNullException("null");
            }
            
            func_ = func;
        }

        public FunctionToComparaerAdapter(
            FunctionToComparaerAdapter<T> src
        )
            : this(src.func_)
        {
            
        }

        public int Compare(
            T x
            , T y
        )
        {
            return func_.Invoke(x, y);
        }
        
        private Func<T, T, int> func_;
    }
}
