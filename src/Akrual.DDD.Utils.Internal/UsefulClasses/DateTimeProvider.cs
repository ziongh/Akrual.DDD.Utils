using System;
using System.Collections.Generic;
using System.Text;

namespace Akrual.DDD.Utils.Internal.UsefulClasses
{
    public abstract class DateTimeProvider
    {
        private static DateTimeProvider current;

        static DateTimeProvider()
        {
            DateTimeProvider.current = new DefaultDateTimeProvider();
        }

        public static DateTimeProvider Current
        {
            get { return DateTimeProvider.current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                DateTimeProvider.current = value;
            }
        }

        public abstract DateTime UtcNow { get; }

        public abstract DateTime Now { get; }

        public static void ResetToDefault()
        {
            DateTimeProvider.current = new DefaultDateTimeProvider();
        }
    }

    public class DefaultDateTimeProvider : DateTimeProvider
    {
        public override DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        public override DateTime Now
        {
            get { return DateTime.Now; }
        }
    }


    public class FakeDateTimeProvider : DateTimeProvider
    {
        private DateTime _dateToReturn;
        public FakeDateTimeProvider(DateTime dateToReturn)
        {
            _dateToReturn = dateToReturn;
        }

        public override DateTime UtcNow
        {
            get { return _dateToReturn; }
        }

        public override DateTime Now
        {
            get { return _dateToReturn; }
        }
    }
}
