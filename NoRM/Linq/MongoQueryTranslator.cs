using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using NoRM.BSON;

namespace NoRM.Linq {
    public class MongoQueryTranslator<T>:ExpressionVisitor {
        
        Expression _expression;
        bool collectionSet = false;
        StringBuilder sb;
        Flyweight fly;

        public object FlyWeight {
            get {
                return fly;
            }
        }
        public string WhereExpression {
            get {
                return sb.ToString();
            }
        }

        public string Translate(Expression exp) {
            this.sb = new StringBuilder();
            fly = new Flyweight();
            //partial evaluator will help with converting system level calls
            //to constant expressions

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
                    case TypeCode.DateTime:
                        sb.Append("new Date('");
                        sb.Append(c.Value);
                        sb.Append("')");
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
            fly.MethodCall = m.Method.Name;
            
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where") {
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            } else if (m.Method.DeclaringType == typeof(string)){

                switch (m.Method.Name) {
                    case "StartsWith":
                        sb.Append("(");
                        this.Visit(m.Object);
                        sb.Append(".indexOf(");
                        this.Visit(m.Arguments[0]);
                        sb.Append(")===0)");
                        return m;

                    case "Contains":
                        sb.Append("(");
                        this.Visit(m.Object);
                        sb.Append(".indexOf(");
                        this.Visit(m.Arguments[0]);
                        sb.Append(")>0)");
                        return m;
                    case "IndexOf":
                        this.Visit(m.Object);
                        sb.Append(".indexOf(");
                        this.Visit(m.Arguments[0]);
                        sb.Append(")");
                        return m;
                    case "EndsWith":
                        sb.Append("(");
                        this.Visit(m.Object);
                        sb.Append(".match(");
                        this.Visit(m.Arguments[0]);
                        sb.Append("+'$')==");
                        this.Visit(m.Arguments[0]);
                        sb.Append(")");
                        return m;

                    case "IsNullOrEmpty":
                        sb.Append("(");
                        this.Visit(m.Arguments[0]);
                        sb.Append(" == '' ||  ");
                        this.Visit(m.Arguments[0]);
                        sb.Append(" == null  )");
                        return m;

                }
            } else if (m.Method.DeclaringType == typeof(DateTime)) {

                switch (m.Method.Name) {

                }
            } else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name.StartsWith("First")) {
                fly.Limit = 1;
                if (m.Arguments.Count > 1) {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    if (lambda != null) {
                        this.Visit(lambda.Body);
                    } else {
                        this.Visit(m.Arguments[0]);
                    }
                } else {
                    this.Visit(m.Arguments[0]);
                }
                return m;

            } else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name.StartsWith("SingleOrDefault")) {
                fly.Limit = 1;
                if (m.Arguments.Count > 1) {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    if (lambda != null) {
                        this.Visit(lambda.Body);
                    } else {
                        this.Visit(m.Arguments[0]);
                    }
                } else {
                    this.Visit(m.Arguments[0]);
                }
                return m;

            } else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name.StartsWith("Count")) {
                if (m.Arguments.Count > 1) {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    if (lambda != null) {
                        this.Visit(lambda.Body);
                    } else {
                        this.Visit(m.Arguments[0]);
                    }
                } else {
                    this.Visit(m.Arguments[0]);
                }
                return m;
            }
            //for now...
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {

            var fullName = m.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                sb.Append("this." + m.Member.Name);
                return m;
            }else if (m.Member.DeclaringType == typeof(string)) {
                switch (m.Member.Name) {
                    case "Length":
                        sb.Append("LEN(");
                        this.Visit(m.Expression);
                        sb.Append(")");
                        return m;
                }
            } else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset)) {
                
                //this is a DateProperty hanging off the property - clip the last 2 elements
                var fixedName = fullName.Skip(1).Take(fullName.Length - 2).ToArray();
                var propName = String.Join(".", fixedName);

                //now we get to do some tricky fun with javascript
                switch (m.Member.Name) {
                    case "Day":
                        this.Visit(m.Expression);
                        sb.Append(".getDate()");
                        return m;
                    case "Month":
                        this.Visit(m.Expression);
                        sb.Append(".getMonth()");
                        return m;
                    case "Year":
                        this.Visit(m.Expression);
                        sb.Append(".getYear()");
                        return m;
                    case "Hour":
                        this.Visit(m.Expression);
                        sb.Append(".getHours()");
                        return m;
                    case "Minute":
                        this.Visit(m.Expression);
                        sb.Append(".getMinutes()");
                        return m;
                    case "Second":
                        this.Visit(m.Expression);
                        sb.Append(".getSeconds()");
                        return m;
                    case "DayOfWeek":
                        this.Visit(m.Expression);
                        sb.Append(".getDay()");
                        return m;
                }
            } else {
                //don't want the first - that's the lambda thing
                var fixedName = fullName.Skip(1).Take(fullName.Length - 1).ToArray();

                var result = String.Join(".", fixedName);
                sb.Append("this." + result);

                return m;

            }
            //if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));

        }
    }

}
