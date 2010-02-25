namespace NoRM.Linq
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using BSON;

    public class MongoQueryTranslator<T> : ExpressionVisitor
    {
        private Flyweight _fly;
        private StringBuilder _sb;

        public Flyweight Flyweight
        {
            get { return _fly; }
        }

        public string WhereExpression
        {
            get { return _sb.ToString(); }
        }

        public string Translate(Expression exp)
        {
            _sb = new StringBuilder();
            _fly = new Flyweight();            
            Visit(exp);
            return _sb.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _sb.Append("(");
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _sb.Append(" && ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _sb.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    _sb.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    _sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    _sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            Visit(b.Right);
            _sb.Append(")");
            return b;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression) e).Operand;
            }
            return e;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                //_sb.Append("SELECT * FROM ");
                //_sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                _sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sb.Append(((bool) c.Value) ? 1 : 0);
                        break;
                    case TypeCode.DateTime:
                        _sb.Append("new Date('");
                        _sb.Append(c.Value);
                        _sb.Append("')");
                        break;
                    case TypeCode.String:
                        _sb.Append("'");
                        _sb.Append(c.Value);
                        _sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        _sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof (Queryable) && m.Method.Name == "Where")
            {
                var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }
            if (m.Method.DeclaringType == typeof (string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        _sb.Append("(");
                        Visit(m.Object);
                        _sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        _sb.Append(")===0)");
                        return m;

                    case "Contains":
                        _sb.Append("(");
                        Visit(m.Object);
                        _sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        _sb.Append(")>0)");
                        return m;
                    case "IndexOf":
                        Visit(m.Object);
                        _sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        _sb.Append(")");
                        return m;
                    case "EndsWith":
                        _sb.Append("(");
                        Visit(m.Object);
                        _sb.Append(".match(");
                        Visit(m.Arguments[0]);
                        _sb.Append("+'$')==");
                        Visit(m.Arguments[0]);
                        _sb.Append(")");
                        return m;

                    case "IsNullOrEmpty":
                        _sb.Append("(");
                        Visit(m.Arguments[0]);
                        _sb.Append(" == '' ||  ");
                        Visit(m.Arguments[0]);
                        _sb.Append(" == null  )");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof (DateTime))
            {
                //switch (m.Method.Name){}
            }
            else if (m.Method.DeclaringType == typeof (Queryable) && m.Method.Name.StartsWith("First"))
            {
                _fly.Limit = 1;                
                Visit(m.Arguments[0]);
                return m;
            }
            else if (m.Method.DeclaringType == typeof (Queryable) && m.Method.Name.StartsWith("SingleOrDefault"))
            {
                _fly.Limit = 1;
                var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
                if (lambda != null)
                {
                    Visit(lambda.Body);
                }
                else
                {
                    Visit(m.Arguments[0]);
                }
                return m;
            }

            //for now...
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var fullName = m.ToString().Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _sb.Append("this." + m.Member.Name);
                return m;
            }
            if (m.Member.DeclaringType == typeof (string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        _sb.Append("LEN(");
                        Visit(m.Expression);
                        _sb.Append(")");
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof (DateTime) || m.Member.DeclaringType == typeof (DateTimeOffset))
            {                
                switch (m.Member.Name)
                {
                    case "Day":
                        Visit(m.Expression);
                        _sb.Append(".getDate()");
                        return m;
                    case "Month":
                        Visit(m.Expression);
                        _sb.Append(".getMonth()");
                        return m;
                    case "Year":
                        Visit(m.Expression);
                        _sb.Append(".getFullYear()");
                        return m;
                    case "Hour":
                        Visit(m.Expression);
                        _sb.Append(".getHours()");
                        return m;
                    case "Minute":
                        Visit(m.Expression);
                        _sb.Append(".getMinutes()");
                        return m;
                    case "Second":
                        Visit(m.Expression);
                        _sb.Append(".getSeconds()");
                        return m;
                    case "DayOfWeek":
                        Visit(m.Expression);
                        _sb.Append(".getDay()");
                        return m;
                }
            }
            else
            {
                //don't want the first - that's the lambda thing
                var fixedName = fullName.Skip(1).Take(fullName.Length - 1).ToArray();
                var result = String.Join(".", fixedName);
                _sb.Append("this." + result);
                return m;
            }
            //if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}