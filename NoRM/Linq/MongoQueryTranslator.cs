using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using NoRM.BSON;
using NoRM.Configuration;

namespace NoRM.Linq {
    public class MongoQueryTranslator:ExpressionVisitor {
        
        Expression _expression;
        bool collectionSet = false;
        StringBuilder sb;
        StringBuilder sbIndexed;
        Flyweight fly;
        int _conditionals;
        bool _isComplex = false;

        public bool IsComplex {
            get {
                return _isComplex;
            }
        }

        public string OptimizedWhere {
            get {
                return sbIndexed.ToString();
            }
        }
        public int ConditionalCount {
            get {
                return _conditionals;
            }
        }
        public Flyweight FlyWeight {
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
            sbIndexed = new StringBuilder();
            fly = new Flyweight();
            this.Visit(exp);
            return sb.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            _conditionals++;
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" && ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _isComplex = true;
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
        string lastFlyProperty = "";
        string lastOperator = " == ";
        
        void SetFlyValue(object value) {

            //if the property has already been set, we can't set it again
            //as fly uses Dictionaries. This means to BETWEEN style native queries
            if (fly.Contains(lastFlyProperty)) {
                _isComplex = true;
                return;
            }
            
            if (lastFlyProperty != " == ") {
                //Can't do comparisons here unless the type is a double
                //which is a limitation of mongo, apparently
                //and won't work if we're doing date comparisons
                if (value.GetType().IsAssignableFrom(typeof(double))) {
                    switch (lastOperator) {
                        case (" > "):
                            fly[lastFlyProperty] = Q.GreaterThan((double)value);
                            break;
                        case (" < "):
                            fly[lastFlyProperty] = Q.LessThan((double)value);
                            break;
                        case (" <= "):
                            fly[lastFlyProperty] = Q.LessOrEqual((double)value);
                            break;
                        case (" >= "):
                            fly[lastFlyProperty] = Q.GreaterOrEqual((double)value);
                            break;
                        case (" <> "):
                            fly[lastFlyProperty] = Q.NotEqual(value);
                            break;
                    }
                } else {

                    //Can't assign? Push to the $where
                    _isComplex = true;
                }
            } else {
                fly[lastFlyProperty] = value;
            }
        }
        protected override Expression VisitConstant(ConstantExpression c) {
            IQueryable q = c.Value as IQueryable;
            if (q != null) {
                //set the collection name
                fly.TypeName = q.ElementType.Name;
            } else if (c.Value == null) {
                sb.Append("NULL");
            } else {
                switch (Type.GetTypeCode(c.Value.GetType())) {
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
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        SetFlyValue(c.Value);
                        break;
                }
            }
            return c;
        }
        public string TranslateCollectionName(Expression exp) {
            ConstantExpression c = null;
            if (exp.NodeType == ExpressionType.Constant) {
                c = (ConstantExpression)exp;
            }else if (exp.NodeType == ExpressionType.Call) {
                var m = (MethodCallExpression)exp;
                c = m.Arguments[0] as ConstantExpression;
            }

            var result = "";

            //the first argument is a Constant - it's the query itself
            IQueryable q = c.Value as IQueryable;
            result = q.ElementType.Name;

            return result;
        }
        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if(string.IsNullOrEmpty(fly.MethodCall))
                fly.MethodCall = m.Method.Name;

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where") {
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            } else if (m.Method.DeclaringType == typeof(string)){
                _isComplex = true;
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
            } else if (m.Method.DeclaringType == typeof(Queryable) && IsCallableMethod(m.Method.Name)) {
                return this.HandleMethodCall((MethodCallExpression)m);
            }

            //for now...
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        bool IsCallableMethod(string methodName) {
            var acceptableMethods = new string[]{
                "First",
                "Single",
                "FirstOrDefault",
                "SingleOrDefault",
                "Count",
                "Sum",
                "Average",
                "Min",
                "Max",
                "Any"

            };
            return acceptableMethods.Any(x=>x==methodName);
        }
        Expression HandleMethodCall(MethodCallExpression m) {
            fly.Limit = 1;
            fly.MethodCall = m.Method.Name;
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
        protected override Expression VisitMemberAccess(MemberExpression m) {

            var fullName = m.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                var alias = MongoConfiguration.GetPropertyAlias(m.Expression.Type, m.Member.Name);
                sb.Append("this." + alias);// m.Member.Name);
                lastFlyProperty = alias;// m.Member.Name;
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
                
                //this is complex
                _isComplex = true;

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
                        sb.Append(".getFullYear()");
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
                //this supports the "deep graph" name - "Product.Address.City"
                var fixedName = fullName.Skip(1).Take(fullName.Length - 1).ToArray();


                var expressionRootType = GetParameterExpression((MemberExpression)m.Expression);

                if (expressionRootType != null)
                {
                    fixedName = GetDeepAlias(expressionRootType.Type, fixedName);
                }

                var result = String.Join(".", fixedName);
                sb.Append("this." + result);
                lastFlyProperty = result;
                return m;

            }
            //if this is a property NOT on the object...
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        private static ParameterExpression GetParameterExpression(MemberExpression expression)
        {
            var expressionRoot = false;
            Expression parentExpression = expression;

            while(!expressionRoot)
            {
                parentExpression = ((MemberExpression)(parentExpression)).Expression;
                expressionRoot = parentExpression is ParameterExpression;
            }

            return (ParameterExpression)parentExpression;
        }

        private static string[] GetDeepAlias(Type type, string[] graph)
        {
            var graphParts = new string[graph.Length];
            var typeToQuery = type;

            for (var i = 0; i <graph.Length; i++)
            {
                var prpperty = BSON.TypeHelper.FindProperty(typeToQuery, graph[i]);
                graphParts[i] = MongoConfiguration.GetPropertyAlias(typeToQuery, graph[i]);
                typeToQuery = prpperty.PropertyType;
            }

            return graphParts;
        }
    }

}
