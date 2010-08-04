using System;


namespace Norm.BSON
{
    public interface IBsonTypeConverter
    {
        Type SerializedType { get; }
        object ConvertToBson(object data);
        object ConvertFromBson(object data);
    }
}
