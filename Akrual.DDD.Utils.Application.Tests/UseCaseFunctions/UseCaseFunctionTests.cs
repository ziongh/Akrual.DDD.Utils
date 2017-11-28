using Akrual.DDD.Utils.Application.Messaging;
using Akrual.DDD.Utils.Application.UseCaseFunctions;
using Xunit;

namespace Akrual.DDD.Utils.Application.Tests.UseCaseFunctions
{
    public class UseCaseFunctionTests
    {
        [Fact]
        public void Execute_WithSampleModels_ReturnSampleModel()
        {
            var usecaseServicer = new ExampleUseCaseFunction();
            var inputmodel = new ExampleInputModel();

            var output = usecaseServicer.Execute(inputmodel);

            Assert.IsType<ExampleOutputModel>(output);
        }

        protected internal class ExampleInputModel : InputModel<ExampleInputModel>
        {
            
        }

        protected internal class ExampleOutputModel : OutputModel<ExampleOutputModel>
        {

        }

        protected internal class ExampleUseCaseFunction : IUseCaseFunction<ExampleInputModel, ExampleOutputModel>
        {
            public ExampleOutputModel Execute(ExampleInputModel input)
            {
                return new ExampleOutputModel();
            }
        }
    }
}
