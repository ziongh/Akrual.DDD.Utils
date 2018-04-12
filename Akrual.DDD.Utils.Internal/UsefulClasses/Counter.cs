using System.Threading;

namespace Akrual.DDD.Utils.Internal.UsefulClasses
{
    // seal the class as it's not designed to be inherited from.
    public sealed class Counter
    {
        // use a meaningful name, 'i' by convention should only be used in a for loop.
        private int current = 0;

        // update the method name to imply that it returns something.
        public int NextValue()
        {
            // prefix fields with 'this'
            return Interlocked.Increment(ref this.current);
        }

        public void Reset()
        {
            this.current = 0;
        }
    }
}
