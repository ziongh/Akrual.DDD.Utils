using System;
using System.Collections.Generic;
using System.Reflection;
using Akrual.DDD.Utils.Application.Messaging;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Utils.Validation;
using FluentValidation;
using FluentValidation.Results;

namespace Akrual.DDD.Utils.Application.UseCaseFunctions
{
    /// <inheritdoc />
    public abstract class BaseUseCaseFunction<TInputModel, TOutputModel> : IUseCaseFunction<TInputModel, TOutputModel>
        where TInputModel : IInputModel
        where TOutputModel : IOutputModel
    {

        protected abstract AbstractValidator<TInputModel> PreConditionEvaluator { get; }
        protected abstract AbstractValidator<TOutputModel> PostConditionEvaluator { get; }

        protected abstract TOutputModel WhatToExecute(TInputModel input);

        public TOutputModel Execute(TInputModel input)
        {
            EvaluatePreConditions(input);
            var result = WhatToExecute(input);
            EvaluatePostConditions(result);
            return result;
        }

        private void EvaluatePostConditions(TOutputModel output)
        {
            var validationResult = PostConditionEvaluator?.Validate(output) ?? new ValidationResult();
            if (!validationResult.IsValid)
            {
                throw validationResult.GetAggregateExceptionOfValidation();
            }
        }

        private void EvaluatePreConditions(TInputModel input)
        {
            var validationResult = PreConditionEvaluator?.Validate(input) ?? new ValidationResult();
            if (!validationResult.IsValid)
            {
                throw validationResult.GetAggregateExceptionOfValidation();
            }
        }

    }
}
