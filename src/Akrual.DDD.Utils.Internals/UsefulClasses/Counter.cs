using System.Threading;

namespace Akrual.DDD.Utils.Internal.UsefulClasses
{
    // seal the class as it's not designed to be inherited from.
    public sealed class Counter
    {
        public Counter(int initialCounter = 0)
        {
            current = initialCounter;
        }

        private int current = 0;

        public int NextValue()
        {
            return Interlocked.Increment(ref this.current);
        }
        public int GetCurrentValue()
        {
            return this.current;
        }

        public void Reset()
        {
            this.current = 0;
        }
    }
}
