using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VersusMod
{
    /// <summary>
    /// Wrapper class that coroutines can apply on IEnumerators to get rid of
    /// certain one-frame delays when stepping in / out of the new routine.
    /// </summary>
    public class SwapImmediately : IEnumerator
    {

        public IEnumerator Inner;

        public object Current => Inner.Current;

        public SwapImmediately(IEnumerator inner)
        {
            Inner = inner;
        }

        public bool MoveNext()
        {
            return Inner.MoveNext();
        }

        public void Reset()
        {
            Inner.Reset();
        }

    }
}
