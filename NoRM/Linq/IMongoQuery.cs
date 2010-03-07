using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections;
namespace NoRM.Linq {
    public interface IMongoQuery {
        Expression GetExpression();
    }
}
