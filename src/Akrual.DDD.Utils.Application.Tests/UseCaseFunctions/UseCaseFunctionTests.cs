using Akrual.DDD.Utils.Application.Messaging;
using Akrual.DDD.Utils.Application.UseCaseFunctions;
using Akrual.DDD.Utils.Internal.Tests;
using FluentValidation;
using Xunit;

namespace Akrual.DDD.Utils.Application.Tests.UseCaseFunctions
{
    public class UseCaseFunctionTests : BaseTests
    {
        [Fact]
        public void Execute_WithSampleModels_ReturnSampleModel()
        {
            var usecaseServicer = new ExampleUseCaseFunction();
            var inputmodel = new ExampleInputModel();

            var output = usecaseServicer.Execute(inputmodel);

            Assert.IsType<ExampleOutputModel>(output);
        }

        protected internal class ExampleInputModel : IInputModel
        {
            
        }

        protected internal class ExampleOutputModel : IOutputModel
        {

        }

        private class ExampleUseCaseFunction : BaseUseCaseFunction<ExampleInputModel, ExampleOutputModel>
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
