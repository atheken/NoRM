using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NoRM.Linq {
    public class MongoQueryTranslator:ExpressionVisitor {
        Expression _expression;
        bool collectionSet = false;
        public MongoQueryTranslator(Expression exp) {

            this.Visit(exp);
        }
        protected override System.Linq.Expressions.Expression VisitBinary(System.Linq.Expressions.BinaryExpression b) {
            
            //inside here is the where the criteria bits are parsed


            return base.VisitBinary(b);
        }

        protected override System.Linq.Expressions.Expression VisitConstant(System.Linq.Expressions.ConstantExpression c) {
            return base.VisitConstant(c);


        }

        protected override System.Linq.Expressions.Expression VisitMethodCall(System.Linq.Expressions.MethodCallExpression m) {
            
            //this is where we build the query
            //greater than etc can all be parsed in here, working on a core "Document"
            //which can be an Expression?

            return base.VisitMethodCall(m);
        }
    }
}
