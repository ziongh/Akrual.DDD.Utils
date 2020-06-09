using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Entities;

namespace Akrual.DDD.Utils.Domain.Rules
{
    /// <summary>
    /// Represents a rule Engine Executor. It will Crunch all the Rules associated to it and return the final output.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRuleReturn"></typeparam>
    public abstract class BaseDomainRuleEvaluator<TEntity, TRuleReturn> : IDomainRuleEvaluator<TEntity, TRuleReturn>
        where TEntity : IEntity
    {
        protected internal List<IDomainRule<TEntity, TRuleReturn>> _rules = new List<IDomainRule<TEntity, TRuleReturn>>();

        /// <summary>
        /// Instantiates the Rule Evaluator passing the rules as parameters
        /// </summary>
        /// <param name="rules">The rules to instantiate the evaluator</param>
        protected BaseDomainRuleEvaluator(params IDomainRule<TEntity, TRuleReturn>[] rules)
        {
            _rules.AddRange(rules);
        }

        /// <summary>
        /// Instantiates the Rule Evaluator passing the rules as an IEnumerable as parameter
        /// </summary>
        /// <param name="rules">The rules to instantiate the evaluator</param>
        protected BaseDomainRuleEvaluator(IEnumerable<IDomainRule<TEntity, TRuleReturn>> rules)
        {
            _rules.AddRange(rules);
        }

        /// <summary>
        /// Adds Rule to Rules Evaluation. And return True if succeeds
        /// </summary>
        /// <param name="rules">The rules beeing added to evaluation</param>
        /// <returns>True if added sucessfully</returns>
        public bool AddRule(params IDomainRule<TEntity, TRuleReturn>[] rules)
        {
            _rules.AddRange(rules);
            return true;
        }


        /// <summary>
        /// Adds Rule to Rules Evaluation. And return True if succeeds
        /// </summary>
        /// <param name="rules">The rules beeing added to evaluation</param>
        /// <returns>True if added sucessfully</returns>
        public bool AddRule(IEnumerable<IDomainRule<TEntity, TRuleReturn>> rules)
        {
            _rules.AddRange(rules);
            return true;
        }


        /// <summary>
        ///     Removes Rule to Rules Evaluation. And return True if succeeds
        ///     <remarks><c>It will only work if the instance being passed to be removed is the same as the one initially added!</c></remarks>
        /// </summary>
        /// <param name="rules">The rules beeing removed to evaluation</param>
        /// <returns>True if removed sucessfully</returns>
        public bool RemoveRule(params IDomainRule<TEntity, TRuleReturn>[] rules)
        {
            foreach (var rule in rules)
            {
                _rules.Remove(rule);
            }
            return true;
        }


        /// <summary>
        ///     Removes Rule to Rules Evaluation. And return True if succeeds
        ///     <remarks><c>It will only work if the instance being passed to be removed is the same as the one initially added!</c></remarks>
        /// </summary>
        /// <param name="rules">The rules beeing removed to evaluation</param>
        /// <returns>True if removed sucessfully</returns>
        public bool RemoveRule(IEnumerable<IDomainRule<TEntity, TRuleReturn>> rules)
        {
            foreach (var rule in rules)
            {
                _rules.Remove(rule);
            }
            return true;
        }

        /// <summary>
        ///     Evaluates all Rules registered to the given entity and return the result.
        ///     <remarks><c>Here is added the logic of the order, min, max, etc.</c></remarks>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract TRuleReturn ExecuteAllRules(TEntity entity);
    }
}