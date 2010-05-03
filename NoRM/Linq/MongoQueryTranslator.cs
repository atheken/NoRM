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
    public class MongoQueryTranslator : ExpressionVisitor
    {
        /// <summary>TODO::Description.</summary>
        private int _takeCount = Int32.MaxValue;

        /// <summary>TODO::Description.</summary>
        private string _lastFlyProperty = string.Empty;

        /// <summary>TODO::Description.</summary>
        private string _lastOperator = " === ";

        /// <summary>TODO::Description.</summary>
        private StringBuilder _sbWhere;

        /// <summary>TODO::Description.</summary>
        private StringBuilder _sbIndexed;

        /// <summary>TODO::Description.</summary>
        public Expando SortFly { get; set; }

        /// <summary>TODO::Description.</summary>
        public string SortDescendingBy { get; set; }

        /// <summary>TODO::Description.</summary>
        bool _whereWritten = false;

        /// <summary>TODO::Description.</summary>
        bool _isDeepGraphWithArrays = false;

        /// <summary>TODO::Description.</summary>
        public String AggregatePropName
        {
            get;
            set;
        }

        /// <summary>TODO::Description.</summary>
        public String TypeName
        {
            get;
            set;
        }

        /// <summary>TODO::Description.</summary>
        public string CollectionName{ get; set;}

        /// <summary>TODO::Description.</summary>
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
            get { return _sbIndexed.ToString(); }
        }

        /// <summary>
        /// Gets conditional count.
        /// </summary>
        public int ConditionalCount { get; private set; }

        /// <summary>
        /// Gets FlyWeight.
        /// </summary>
        public Expando FlyWeight { get; private set; }

        /// <summary>
        /// How many to skip.
        /// </summary>
        public int Skip { get; set; }


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
            get { return _sbWhere.ToString(); }
        }

        /// <summary>TODO::Description.</summary>
        public bool UseScopedQualifier { get; set; }

        /// <summary>
        /// Translates LINQ to MongoDB.
        /// </summary>
        /// <param name="exp">The expression.</param>
        /// <returns>The translate.</returns>
        public string Translate(Expression exp)
        {
            return Translate(exp, true);
        }

        /// <summary>TODO::Description.</summary>
        public string Translate(Expression exp, bool useScopedQualifier)
        {
            UseScopedQualifier = useScopedQualifier;
            _sbWhere = new StringBuilder();
            _sbIndexed = new StringBuilder();
            FlyWeight = new Expando();
            SortFly = new Expando();

            Visit(exp);

            ProcessGuards();
            TransformToFlyWeightWhere();

            return WhereExpression;
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
        /// <param name="m">The expression.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                var alias = MongoConfiguration.GetPropertyAlias(m.Expression.Type, m.Member.Name);
                var id = TypeHelper.GetHelperForType(m.Expression.Type).FindIdProperty();
                if (id != null && id.Name == alias)
                {
                    alias = "_id";
                }

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
                // this supports the "deep graph" name - "Product.Address.City"
                var fullName = m.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                
                var fixedName = fullName
                    .Skip(1)
                    .Where(x => x != "First()")
                    .Where(x => !x.StartsWith("get_Item("))
                    .Select(x => Regex.Replace(x, @"\[[0-9]+\]$", ""))
                    .ToArray();

                if (!_isDeepGraphWithArrays)
                    _isDeepGraphWithArrays = fullName.Length - fixedName.Length != 1;

                var expressionRootType = GetParameterExpression(m.Expression);
                if (expressionRootType != null)
                {
                    fixedName = GetDeepAlias(expressionRootType.Type, fixedName);
                }

                if (UseScopedQualifier)
                {
                    _sbWhere.Append("this.");
                }

                string result = string.Join(".", fixedName);
                
                _sbWhere.Append(result);
                _lastFlyProperty = result;

                return m;
            }

            // if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
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
                var property = BSON.TypeHelper.FindProperty(typeToQuery, graph[i]);
                graphParts[i] = MongoConfiguration.GetPropertyAlias(typeToQuery, graph[i]);

                if (property.PropertyType.IsGenericType)
                    typeToQuery = property.PropertyType.GetGenericArguments()[0];
                else 
                    typeToQuery = property.PropertyType.HasElementType ? property.PropertyType.GetElementType() : property.PropertyType;
            }

            return graphParts;
        }

        /// <summary>TODO::Description.</summary>
        private void VisitBinaryOperator(BinaryExpression b) {

            switch (b.NodeType) {
                case ExpressionType.And:
                    _lastOperator = " & ";
                    IsComplex = true;
                    break;
                case ExpressionType.AndAlso:
                    _lastOperator = " && ";
                    break;
                case ExpressionType.Or:
                    _lastOperator = " | ";
                    IsComplex = true;
                    break;
                case ExpressionType.OrElse:
                    _lastOperator = " || ";
                    IsComplex = true;
                    break;
                case ExpressionType.Equal:
                    _lastOperator = " === ";
                    break;
                case ExpressionType.NotEqual:
                    _lastOperator = " !== ";
                    break;
                case ExpressionType.LessThan:
                    _lastOperator = " < ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    _lastOperator = " <= ";
                    break;
                case ExpressionType.GreaterThan:
                    _lastOperator = " > ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _lastOperator = " >= ";
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    _lastOperator = " + ";
                    IsComplex = true;
                    break;
                case ExpressionType.Coalesce:
                     _lastOperator = " || ";
                    IsComplex = true;
                    break;
                case ExpressionType.Divide:
                     _lastOperator = " / ";
                    IsComplex = true;
                    break;
                case ExpressionType.ExclusiveOr:
                     _lastOperator = " ^ ";
                    IsComplex = true;
                    break;
                case ExpressionType.LeftShift:
                     _lastOperator = " << ";
                    IsComplex = true;
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                     _lastOperator = " * ";
                    IsComplex = true;
                    break;
                case ExpressionType.RightShift:
                     _lastOperator = " >> ";
                    IsComplex = true;
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                     _lastOperator = " - ";
                    IsComplex = true;
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            _sbWhere.Append(_lastOperator);
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
            _sbWhere.Append("(");
            Visit(b.Left);
            VisitBinaryOperator(b);
            Visit(b.Right);
            _sbWhere.Append(")");
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
                this.CollectionName = MongoConfiguration.GetCollectionName(q.ElementType);                

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
                _sbWhere.Append("null");
                SetFlyValue(null);
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sbWhere.Append(((bool)c.Value) ? 1 : 0);
                        SetFlyValue(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.DateTime:
                        var val = "new Date(" + (long)((DateTime)c.Value).Subtract(BsonHelper.EPOCH).TotalMilliseconds + ")";
                        _sbWhere.Append(val);
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.String:
                        var sval = "\"" + c.Value.ToString().EscapeDoubleQuotes() + "\"";
                        _sbWhere.Append(sval);
                        SetFlyValue(c.Value);
                        break;
                    case TypeCode.Object:
                        if (c.Value is ObjectId)
                        {
                            if (_lastOperator == " === " || _lastOperator == " !== ")
                            {
                                _sbWhere.Remove(_sbWhere.Length - 2, 1);
                            }
                            _sbWhere.AppendFormat("'{0}'", c.Value);
                            SetFlyValue(c.Value);
                        }
                        else if (c.Value is Guid)
                        {
                            _sbWhere.AppendFormat("'{0}'", c.Value);
                            SetFlyValue(c.Value);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        }
                        break;
                    default:
                        _sbWhere.Append(c.Value);
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

            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        {
                            string value = (string)m.Arguments[0].GetConstantValue();

                            _sbWhere.Append("(");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".indexOf(\"{0}\")===0)", value.EscapeDoubleQuotes());
  
                            SetFlyValue(new Regex("^" + value));

                            return m;
                        }
                    case "EndsWith":
                        {
                            string value = (string)m.Arguments[0].GetConstantValue();

                            //_sbWhere.Append("(");
                            //Visit(m.Object);
                            //_sbWhere.AppendFormat(".match(\"{0}$\")==\"{0}\")", value.EscapeDoubleQuotes());

                            //Seems 10% quicker than above when complex query invoked
                            _sbWhere.Append("((");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".length - {0}) >= 0 && ", value.Length);
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".lastIndexOf(\"{0}\") === (", value.EscapeDoubleQuotes());
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".length - {0}))", value.Length);

                            SetFlyValue(new Regex(value + "$"));

                            return m;
                        }
                    case "Contains":
                        {
                            string value = (string)m.Arguments[0].GetConstantValue();

                            _sbWhere.Append("(");
                            Visit(m.Object);
                            _sbWhere.AppendFormat(".indexOf(\"{0}\")>-1)", value.EscapeDoubleQuotes());
                            
                            SetFlyValue(new Regex(value));

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
                        Visit(m.Arguments[0]);
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
                    return HandleMethodCall(m);
                }
                throw new NotSupportedException(string.Format("Subqueries with {0} are not currently supported", m.Method.Name));
            }
            else if (typeof(Enumerable).IsAssignableFrom(m.Method.DeclaringType))
            {
                if (m.Method.Name == "Count" && m.Arguments.Count == 1)
                {
                    return HandleMethodCall(m);
                }

                throw new NotSupportedException(string.Format("Subqueries with {0} are not currently supported", m.Method.Name));
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        private static HashSet<String> _callableMethods = new HashSet<string>(){
            "First","Single","FirstOrDefault","SingleOrDefault","Count",
            "Sum","Average","Min","Max","Any","Take","Skip", 
            "OrderBy","ThenBy","OrderByDescending","ThenByDescending","Where"};

        /// <summary>
        /// Determines if it's a callable method.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <returns>The is callable method.</returns>
        private static bool IsCallableMethod(string methodName)
        {
            return MongoQueryTranslator._callableMethods.Contains(methodName);
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
        /// The set flyweight value.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetFlyValue(object value)
        {
            // if the property has already been set, we can't set it again
            // as fly uses Dictionaries. This means you can't do BETWEEN style native queries
            if (FlyWeight.Contains(_lastFlyProperty))
            {
                IsComplex = true;
                return;
            }

            switch (_lastOperator)
            {
                case " !== ":
                    FlyWeight[_lastFlyProperty] = Q.NotEqual(value);
                    break;
                case " === ":
                    FlyWeight[_lastFlyProperty] = value;
                    break;
                default:
                    // Can't do comparisons here unless the type is a double
                    // which is a limitation of mongo, apparently
                    // and won't work if we're doing date comparisons
                    if (value != null && value.GetType().IsAssignableFrom(typeof(double)))
                    {
                        switch (_lastOperator)
                        {
                            case " > ":
                                FlyWeight[_lastFlyProperty] = Q.GreaterThan((double)value);
                                break;
                            case " < ":
                                FlyWeight[_lastFlyProperty] = Q.LessThan((double)value);
                                break;
                            case " <= ":
                                FlyWeight[_lastFlyProperty] = Q.LessOrEqual((double)value);
                                break;
                            case " >= ":
                                FlyWeight[_lastFlyProperty] = Q.GreaterOrEqual((double)value);
                                break;
                        }
                    }
                    else
                    {
                        // Can't assign? Push to the $where
                        IsComplex = true;
                    }
                    break;
            }

        }

        /// <summary>
        /// Handles skip.
        /// </summary>
        /// <param name="exp">The expression.</param>
        private void HandleSkip(Expression exp)
        {
            this.Skip = (int)exp.GetConstantValue();
        }

        /// <summary>
        /// Handles take.
        /// </summary>
        /// <param name="exp">The expression.</param>
        private void HandleTake(Expression exp)
        {
            this.Take = (int)exp.GetConstantValue();
        }

        private void HandleSort(Expression exp, OrderBy orderby)
        {
            var stripped = (LambdaExpression)StripQuotes(exp);
            var member = (MemberExpression)stripped.Body;
            this.SortFly[member.Member.Name] = orderby;
        }

        private void HandleAggregate(MethodCallExpression exp)
        {
            if (exp.Arguments.Count == 2)
            {
                var stripped = (LambdaExpression)StripQuotes(exp.Arguments[1]);
                var member = (MemberExpression)stripped.Body;
                AggregatePropName = member.Member.Name;
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
                IsComplex = true;
            }
           
            Visit(exp);
            _whereWritten = true;
        }

        private void HandleContains(MethodCallExpression m)
        {
            var collection = (IEnumerable)m.Object.GetConstantValue();

            _sbWhere.Append("(");
            foreach (var item in collection)
            {
                Visit(m.Arguments[0]);
                _sbWhere.Append(" === ");
                Visit(Expression.Constant(item));
                _sbWhere.Append(" || ");
            }
            _sbWhere.Remove(_sbWhere.Length - 4, 4);
            _sbWhere.Append(")");
        }

        private void HandleSubCount(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);
            _sbWhere.Append(".length");
        }

        private void HandleRegexIsMatch(MethodCallExpression m)
        {
            var options = RegexOptions.None;
            var jsoptions = "g";
            if (m.Arguments.Count == 3)
            {
                options = (RegexOptions)m.Arguments[2].GetConstantValue();
                jsoptions = VisitRegexOptions(m, options);
            }

            string value = (string)m.Arguments[1].GetConstantValue();

            _sbWhere.AppendFormat("(new RegExp(\"{0}\",\"{1}\")).test(", value.EscapeDoubleQuotes(), jsoptions);
            Visit(m.Arguments[0]);
            _sbWhere.Append(")");

            SetFlyValue(new Regex(value, options));
        }

        private static string VisitRegexOptions(MethodCallExpression m, RegexOptions options)
        {
            RegexOptions[] allowedOptions = new RegexOptions[] { RegexOptions.IgnoreCase, RegexOptions.Multiline, RegexOptions.None };
            foreach (RegexOptions Type in Enum.GetValues(typeof(RegexOptions)))
            {
                if ((options & Type) == Type && !allowedOptions.Contains(Type))
                    throw new NotSupportedException(string.Format("Only the RegexOptions.Ignore and RegexOptions.Multiline options are supported.", m.Method.Name));
            }

            var jsoptions = "g";

            if ((options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
                jsoptions += "i";
            if ((options & RegexOptions.Multiline) == RegexOptions.Multiline)
                jsoptions += "m";

            return jsoptions;
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
                case "Any":
                case "Single":
                case "SingleOrDefault":
                case "First":
                case "FirstOrDefault":
                case "Where":
                    TranslateToWhere(m);
                    break;
                case "Contains":
                    HandleContains(m);
                    return m;
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
                case "Count":
                    HandleSubCount(m);
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