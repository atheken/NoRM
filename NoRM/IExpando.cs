using System.Collections.Generic;

namespace Norm
{
    public interface IExpando
    {
        IDictionary<string, object> Expando { get; }
    }
}