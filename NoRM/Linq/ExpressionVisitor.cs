using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Norm.Linq
{
    /// <summary>
    /// The expression visitor.
    /// </summary>
    public abstract class ExpressionVisitor
    {
        /// <summary>
        /// Visits an expression.
        /// </summary>
        /// <param retval="exp">The expression.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// </exception>
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression) exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression) exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression) exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression) exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression) exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression) exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression) exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression) exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression) exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression) exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression) exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression) exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression) exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression) exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        /// <summary>
        /// Visits a binding.
        /// </summary>
        /// <param retval="binding">
        /// The binding.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment) binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding) binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding) binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        /// <summary>
        /// Visits an element initializer.
        /// </summary>
        /// <param retval="initializer">The initializer.</param>
        /// <returns></returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            var arguments = VisitExpressionList(initializer.Arguments);
            return arguments != initializer.Arguments 
                ? Expression.ElementInit(initializer.AddMethod, arguments) 
                : initializer;
        }

        /// <summary>
        /// Visits a unary expression.
        /// </summary>
        /// <param retval="u">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            var operand = Visit(u.Operand);
            return operand != u.Operand 
                ? Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method) 
                : u;
        }

        /// <summary>
        /// Visits a binary expression.
        /// </summary>
        /// <param retval="b">
        /// The expression.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            var left = Visit(b.Left);
            var right = Visit(b.Right);
            var conversion = Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }

            return b;
        }

        /// <summary>
        /// Visits a "type is" expression
        /// </summary>
        /// <param retval="b">
        /// The expression.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            var expr = Visit(b.Expression);
            return expr != b.Expression 
                ? Expression.TypeIs(expr, b.TypeOperand) 
                : b;
        }

        /// <summary>
        /// Visits a constant.
        /// </summary>
        /// <param retval="c">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        /// <summary>
        /// Visits a conditional.
        /// </summary>
        /// <param retval="c">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            var test = Visit(c.Test);
            var ifTrue = Visit(c.IfTrue);
            var ifFalse = Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }

            return c;
        }

        /// <summary>
        /// Visits a parameter.
        /// </summary>
        /// <param retval="p">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        /// <summary>
        /// The visit member access.
        /// </summary>
        /// <param retval="m">The m.</param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            var exp = Visit(m.Expression);
            return exp != m.Expression 
                ? Expression.MakeMemberAccess(exp, m.Member) 
                : m;
        }

        /// <summary>
        /// Visits amethod call.
        /// </summary>
        /// <param retval="m">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            var obj = Visit(m.Object);
            IEnumerable<Expression> args = VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }

            return m;
        }

        /// <summary>
        /// Visits an expression list.
        /// </summary>
        /// <param retval="original">
        /// The original expression.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(p);
                }
            }

            return list != null 
                ? list.AsReadOnly() 
                : original;
        }

        /// <summary>
        /// Visits a member assignment.
        /// </summary>
        /// <param retval="assignment">
        /// The assignment.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var e = Visit(assignment.Expression);
            return e != assignment.Expression 
                ? Expression.Bind(assignment.Member, e) 
                : assignment;
        }

        /// <summary>
        /// Visits a member member binding.
        /// </summary>
        /// <param retval="binding">
        /// The binding.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindings = VisitBindingList(binding.Bindings);
            return bindings != binding.Bindings 
                ? Expression.MemberBind(binding.Member, bindings) 
                : binding;
        }

        /// <summary>
        /// Visits a member list binding.
        /// </summary>
        /// <param retval="binding">
        /// The binding.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers = VisitElementInitializerList(binding.Initializers);
            return initializers != binding.Initializers 
                ? Expression.ListBind(binding.Member, initializers) 
                : binding;
        }

        /// <summary>
        /// Visits a binding list.
        /// </summary>
        /// <param retval="original">
        /// The original.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(b);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        /// <summary>
        /// Visits an element initializer list.
        /// </summary>
        /// <param retval="original">The original.</param>
        /// <returns></returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(init);
                }
            }

            if (list != null)
            {
                return list;
            }
            return original;
        }

        /// <summary>
        /// Visits a lambda.
        /// </summary>
        /// <param retval="lambda">
        /// The lambda.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            var body = Visit(lambda.Body);
            return body != lambda.Body 
                ? Expression.Lambda(lambda.Type, body, lambda.Parameters) 
                : lambda;
        }

        /// <summary>
        /// Visits a new expression.
        /// </summary>
        /// <param retval="nex">
        /// The expression.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                return nex.Members != null 
                    ? Expression.New(nex.Constructor, args, nex.Members) 
                    : Expression.New(nex.Constructor, args);
            }

            return nex;
        }

        /// <summary>
        /// Visits a member init.
        /// </summary>
        /// <param retval="init">The init.</param>
        /// <returns></returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var bindings = VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }

            return init;
        }

        /// <summary>
        /// Visits a list init.
        /// </summary>
        /// <param retval="init">The init.</param>
        /// <returns></returns>
        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var initializers = VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }

            return init;
        }

        /// <summary>
        /// Visits a new array.
        /// </summary>
        /// <param retval="na">
        /// The na.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                return na.NodeType == ExpressionType.NewArrayInit 
                    ? Expression.NewArrayInit(na.Type.GetElementType(), exprs) 
                    : Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
            }

            return na;
        }

        /// <summary>
        /// Visits an invocation.
        /// </summary>
        /// <param retval="iv">The invocation.</param>
        /// <returns></returns>
        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitExpressionList(iv.Arguments);
            var expr = Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }

            return iv;
        }
    }
}