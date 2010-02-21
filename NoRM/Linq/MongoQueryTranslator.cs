using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NoRM.Linq {
    public class MongoQueryTranslator<T>:ExpressionVisitor {
        
        Expression _expression;
        bool collectionSet = false;
        StringBuilder sb;


        public string WhereExpression {
            get {
                return sb.ToString();
            }
        }

        public string Translate(Expression exp) {
            this.sb = new StringBuilder();
            this.Visit(exp);
            return sb.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" && ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }
        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        protected override Expression VisitConstant(ConstantExpression c) {
            IQueryable q = c.Value as IQueryable;
            if (q != null) {
                // assume constant nodes w/ IQueryables are table references
                //sb.Append("SELECT * FROM ");
                //sb.Append(q.ElementType.Name);
            } else if (c.Value == null) {
                sb.Append("NULL");
            } else {
                switch (Type.GetTypeCode(c.Value.GetType())) {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where") {
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }

            //for now...
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                sb.Append("this." + m.Member.Name);
                return m;
            } else {
                //nested expression
                //reference from the top level down
                var fullName = m.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                //don't want the first - that's the lambda thing
                var fixedName = fullName.Skip(1).Take(fullName.Length - 1).ToArray();

                var result = String.Join(".", fixedName);
                sb.Append("this." + result);

                return m;
            }
        }
    }
}
