namespace NoRM.Commands.Modifiers
{
    using System;
    using System.Linq.Expressions;


    public interface IModifierExpression<T>
    {
        void Increment(Expression<Func<T, object>> func, int ammountToIncrement);
        void SetValue<X>(Expression<Func<T, object>> func,X valueToSet );
        void Push<X>(Expression<Func<T, object>> func,X valueToPush );
        void PushAll<X>(Expression<Func<T, object>> func,params X[] pushValues);
        void AddToSet<X>(Expression<Func<T, object>> func, X addToSetValue);
        void Pull<X>(Expression<Func<T, object>> func, X pullValue);
        void PopFirst(Expression<Func<T, object>> func);
        void PopLast(Expression<Func<T, object>> func);
        void PullAll<X>(Expression<Func<T, object>> func,params X[] pullValue);
    }
}