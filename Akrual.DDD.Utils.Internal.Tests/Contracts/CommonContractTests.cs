using Akrual.DDD.Utils.Internal.Contracts;
using Akrual.DDD.Utils.Internal.Exceptions;
using Xunit;

namespace Akrual.DDD.Utils.Internal.Tests.Contracts
{
    public class CommonContractTests : BaseTests
    {
        [Fact]
        public void EnsureNotNull_WhenNull_ReturnException()
        {
            TestingClass variable = null;
            Assert.Throws<ContractExceptionWithProperty>(() => variable.EnsuresNotNull());
        }

        private class TestingClass
        {
        }
    }
}
