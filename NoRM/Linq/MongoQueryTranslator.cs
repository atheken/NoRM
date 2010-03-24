using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Norm.BSON;
using Norm.Configuration;

namespace Norm.Linq
{
    /// <summary>
    /// The mongo query translator.
    /// </summary>
    public class MongoQueryTranslator : ExpressionVisitor
    {
        private Expression _expression;
        private bool collectionSet;
        private string lastFlyProperty = string.Empty;
        private string lastOperator = " == ";
        private StringBuilder sb;
        private StringBuilder sbIndexed;

        public String PropName
        {
            get;
            set;
        }
        public String TypeName
        {
            get;
            set;
        }

        public String MethodCall
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether IsComplex.
        /// </summary>
        public bool IsComplex { get; private set; }

        /// <summary>
        /// Gets optimized where clause.
        /// </summary>
        public string OptimizedWhere
        {
            get { return sbIndexed.ToString(); }
        }

        /// <summary>
        /// Gets conditional count.
        /// </summary>
        public int ConditionalCount { get; private set; }

        /// <summary>
        /// Gets FlyWeight.
        /// </summary>
        public Flyweight FlyWeight { get; private set; }

        /// <summary>
        /// How many to skip.
        /// </summary>
        public int Skip { get; set; }

        private int _takeCount = Int32.MaxValue;

        /// <summary>
        /// How many to take (Int32.MaxValue) by default.
        /// </summary>
        public int Take
        {
            get
            {
                return this._takeCount;
            }
            set
            {
                this._takeCount = value;
            }
        }

        /// <summary>
        /// Gets where expression.
        /// </summary>
        public string WhereExpression
        {
            get { return sb.ToString(); }
        }

        public bool UseScopedQualifier { get; set; }

        /// <summary>
        /// The translate collection name.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns>The collection name.</returns>
        public string TranslateCollectionName(Expression exp)
        {
            ConstantExpression c = null;
            switch (exp.NodeType)
            {
                case ExpressionType.Constant:
                    c = (ConstantExpression)exp;
                    break;
                case ExpressionType.Call:
                    {
                        var m = (MethodCallExpression)exp;
                        c = m.Arguments[0] as ConstantExpression;
                    }
                    break;
            }

            //var result = string.Empty;

            // the first argument is a Constant - it's the query itself
            var q = c.Value as IQueryable;
            var result = q.ElementType.Name;

            return result;
        }

        /// <summary>
        /// Translates LINQ to MongoDB.
        /// </summary>
        /// <param name="exp">The expression.</param>
        /// <returns>The translate.</returns>
        public string Translate(Expression exp)
        {
            return Translate(exp, true);
        }

        public string Translate(Expression exp, bool useScopedQualifier)
        {
            UseScopedQualifier = useScopedQualifier;
            sb = new StringBuilder();
            sbIndexed = new StringBuilder();
            FlyWeight = new Flyweight();
            Visit(exp);
            return sb.ToString();
        }

        /// <summary>
        /// Visits member access.
        /// </summary>
        /// <param name="m">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            //if(m.Expression.NodeType == ExpressionType.MemberAccess)
            //{
            //    VisitMemberAccess((MemberExpression)m.Expression);
            //}

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                var alias = MongoConfiguration.GetPropertyAlias(m.Expression.Type, m.Member.Name);

                if (UseScopedQualifier)
                {
                    sb.Append("this.");
                }
                sb.Append(alias);

                lastFlyProperty = alias;
                return m;
            }

            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        sb.Append("LEN(");
                        Visit(m.Expression);
                        sb.Append(")");
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                var fullName = m.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                // this is complex
                IsComplex = true;

                // this is a DateProperty hanging off the property - clip the last 2 elements
                var fixedName = fullName.Skip(1).Take(fullName.Length - 2).ToArray();
                var propName = string.Join(".", fixedName);

                // now we get to do some tricky fun with javascript
                switch (m.Member.Name)
                {
                    case "Day":
                        Visit(m.Expression);
                        sb.Append(".getDate()");
                        return m;
                    case "Month":
                        Visit(m.Expression);
                        sb.Append(".getMonth()");
                        return m;
                    case "Year":
                        Visit(m.Expression);
                        sb.Append(".getFullYear()");
                        return m;
                    case "Hour":
                        Visit(m.Expression);
                        sb.Append(".getHours()");
                        return m;
                    case "Minute":
                        Visit(m.Expression);
                        sb.Append(".getMinutes()");
                        return m;
                    case "Second":
                        Visit(m.Expression);
                        sb.Append(".getSeconds()");
                        return m;
                    case "DayOfWeek":
                        Visit(m.Expression);
                        sb.Append(".getDay()");
                        return m;
                }
            }
            //else if (m.Expression.NodeType == ExpressionType.MemberAccess)
            //{
            //    // Same as below.
            //    // if this is a property NOT on the object...
            //    throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
            //}
            //else if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
            //{
            //    switch (m.Member.MemberType)
            //    {
            //        case MemberTypes.Property:
            //            var propertyInfo = (PropertyInfo)m.Member;
            //            var innerMember = (MemberExpression)m.Expression;
            //            var closureFieldInfo = (FieldInfo)innerMember.Member;
            //            var obj = closureFieldInfo.GetValue(((ConstantExpression)innerMember.Expression).Value);
            //            var propertyAlias = propertyInfo.GetValue(obj, null).ToString();
            //            sb.Append(propertyAlias);

            //            lastFlyProperty = propertyAlias;
            //            break;
            //        case MemberTypes.Field:
            //            var fieldInfo = (FieldInfo)m.Member;
            //            var fieldAlias = fieldInfo.GetValue(((ConstantExpression)m.Expression).Value).ToString();
            //            sb.Append(fieldAlias);

            //            lastFlyProperty = fieldAlias;
            //            break;
            //        default:
            //            Visit(m.Expression);
            //            break;
            //    }

            //    return m;
            //}
            else
            {
                var fullName = m.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                // this supports the "deep graph" name - "Product.Address.City"
                var fixedName = fullName.Skip(1).Take(fullName.Length - 1).ToArray();

                String result = "";
                var constant = m.Expression as ConstantExpression;
                if (constant != null)
                {
                    //result = constant.GetConstantValue().ToString();
                }
                else
                {
                    var expressionRootType = GetParameterExpression((MemberExpression)m.Expression);

                    if (expressionRootType != null)
                    {
                        fixedName = GetDeepAlias(expressionRootType.Type, fixedName);
                    }

                    result = string.Join(".", fixedName);
                    //sb.Append("this." + result);
                    if (UseScopedQualifier)
                    {
                        sb.Append("this.");
                    }
                }
                sb.Append(result);

                lastFlyProperty = result;
                return m;
            }

            // if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        /// <summary>
        /// Visits a binary expression.
        /// </summary>
        /// <param name="b">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            ConditionalCount++;
            sb.Append("(");
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" && ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    IsComplex = true;
                    sb.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    lastOperator = " == ";
                    sb.Append(lastOperator);
                    break;
                case ExpressionType.NotEqual:
                    lastOperator = " <> ";
                    sb.Append(lastOperator);
                    break;
                case ExpressionType.LessThan:
                    lastOperator = " < ";
                    sb.Append(lastOperator);
                    break;
                case ExpressionType.LessThanOrEqual:
                    lastOperator = " <= ";
                    sb.Append(lastOperator);
                    break;
                case ExpressionType.GreaterThan:
                    lastOperator = " > ";
                    sb.Append(lastOperator);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    lastOperator = " >= ";
                    sb.Append(lastOperator);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            Visit(b.Right);
            sb.Append(")");
            return b;
        }

        /// <summary>
        /// Visits a constant.
        /// </summary>
        /// <param name="c">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q != null)
            {
                // set the collection name
                this.TypeName = q.ElementType.Name;

                // this is our Query wrapper - see if it has an expression
                var qry = (IMongoQuery)c.Value;
                var innerExpression = qry.GetExpression();
                if (innerExpression.NodeType == ExpressionType.Call)
                {
                    VisitMethodCall(innerExpression as MethodCallExpression);
                }
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        SetFlyValue(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.DateTime:
                        var val = "new Date('" + c.Value + "')";
                        sb.Append(val);
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.String:
                        var sval = "'" + c.Value + "'";
                        sb.Append(sval);
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.Object:
                        if (c.Value is ObjectId)
                        {
                            sb.AppendFormat("ObjectId('{0}')",c.Value);
                            SetFlyValue(c.Value);
                        }
                        else if (c.Value is Guid)
                        {
                            sb.AppendFormat("'{0}'", c.Value);
                            SetFlyValue(c.Value);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        }
                        break;
                    default:
                        sb.Append(c.Value);
                        SetFlyValue(c.Value);
                        break;
                }
            }

            return c;
        }

        /// <summary>
        /// Visits a method call.
        /// </summary>
        /// <param name="m">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (string.IsNullOrEmpty(this.MethodCall))
            {
                this.MethodCall = m.Method.Name;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }
            if (m.Method.DeclaringType == typeof(string))
            {
                IsComplex = true;
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        sb.Append("(");
                        Visit(m.Object);
                        sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        sb.Append(")===0)");
                        return m;

                    case "Contains":
                        sb.Append("(");
                        Visit(m.Object);
                        sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        sb.Append(")>0)");
                        return m;
                    case "IndexOf":
                        Visit(m.Object);
                        sb.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        sb.Append(")");
                        return m;
                    case "EndsWith":
                        sb.Append("(");
                        Visit(m.Object);
                        sb.Append(".match(");
                        Visit(m.Arguments[0]);
                        sb.Append("+'$')==");
                        Visit(m.Arguments[0]);
                        sb.Append(")");
                        return m;

                    case "IsNullOrEmpty":
                        sb.Append("(");
                        Visit(m.Arguments[0]);
                        sb.Append(" == '' ||  ");
                        Visit(m.Arguments[0]);
                        sb.Append(" == null  )");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                //switch (m.Method.Name)
                //{
                //}
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && IsCallableMethod(m.Method.Name))
            {
                return HandleMethodCall(m);
            }
            else if (m.Method.DeclaringType == typeof(Enumerable))
            {
                // Subquery - Count() or Sum()
                if (IsCallableMethod(m.Method.Name))
                {
                }
            }

            // for now...
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        /// <summary>
        /// Determines if it's a callable method.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <returns>The is callable method.</returns>
        private static bool IsCallableMethod(string methodName)
        {
            var acceptableMethods = new[]
                                        {
                                            "First",
                                            "Single",
                                            "FirstOrDefault",
                                            "SingleOrDefault",
                                            "Count",
                                            "Sum",
                                            "Average",
                                            "Min",
                                            "Max",
                                            "Any",
                                            "Take",
                                            "Skip",
                                            "Count"
                                        };
            return acceptableMethods.Any(x => x == methodName);
        }

        /// <summary>
        /// Strip quotes.
        /// </summary>
        /// <param name="e">The expression.</param>
        /// <returns></returns>
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        /// <summary>
        /// The get parameter expression.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// </returns>
        private static ParameterExpression GetParameterExpression(Expression expression)
        {
            var expressionRoot = false;
            Expression parentExpression = expression;

            while (!expressionRoot)
            {
                parentExpression = ((MemberExpression)parentExpression).Expression;
                expressionRoot = parentExpression is ParameterExpression;
            }

            return (ParameterExpression)parentExpression;
        }

        /// <summary>
        /// The get deep alias.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="graph">The graph.</param>
        /// <returns></returns>
        private static string[] GetDeepAlias(Type type, string[] graph)
        {
            var graphParts = new string[graph.Length];
            var typeToQuery = type;

            for (var i = 0; i < graph.Length; i++)
            {
                var prpperty = BSON.TypeHelper.FindProperty(typeToQuery, graph[i]);
                graphParts[i] = MongoConfiguration.GetPropertyAlias(typeToQuery, graph[i]);
                typeToQuery = prpperty.PropertyType;
            }

            return graphParts;
        }

        /// <summary>
        /// The set flyweight value.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetFlyValue(object value)
        {
            // if the property has already been set, we can't set it again
            // as fly uses Dictionaries. This means to BETWEEN style native queries
            if (FlyWeight.Contains(lastFlyProperty))
            {
                IsComplex = true;
                return;
            }

            if (lastOperator != " == ")
            {
                // Can't do comparisons here unless the type is a double
                // which is a limitation of mongo, apparently
                // and won't work if we're doing date comparisons
                if (value.GetType().IsAssignableFrom(typeof(double)))
                {
                    switch (lastOperator)
                    {
                        case " > ":
                            FlyWeight[lastFlyProperty] = Q.GreaterThan((double)value);
                            break;
                        case " < ":
                            FlyWeight[lastFlyProperty] = Q.LessThan((double)value);
                            break;
                        case " <= ":
                            FlyWeight[lastFlyProperty] = Q.LessOrEqual((double)value);
                            break;
                        case " >= ":
                            FlyWeight[lastFlyProperty] = Q.GreaterOrEqual((double)value);
                            break;
                        case " <> ":
                            FlyWeight[lastFlyProperty] = Q.NotEqual(value);
                            break;
                    }
                }
                else
                {
                    // Can't assign? Push to the $where
                    IsComplex = true;
                }
            }
            else
            {
                FlyWeight[lastFlyProperty] = value;
            }
        }

        /// <summary>
        /// Handles skip.
        /// </summary>
        /// <param name="exp">The expression.</param>
        private void HandleSkip(Expression exp)
        {
            FlyWeight["$skip"] = (int)exp.GetConstantValue();
        }

        /// <summary>
        /// Handles take.
        /// </summary>
        /// <param name="exp">The expression.</param>
        private void HandleTake(Expression exp)
        {
            FlyWeight["$limit"] = (int)exp.GetConstantValue();
        }

        /// <summary>
        /// The handle method call.
        /// </summary>
        /// <param name="m">The expression.</param>
        /// <returns></returns>
        private Expression HandleMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Skip":
                    HandleSkip(m.Arguments[1]);
                    return m;
                case "Take":
                    HandleTake(m.Arguments[1]);
                    Visit(m.Arguments[0]);
                    return m;
                default:
                    this.Take = 1;
                    this.MethodCall = m.Method.Name;
                    if (m.Arguments.Count > 1)
                    {
                        var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        if (lambda != null)
                        {
                            Visit(lambda.Body);
                        }
                        else
                        {
                            Visit(m.Arguments[0]);
                        }
                    }
                    else
                    {
                        Visit(m.Arguments[0]);
                    }
                    break;
            }

            return m;
        }
    }
}