using System;
using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Internal.Exceptions;
using FluentValidation.Results;

namespace Akrual.DDD.Utils.Domain.Utils.Validation
{
    public static class ValidationResultExtensions
    {
        public static AggregateException GetAggregateExceptionOfValidation(this ValidationResult validationResult)
        {
            if (!validationResult.IsValid && validationResult.Errors.Any())
            {
                var exceptions = new List<Exception>();
                foreach (var error in validationResult.Errors)
                {
                    var ex = new ContractExceptionWithProperty(error.ErrorMessage)
                    {
                        PropertyName = error.PropertyName,
                        AttemptedValue = error.AttemptedValue
                    };
                    exceptions.Add(ex);
                }
                return new AggregateException("Error on Domain Contract",exceptions);
            }
            else
            {
                return new AggregateException();
            }
        }
    }
}
