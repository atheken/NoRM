namespace Norm.Commands.Modifiers
{
    using System;
    using System.Linq.Expressions;
    using BSON;


    internal class ModifierExpression<T> : IModifierExpression<T>
    {
        public ModifierExpression()
        {
            Expression=new Expando();
        }
        public void Increment(Expression<Func<T, object>> func,int amountToIncrement)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Increment(amountToIncrement);
        }

        public void SetValue<X>(Expression<Func<T, object>> func,X rer)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Set(rer);
        }

        public void Push<X>(Expression<Func<T, object>> func,X valueToPush)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Push(valueToPush);
        }

        public void PushAll<X>(Expression<Func<T, object>> func, params X[] pushValues)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.PushAll(pushValues);
        }

        public void AddToSet<X>(Expression<Func<T, object>> func, X addToSetValue)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.AddToSet(addToSetValue);            
        }

        public void Pull<X>(Expression<Func<T, object>> func, X pullValue)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Pull(pullValue);
        }

        public void PopFirst(Expression<Func<T, object>> func)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Pop(PopType.RemoveFirst);
        }

        public void PopLast(Expression<Func<T, object>> func)
        {

            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.Pop(PopType.RemoveLast);
        }

        public void PullAll<X>(Expression<Func<T, object>> func, params X[] pullValue)
        {
            var propertyName = ReflectionHelper.FindProperty(func);
            Expression[propertyName] = M.PullAll(pullValue);
        }

        /// <summary>
        /// The modifier expression
        /// </summary>
        public Expando Expression { get; set; }
    }
}