using System;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Configuration;
using Norm.Protocol.Messages;

namespace Norm.Collections
{
     class CreateIndexExpression<T> : ICreateIndexExpression<T>
    {
        public Expando Expando
        {
            get;
            set;
        }

        public string CompoundName
        {
            get;
            set;
        }

        public CreateIndexExpression()
        {
            Expando = new Expando();

        }
        public void Index(Expression<Func<T, object>> func, IndexOption indexDirection)
        {
            var propName = this.RecurseExpression(func.Body);
            Expando[propName] = indexDirection;
            CompoundName += propName + "_" + (int)indexDirection;

        }
        private String RecurseExpression(Expression body)
        {
            var me = body as MemberExpression;
            if (me != null)
            {
                return this.RecurseMemberExpression(me);
            }

            var ue = body as UnaryExpression;
            if (ue != null)
            {
                return this.RecurseExpression(ue.Operand);
            }

            throw new MongoException("Unknown expression type, expected a MemberExpression or UnaryExpression.");
        }
        private String RecurseMemberExpression(MemberExpression mex)
        {
            var retval = "";
            var parentEx = mex.Expression as MemberExpression;
            if (parentEx != null)
            {
                //we need to recurse because we're not at the root yet.
                retval += this.RecurseMemberExpression(parentEx) + ".";
            }
            retval += MongoConfiguration.GetPropertyAlias(mex.Expression.Type, mex.Member.Name);
            return retval;
        }

    }

    public interface ICreateIndexExpression<T>
    {
        // Methods
        void Index(Expression<Func<T, object>> func, IndexOption indexDirection);

        // Properties
        string CompoundName { get; set; }
        Expando Expando { get; set; }
    }
}