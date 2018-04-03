using System;
using System.Collections.Generic;
using System.Text;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace Akrual.DDD.Utils.Internal.UsefulClasses
{
    public class ScopedLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        public Lifestyle SelectLifestyle(Type implementationType)
        {
            return Lifestyle.Scoped;
        }
    }
}
