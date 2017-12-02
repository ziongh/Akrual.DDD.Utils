using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Akrual.DDD.Utils.Internal.Extensions;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using Xunit;
using Xunit.Sdk;

namespace Akrual.DDD.Domain.Tests.Utils
{
    /// <inheritdoc />
    public class AssertExtensions : Xunit.Assert
    {
        /// <summary>
        /// Compare each and every puplic property of the Entity
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AllPublicPropertyEqual(object expected, object actual)
        {
            var diffs = new List<DiferentProperty>();
            actual.PublicInstancePropertiesEqual(expected, ref diffs);

            if (diffs.Any())
            {
                var expectedString = string.Join(",", diffs.Select(s => "{" + s.PropertyExpectedValue + "}").ToArray());
                var actualString = string.Join(",", diffs.Select(s => "{" + s.PropertyActualValue + "}").ToArray());
                throw new NotEqualException(expectedString, actualString);
            }
        }
    }
}
