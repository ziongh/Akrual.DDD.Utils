using Akrual.DDD.Utils.Application.Messaging;
using Akrual.DDD.Utils.Application.UseCaseFunctions;
using FluentValidation;
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

        protected internal class ExampleUseCaseFunction : BaseUseCaseFunction<ExampleInputModel, ExampleOutputModel>
        {
            protected override AbstractValidator<ExampleInputModel> PreConditionEvaluator { get; }
            protected override AbstractValidator<ExampleOutputModel> PostConditionEvaluator { get; }

            protected override ExampleOutputModel WhatToExecute(ExampleInputModel input)
            {
                return new ExampleOutputModel();
            }
        }
    }
}
