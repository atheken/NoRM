namespace NoRM
{
    using System.Collections.Generic;

    public interface IExpando
    {
        IDictionary<string, object> Expando{ get;}
    }
}