using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Norm.Linq
{
    /// <summary>
    /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
    /// </summary>
    public static class PartialEvaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>
        /// A new tree with sub-trees evaluated and replaced.
        /// </returns>
        public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression), expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>
        /// A new tree with sub-trees evaluated and replaced.
        /// </returns>
        public static Expression Eval(Expression expression)
        {
            return Eval(expression, CanBeEvaluatedLocally);
        }

        /// <summary>
        /// The can be evaluated locally.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The can be evaluated locally.</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            /// <summary>
            /// The candidates.
            /// </summary>
            private HashSet<Expression> candidates;

            /// <summary>
            /// The cannot be evaluated.
            /// </summary>
            private bool cannotBeEvaluated;

            /// <summary>
            /// The fn can be evaluated.
            /// </summary>
            private Func<Expression, bool> fnCanBeEvaluated;

            /// <summary>
            /// Initializes a new instance of the <see cref="Nominator"/> class.
            /// </summary>
            /// <param name="fnCanBeEvaluated">
            /// The fn can be evaluated.
            /// </param>
            private Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                candidates = new HashSet<Expression>();
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            /// <summary>
            /// Nomination.
            /// </summary>
            /// <param name="fnCanBeEvaluated">The fn can be evaluated.</param>
            /// <param name="expression">The expression.</param>
            /// <returns></returns>
            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
            {
                var nominator = new Nominator(fnCanBeEvaluated);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            /// <summary>
            /// Visits an expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <returns></returns>
            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    var saveCannotBeEvaluated = cannotBeEvaluated;
                    cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!cannotBeEvaluated)
                    {
                        if (fnCanBeEvaluated(expression))
                        {
                            candidates.Add(expression);
                        }
                        else
                        {
                            cannotBeEvaluated = true;
                        }
                    }

                    cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }
        }

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            /// <summary>
            /// The candidates.
            /// </summary>
            private readonly HashSet<Expression> _candidates;

            /// <summary>
            /// Initializes a new instance of the <see cref="SubtreeEvaluator"/> class.
            /// </summary>
            /// <param name="candidates">
            /// The candidates.
            /// </param>
            private SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this._candidates = candidates;
            }

            /// <summary>
            /// The eval.
            /// </summary>
            /// <param name="candidates">
            /// The candidates.
            /// </param>
            /// <param name="exp">
            /// The exp.
            /// </param>
            /// <returns>
            /// </returns>
            internal static Expression Eval(HashSet<Expression> candidates, Expression exp)
            {
                return new SubtreeEvaluator(candidates).Visit(exp);
            }

            /// <summary>
            /// The visit.
            /// </summary>
            /// <param name="exp">
            /// The exp.
            /// </param>
            /// <returns>
            /// </returns>
            protected override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }

                return _candidates.Contains(exp)
                    ? Evaluate(exp)
                    : base.Visit(exp);
            }

            /// <summary>
            /// The evaluate.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns></returns>
            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }

                var type = e.Type;
                if (type.IsValueType)
                {
                    e = Expression.Convert(e, typeof(object));
                }

                var lambda = Expression.Lambda<Func<object>>(e);
                var fn = lambda.Compile();
                return Expression.Constant(fn(), type);
            }
        }
    }
}