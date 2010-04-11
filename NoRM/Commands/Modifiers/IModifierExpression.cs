namespace Norm.Commands.Modifiers
{
    using System;
    using System.Linq.Expressions;

    /// <summary>TODO::Description.</summary>
    public interface IModifierExpression<T>
    {
        /// <summary>TODO::Description.</summary>
        void Increment(Expression<Func<T, object>> func, int ammountToIncrement);

        /// <summary>TODO::Description.</summary>
        void SetValue<X>(Expression<Func<T, object>> func,X valueToSet );

        /// <summary>TODO::Description.</summary>
        void Push<X>(Expression<Func<T, object>> func,X valueToPush );

        /// <summary>TODO::Description.</summary>
        void PushAll<X>(Expression<Func<T, object>> func,params X[] pushValues);

        /// <summary>TODO::Description.</summary>
        void AddToSet<X>(Expression<Func<T, object>> func, X addToSetValue);

        /// <summary>TODO::Description.</summary>
        void Pull<X>(Expression<Func<T, object>> func, X pullValue);

        /// <summary>TODO::Description.</summary>
        void PopFirst(Expression<Func<T, object>> func);

        /// <summary>TODO::Description.</summary>
        void PopLast(Expression<Func<T, object>> func);

        /// <summary>TODO::Description.</summary>
        void PullAll<X>(Expression<Func<T, object>> func,params X[] pullValue);
    }
}