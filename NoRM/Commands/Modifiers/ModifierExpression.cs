namespace Norm.Commands.Modifiers
{
    using System;
    using System.Linq.Expressions;
    using BSON;


    internal class ModifierExpression<T> : IModifierExpression<T>
    {
        public ModifierExpression()
        {
            Fly=new Expando();
        }
        public void Increment(Expression<Func<T, object>> func,int amountToIncrement)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Increment(amountToIncrement);
        }

        public void SetValue<X>(Expression<Func<T, object>> func,X rer)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Set(rer);
        }

        public void Push<X>(Expression<Func<T, object>> func,X valueToPush)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Push(valueToPush);
        }

        public void PushAll<X>(Expression<Func<T, object>> func, params X[] pushValues)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.PushAll(pushValues);
        }

        public void AddToSet<X>(Expression<Func<T, object>> func, X addToSetValue)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.AddToSet(addToSetValue);            
        }

        public void Pull<X>(Expression<Func<T, object>> func, X pullValue)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Pull(pullValue);
        }

        public void PopFirst(Expression<Func<T, object>> func)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Pop(PopType.RemoveFirst);
        }

        public void PopLast(Expression<Func<T, object>> func)
        {

            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.Pop(PopType.RemoveLast);
        }

        public void PullAll<X>(Expression<Func<T, object>> func, params X[] pullValue)
        {
            var propertyName = TypeHelper.FindProperty(func);
            Fly[propertyName] = M.PullAll(pullValue);
        }

        public Expando Fly { get; set; }
    }
}