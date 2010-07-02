using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Norm.BSON;
using Norm.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

namespace Norm.Linq
{
    /// <summary>
    /// The mongo query translator.
    /// </summary>
    internal class MongoQueryTranslator : ExpressionVisitor
    {

        private int _takeCount = Int32.MaxValue;
        private string _lastFlyProperty = string.Empty;
        private string _lastOperator = " === ";
        private List<string> _prefixAlias = new List<string>();

        private StringBuilder _sbWhere;

        private Expando FlyWeight { get; set; }
        private Expando SortFly { get; set; }

        bool _whereWritten = false;
        bool _isDeepGraphWithArrays = false;

        private string AggregatePropName { get; set; }
        private string TypeName { get; set; }
        public string CollectionName { get; set; }
        private string MethodCall { get; set; }

        /// <summary>
        /// Gets a value indicating whether IsComplex.
        /// </summary>
        private bool IsComplex { get; set; }

        /// <summary>
        /// Gets conditional count.
        /// </summary>
        private int ConditionalCount { get; set; }

        /// <summary>
        /// How many to skip.
        /// </summary>
        private int Skip { get; set; }

        /// <summary>
        /// How many to take (Int32.MaxValue) by default.
        /// </summary>
        private int Take
        {
            get { return _takeCount; }
            set { _takeCount = value; }
        }

        /// <summary>
        /// Gets where expression.
        /// </summary>
        private string WhereExpression
        {
            get { return _sbWhere.ToString(); }
        }

        /// <summary>
        /// Whether to use the "this" qualifier
        /// </summary>
        public bool UseScopedQualifier { get; set; }

        /// <summary>
        /// Translates LINQ to MongoDB.
        /// </summary>
        /// <param retval="exp">The expression.</param>
        /// <returns>The translated string</returns>
        public QueryTranslationResults Translate(Expression exp)
        {
            return Translate(exp, true);
        }

        private Type OriginalSelectType { get; set; }
        private LambdaExpression SelectLambda { get; set; }

        /// <summary>
        /// Translates LINQ to MongoDB.
        /// </summary>
        /// <param retval="exp">The expression.</param>
        /// <param retval="useScopedQualifier">Whether to use the "this" qualifier</param>
        /// <returns>The translated string</returns>
        public QueryTranslationResults Translate(Expression exp, bool useScopedQualifier)
        {
            UseScopedQualifier = useScopedQualifier;
            _sbWhere = new StringBuilder();
            FlyWeight = new Expando();
            SortFly = new Expando();

            Visit(exp);

            ProcessGuards();
            TransformToFlyWeightWhere();

            return new QueryTranslationResults
                       {
                           Where = FlyWeight,
                           Sort = SortFly,
                           Skip = Skip,
                           Take = Take,
                           CollectionName = CollectionName,
                           MethodCall = MethodCall,
                           AggregatePropName = AggregatePropName,
                           IsComplex = IsComplex,
                           TypeName = TypeName,
                           Query = WhereExpression,
                           Select = SelectLambda,
                           OriginalSelectType = OriginalSelectType
                       };
        }

        private void ProcessGuards()
        {
            if (_isDeepGraphWithArrays && IsComplex)
            {
                var aggMethods = new[] { "Max", "Min", "Sum", "Average" };
                if (aggMethods.Contains(MethodCall))
                    throw new NotSupportedException("You cannot use deep graph resolution when using the following aggregates: " + string.Join(", ", aggMethods));

                throw new NotSupportedException("You cannot use deep graph resolution if the query is considered complex");
            }
        }

        private void TransformToFlyWeightWhere()
        {
            var where = WhereExpression;
            if (!string.IsNullOrEmpty(where) && IsComplex)
            {
                // reset - need to use the where statement generated
                // instead of the props set on the internal flyweight
                FlyWeight = new Expando();
                if (where.StartsWith("function"))
                {
                    FlyWeight["$where"] = where;
                }
                else
                {
                    FlyWeight["$where"] = " function(){return " + where + ";}";
                }
            }
        }

        /// <summary>
        /// Visits member access.
        /// </summary>
        /// <param retval="m">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                var alias = VisitAlias(m);

                VisitDateTimeProperty(m);
                
                if (UseScopedQualifier)
                {
                    _sbWhere.Append("this.");
                }

                _sbWhere.Append(alias);
                _lastFlyProperty = alias;

                return m;
            }

            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        IsComplex = true;
                        Visit(m.Expression);
                        _sbWhere.Append(".length");
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                #region DateTime Magic
                // this is complex
                IsComplex = true;

                // now we get to do some tricky fun with javascript
                switch (m.Member.Name)
                {
                    case "Day":
                        Visit(m.Expression);
                        _sbWhere.Append(".getDate()");
                        return m;
                    case "Month":
                        Visit(m.Expression);
                        _sbWhere.Append(".getMonth()");
                        return m;
                    case "Year":
                        Visit(m.Expression);
                        _sbWhere.Append(".getFullYear()");
                        return m;
                    case "Hour":
                        Visit(m.Expression);
                        _sbWhere.Append(".getHours()");
                        return m;
                    case "Minute":
                        Visit(m.Expression);
                        _sbWhere.Append(".getMinutes()");
                        return m;
                    case "Second":
                        Visit(m.Expression);
                        _sbWhere.Append(".getSeconds()");
                        return m;
                    case "DayOfWeek":
                        Visit(m.Expression);
                        _sbWhere.Append(".getDay()");
                        return m;
                }
                #endregion
            }
            else
            {
                // this supports the "deep graph" retval - "Product.Address.City"
                string deepAlias = VisitDeepAlias(m);

                VisitDateTimeProperty(m);
                if (UseScopedQualifier)
                {
                    _sbWhere.Append("this.");
                }

                _sbWhere.Append(deepAlias);
                _lastFlyProperty = deepAlias;

                return m;
            }

            // if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        private string VisitDeepAlias(MemberExpression m)
        {
            var fullName = m.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            var fixedName = fullName
                .Skip(1)
                .Select(x => Regex.Replace(x, @"^get_Item\(([0-9]+)\)$", "$1|Ind"))
                .Select(x => Regex.Replace(x, @"\[([0-9]+)\]$", "$1|Ind"))
                .Select(x => x.Replace("First()", "0|Ind"))
                .ToArray();

            if (!_isDeepGraphWithArrays)
                _isDeepGraphWithArrays = fullName.Length - fixedName.Length != 1;

            var expressionRootType = GetParameterExpression(m.Expression);
            if (expressionRootType != null)
            {
                fixedName = GetDeepAlias(expressionRootType.Type, fixedName);
            }

            string result = string.Join(".", fixedName.Select(x => x.Replace("|Ind", "")).ToArray());

            return result;
        }

        private string VisitAlias(MemberExpression m)
        {
            var alias = MongoConfiguration.GetPropertyAlias(m.Expression.Type, m.Member.Name);
            var id = ReflectionHelper.GetHelperForType(m.Expression.Type).FindIdProperty();
            if (id != null && id.Name == alias)
            {
                alias = "_id";
            }

            return alias;
        }

        private void VisitDateTimeProperty(MemberExpression m)
        {
            if (m.Member.MemberType == MemberTypes.Property || m.Member.MemberType == MemberTypes.Field)
            {
                var property = m.Member as PropertyInfo;
                if (property != null && (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)))
                {
                    _sbWhere.Append("+");
                }
            }
        }

        private string GetOperator(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return "-";
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Not:
                    return IsBoolean(u.Operand.Type) ? "!" : "";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Visits a Unary call.
        /// </summary>
        /// <param retval="u">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitUnary(UnaryExpression u)
        {
            string op = GetOperator(u);
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    if (IsBoolean(u.Operand.Type))
                    {
                        _sbWhere.Append(op);
                        VisitPredicate(u.Operand, true);
                    }
                    else
                    {
                        _sbWhere.Append(op);
                        Visit(u.Operand);
                    }
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _sbWhere.Append(op);
                    Visit(u.Operand);
                    break;
                case ExpressionType.UnaryPlus:
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    // ignore conversions for now
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        private bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        private bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Convert:
                    return IsBoolean(expr.Type);
                default:
                    return false;
            }
        }

        private Expression VisitPredicate(Expression expr, bool IsNotOperator)
        {
            Visit(expr);
            if (IsPredicate(expr))
            {
                //_sbWhere.Append(" === true");
                SetFlyValue(!IsNotOperator);
            }
            return expr;
        }

        private Expression VisitPredicate(Expression expr)
        {
            return VisitPredicate(expr, false);
        }

        /// <summary>
        /// The get parameter expression.
        /// </summary>
        /// <param retval="expression">
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
                if (parentExpression.NodeType == ExpressionType.MemberAccess)
                {
                    parentExpression = ((MemberExpression)parentExpression).Expression;
                    expressionRoot = parentExpression is ParameterExpression;
                }
                else if (parentExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    parentExpression = ((BinaryExpression)parentExpression).Left;
                    expressionRoot = parentExpression is ParameterExpression;
                }
                else if (parentExpression.NodeType == ExpressionType.Call)
                {
                    var expr = ((MethodCallExpression)parentExpression).Arguments[0];
                    if (expr.NodeType == ExpressionType.MemberAccess)
                    {
                        parentExpression = ((MemberExpression)expr).Expression;
                    }
                    else if (expr.NodeType == ExpressionType.Constant)
                    {
                        parentExpression = ((MemberExpression)((MethodCallExpression)parentExpression).Object).Expression;
                    }

                    expressionRoot = parentExpression is ParameterExpression;
                }
                else
                {
                    expressionRoot = true;
                }

            }

            return (ParameterExpression)parentExpression;
        }

        private static string[] GetDeepAlias(Type type, string[] graph)
        {
            var graphParts = new string[graph.Length];
            var typeToQuery = type;

            for (var i = 0; i < graph.Length; i++)
            {
                if (graph[i].EndsWith("|Ind"))
                {
                    graphParts[i] = graph[i];
                    continue;
                }

                var property = BSON.ReflectionHelper.FindProperty(typeToQuery, graph[i]);
                graphParts[i] = MongoConfiguration.GetPropertyAlias(typeToQuery, graph[i]);

                if (property.PropertyType.IsGenericType)
                    typeToQuery = property.PropertyType.GetGenericArguments()[0];
                else
                    typeToQuery = property.PropertyType.HasElementType ? property.PropertyType.GetElementType() : property.PropertyType;
            }

            return graphParts;
        }

        private void VisitBinaryOperator(BinaryExpression b)
        {

            string currentOperator;

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    currentOperator = " & ";
                    IsComplex = true;
                    break;
                case ExpressionType.AndAlso:
                    currentOperator = " && ";
                    break;
                case ExpressionType.Or:
                    currentOperator = " | ";
                    IsComplex = true;
                    break;
                case ExpressionType.OrElse:
                    currentOperator = " || ";
                    IsComplex = true;
                    break;
                case ExpressionType.Equal:
                    _lastOperator = " === ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.NotEqual:
                    _lastOperator = " !== ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.LessThan:
                    _lastOperator = " < ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.LessThanOrEqual:
                    _lastOperator = " <= ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.GreaterThan:
                    _lastOperator = " > ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _lastOperator = " >= ";
                    currentOperator = _lastOperator;
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    _lastOperator = " + ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.Coalesce:
                    _lastOperator = " || ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.Divide:
                    _lastOperator = " / ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.ExclusiveOr:
                    _lastOperator = " ^ ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.LeftShift:
                    _lastOperator = " << ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    _lastOperator = " * ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.RightShift:
                    _lastOperator = " >> ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    _lastOperator = " - ";
                    currentOperator = _lastOperator;
                    IsComplex = true;
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            _sbWhere.Append(currentOperator);
        }

        /// <summary>
        /// Visits a binary expression.
        /// </summary>
        /// <param retval="b">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            ConditionalCount++;
            _sbWhere.Append("(");

            var hasVisited = false;
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (IsBoolean(b.Left.Type))
                    {
                        VisitPredicate(b.Left);
                        VisitBinaryOperator(b);
                        VisitPredicate(b.Right);

                        hasVisited = true;
                    }
                    break;
            }

            if (!hasVisited)
            {
                Visit(b.Left);
                VisitBinaryOperator(b);
                Visit(b.Right);
            }

            _sbWhere.Append(")");
            return b;
        }

        /// <summary>
        /// Visits a constant.
        /// </summary>
        /// <param retval="c">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q != null)
            {
                // set the collection retval
                TypeName = q.ElementType.Name;
                
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
                _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                SetFlyValue(null);
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.DateTime:
                        _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.String:
                        _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.Object:
                        if (c.Value is ObjectId)
                        {
                            if (_lastOperator == " === " || _lastOperator == " !== ")
                            {
                                _sbWhere.Remove(_sbWhere.Length - 2, 1);
                            }
                            _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                            SetFlyValue(c.Value);
                        }
                        else if (c.Value is Guid)
                        {
                            _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                            SetFlyValue(c.Value);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        }
                        break;
                    default:
                        _sbWhere.Append(GetJavaScriptConstantValue(c.Value));
                        SetFlyValue(c.Value);
                        break;
                }
            }

            return c;
        }

        private string GetJavaScriptConstantValue(object value)
        {
            if (value == null)
                return "null";

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return ((bool)value) ? "true" : "false";
                case TypeCode.DateTime:
                    return "+(" + (long)((DateTime)value).ToUniversalTime().Subtract(BsonHelper.EPOCH).TotalMilliseconds + ")";
                case TypeCode.String:
                    return "\"" + value.ToString().EscapeJavaScriptString() + "\"";
                case TypeCode.Object:
                    if (value is ObjectId || value is Guid)
                    {
                        return string.Format("\"{0}\"", value);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", value));
                    }
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Visits a method call.
        /// </summary>
        /// <param retval="m">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (string.IsNullOrEmpty(MethodCall))
            {
                MethodCall = m.Method.Name;
            }

            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        {
                            string value = m.Arguments[0].GetConstantValue<string>();

                            _sbWhere.Append("(");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".indexOf(\"{0}\")===0)", value.EscapeJavaScriptString());

                            SetFlyValue(new Regex("^" + Regex.Escape(value)));

                            return m;
                        }
                    case "EndsWith":
                        {
                            string value = m.Arguments[0].GetConstantValue<string>();

                            //_sbWhere.Append("(");
                            //Visit(m.Object);
                            //_sbWhere.AppendFormat(".match(\"{0}$\")==\"{0}\")", value.EscapeDoubleQuotes());

                            //Seems 10% quicker than above when complex query invoked
                            _sbWhere.Append("((");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".length - {0}) >= 0 && ", value.Length);
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".lastIndexOf(\"{0}\") === (", value.EscapeJavaScriptString());
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".length - {0}))", value.Length);

                            SetFlyValue(new Regex(Regex.Escape(value) + "$"));

                            return m;
                        }
                    case "Contains":
                        {
                            string value = m.Arguments[0].GetConstantValue<string>();

                            _sbWhere.Append("(");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".indexOf(\"{0}\")>-1)", value.EscapeJavaScriptString());

                            SetFlyValue(new Regex(Regex.Escape(value)));

                            return m;
                        }
                    case "IndexOf":
                        Visit(m.Object);
                        _sbWhere.Append(".indexOf(");
                        Visit(m.Arguments[0]);
                        _sbWhere.Append(")");
                        IsComplex = true;
                        return m;
                    case "LastIndexOf":
                        Visit(m.Object);
                        _sbWhere.Append(".lastIndexOf(");
                        Visit(m.Arguments[0]);
                        _sbWhere.Append(")");
                        IsComplex = true;
                        return m;
                    case "IsNullOrEmpty":
                        _sbWhere.Append("(");
                        Visit(m.Arguments[0]);
                        _sbWhere.Append(" == '' ||  ");
                        Visit(m.Arguments[0]);
                        _sbWhere.Append(" == null  )");
                        IsComplex = true;
                        return m;
                    case "ToLower":
                    case "ToLowerInvariant":
                        Visit(m.Object);
                        _sbWhere.Append(".toLowerCase()");
                        IsComplex = true;
                        return m;
                    case "ToUpper":
                    case "ToUpperInvariant":
                        Visit(m.Object);
                        _sbWhere.Append(".toUpperCase()");
                        IsComplex = true;
                        return m;
                    case "Substring":
                        Visit(m.Object);
                        _sbWhere.Append(".substr(");
                        Visit(m.Arguments[0]);
                        if (m.Arguments.Count == 2)
                        {
                            _sbWhere.Append(",");
                            Visit(m.Arguments[1]);
                        }
                        _sbWhere.Append(")");
                        IsComplex = true;
                        return m;
                    case "Replace":
                        Visit(m.Object);
                        _sbWhere.Append(".replace(new RegExp(");
                        _sbWhere.Append(GetJavaScriptConstantValue(Regex.Escape(m.Arguments[0].GetConstantValue<string>())));
                        _sbWhere.Append(",'g'),");
                        Visit(m.Arguments[1]);
                        _sbWhere.Append(")");
                        IsComplex = true;
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Regex))
            {
                if (m.Method.Name == "IsMatch")
                {
                    HandleRegexIsMatch(m);
                    return m;
                }

                throw new NotSupportedException(string.Format("Only the static Regex.IsMatch is supported.", m.Method.Name));
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && IsCallableMethod(m.Method.Name))
            {
                return HandleMethodCall(m);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(m.Method.DeclaringType))
            {
                if (m.Method.Name == "Contains")
                {
                    HandleContains(m);
                    return m;
                }

                throw new NotSupportedException(string.Format("Subqueries with {0} are not currently supported", m.Method.Name));
            }
            else if (typeof(Enumerable).IsAssignableFrom(m.Method.DeclaringType))
            {
                if (m.Method.Name == "Count" && m.Arguments.Count == 1)
                {
                    HandleSubCount(m);
                    return m;
                }
                if (m.Method.Name == "Any")
                {
                    HandleSubAny(m);
                    return m;
                }

                throw new NotSupportedException(string.Format("Subqueries with {0} are not currently supported", m.Method.Name));
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        private static HashSet<String> _callableMethods = new HashSet<string>(){
            "First","Single","FirstOrDefault","SingleOrDefault","Count",
            "Sum","Average","Min","Max","Any","Take","Skip", 
            "OrderBy","ThenBy","OrderByDescending","ThenByDescending","Where", "Select"};

        /// <summary>
        /// Determines if it's a callable method.
        /// </summary>
        /// <param retval="methodName">The method retval.</param>
        /// <returns>The is callable method.</returns>
        private static bool IsCallableMethod(string methodName)
        {
            return MongoQueryTranslator._callableMethods.Contains(methodName);
        }

        /// <summary>
        /// The set flyweight value.
        /// </summary>
        /// <param retval="value">The value.</param>
        private void SetFlyValue(object value)
        {
            if (_prefixAlias.Count > 0)
            {
                _lastFlyProperty = (string.Join(".", _prefixAlias.ToArray()) + "." + _lastFlyProperty).TrimEnd('.');
            }

            SetFlyValue(_lastFlyProperty, value);
        }

        private void SetFlyValue(string key, object value)
        {
            if (!CanGetQualifier(_lastOperator, value))
            {
                IsComplex = true;
                return;
            }

            if (FlyWeight.Contains(key))
            {
                var existing = FlyWeight[key] as Expando;
                if (existing != null)
                {
                    var newq = GetQualifier(_lastOperator, value) as Expando;
                    if (newq != null)
                    {
                        existing.Merge(newq);
                        return;
                    }
                }

                IsComplex = true;
                return;
            }

            FlyWeight[key] = GetQualifier(_lastOperator, value);
        }

        private bool CanGetQualifier(string op, object value)
        {
            if (op == " !== " || op == " === ")
                return true;

            if (value != null && (value.GetType().IsAssignableFrom(typeof(double))
                        || value.GetType().IsAssignableFrom(typeof(double?))
                        || value.GetType().IsAssignableFrom(typeof(int))
                        || value.GetType().IsAssignableFrom(typeof(int?))
                        || value.GetType().IsAssignableFrom(typeof(long))
                        || value.GetType().IsAssignableFrom(typeof(long?))
                        || value.GetType().IsAssignableFrom(typeof(float))
                        || value.GetType().IsAssignableFrom(typeof(float?))
                        || value.GetType().IsAssignableFrom(typeof(DateTime))
                        || value.GetType().IsAssignableFrom(typeof(DateTime?))))
            {
                switch (op)
                {
                    case " > ":
                    case " < ":
                    case " <= ":
                    case " >= ":
                        return true;
                }
            }

            return false;
        }

        private object GetQualifier(string op, object value)
        {
            switch (op)
            {
                case " === ":
                    return value;
                case " !== ":
                    return Q.NotEqual(value).AsExpando();
                case " > ":
                    return Q.GreaterThan(value).AsExpando();
                case " < ":
                    return Q.LessThan(value).AsExpando();
                case " <= ":
                    return Q.LessOrEqual(value).AsExpando();
                case " >= ":
                    return Q.GreaterOrEqual(value).AsExpando();
            }

            return null;
        }

        /// <summary>
        /// Handles skip.
        /// </summary>
        /// <param retval="exp">The expression.</param>
        private void HandleSkip(Expression exp)
        {
            Skip = exp.GetConstantValue<int>();
        }

        /// <summary>
        /// Handles take.
        /// </summary>
        /// <param retval="exp">The expression.</param>
        private void HandleTake(Expression exp)
        {
            Take = exp.GetConstantValue<int>();
        }

        private void HandleSort(Expression exp, OrderBy orderby)
        {
            var stripped = GetLambda(exp);
            var member = stripped.Body as MemberExpression;
            if (member == null)
                throw new NotSupportedException("Order clause supplied is not supported");

            SortFly[VisitDeepAlias(member)] = orderby;
        }

        private void HandleAggregate(MethodCallExpression exp)
        {
            if (exp.Arguments.Count == 2)
            {
                var stripped = GetLambda(exp.Arguments[1]);
                var member = stripped.Body as MemberExpression;
                if (member == null)
                    throw new NotSupportedException("Aggregate clause supplied is not supported");

                AggregatePropName = VisitDeepAlias(member);
            }
        }

        private void TranslateToWhere(MethodCallExpression exp)
        {
            if (exp.Arguments.Count == 2)
            {
                HandleWhere(exp.Arguments[1]);
            }
        }

        private void HandleWhere(Expression exp)
        {
            if (_whereWritten)
            {
                _sbWhere.Append(" && ");
            }

            VisitPredicate(GetLambda(exp).Body);
            _whereWritten = true;
        }

        private void HandleContains(MethodCallExpression m)
        {
            var collection = m.Object.GetConstantValue<IEnumerable>().Cast<object>().ToArray();
            var member = VisitDeepAlias((MemberExpression)m.Arguments[0]);

            if (collection.Length > 0)
            {
                _sbWhere.Append("(");
                foreach (var item in collection)
                {
                    if (UseScopedQualifier)
                        _sbWhere.Append("this.");

                    _sbWhere.Append(member);
                    _sbWhere.Append(" === ");
                    _sbWhere.Append(GetJavaScriptConstantValue(item));
                    _sbWhere.Append(" || ");
                }
                _sbWhere.Remove(_sbWhere.Length - 4, 4);
                _sbWhere.Append(")");
            }
            else
            {
                //Handle no items in the contains list
                _sbWhere.Append("(1===2)");
            }

            SetFlyValue(member, Q.In(collection).AsExpando());
        }

        private void HandleSubCount(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sbWhere.Append(".length");
            IsComplex = true;
        }

        private void HandleSubAny(MethodCallExpression m)
        {
            if (m.Arguments.Count == 1)
            {
                Visit(m.Arguments[0]);
                _sbWhere.Append(".length > 0");
                IsComplex = true;
            }
            else if (m.Arguments.Count == 2)
            {
                _prefixAlias.Add(VisitDeepAlias((MemberExpression)m.Arguments[0]));
                VisitPredicate(GetLambda(m.Arguments[1]).Body);
                _prefixAlias.RemoveAt(_prefixAlias.Count - 1);

                if (IsComplex)
                    throw new NotSupportedException("Subqueries with Any are not supported with complex queries");
            }
        }

        private void HandleRegexIsMatch(MethodCallExpression m)
        {
            var options = RegexOptions.None;
            var jsoptions = "g";
            if (m.Arguments.Count == 3)
            {
                options = m.Arguments[2].GetConstantValue<RegexOptions>();
                jsoptions = VisitRegexOptions(m, options);
            }

            string value = m.Arguments[1].GetConstantValue<string>();

            _sbWhere.AppendFormat("(new RegExp(\"{0}\",\"{1}\")).test(", value.EscapeJavaScriptString(), jsoptions);
            Visit(m.Arguments[0]);
            _sbWhere.Append(")");

            SetFlyValue(new Regex(value, options));
        }

        private static string VisitRegexOptions(MethodCallExpression m, RegexOptions options)
        {
            var allowedOptions = new[] { RegexOptions.IgnoreCase, RegexOptions.Multiline, RegexOptions.None };
            foreach (RegexOptions type in Enum.GetValues(typeof(RegexOptions)))
            {
                if ((options & type) == type && !allowedOptions.Contains(type))
                    throw new NotSupportedException(string.Format("Only the RegexOptions.Ignore and RegexOptions.Multiline options are supported.", m.Method.Name));
            }

            var jsoptions = "g";

            if ((options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
                jsoptions += "i";
            if ((options & RegexOptions.Multiline) == RegexOptions.Multiline)
                jsoptions += "m";

            return jsoptions;
        }

        private void HandleSelect(MethodCallExpression m)
        {
            SelectLambda = GetLambda(m.Arguments[1]);
            OriginalSelectType = SelectLambda.Parameters[0].Type;
        }

        /// <summary>
        /// The handle method call.
        /// </summary>
        /// <param retval="m">The expression.</param>
        /// <returns></returns>
        private Expression HandleMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Any":
                case "Single":
                case "SingleOrDefault":
                case "First":
                case "FirstOrDefault":
                case "Where":
                    TranslateToWhere(m);
                    break;
                case "OrderBy":
                case "ThenBy":
                    HandleSort(m.Arguments[1], OrderBy.Ascending);
                    break;
                case "OrderByDescending":
                case "ThenByDescending":
                    HandleSort(m.Arguments[1], OrderBy.Descending);
                    break;
                case "Skip":
                    HandleSkip(m.Arguments[1]);
                    break;
                case "Take":
                    HandleTake(m.Arguments[1]);
                    break;
                case "Min":
                case "Max":
                case "Sum":
                case "Average":
                    HandleAggregate(m);
                    break;
                case "Select":
                    HandleSelect(m);
                    break;
                default:
                    Take = 1;
                    MethodCall = m.Method.Name;
                    if (m.Arguments.Count > 1)
                    {
                        var lambda = GetLambda(m.Arguments[1]);
                        if (lambda != null)
                        {
                            Visit(lambda.Body);
                        }
                    }

                    break;
            }

            Visit(m.Arguments[0]);

            return m;
        }

        private static LambdaExpression GetLambda(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)e).Value as LambdaExpression;
            }
            return e as LambdaExpression;
        }
    }
}