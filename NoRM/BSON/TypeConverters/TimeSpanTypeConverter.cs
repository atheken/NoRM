using System;

namespace Norm.BSON.TypeConverters
{
    /// <summary>
    /// Converts TimeSpan to/from long for storage.
    /// </summary>
    public class TimeSpanTypeConverter : IBsonTypeConverter
    {
        #region IBsonTypeConverter Members
        
        public Type SerializedType
        {
            get { return typeof(long); }
        }

        public object ConvertToBson(object data)
        {
            return ((TimeSpan)data).Ticks;
        }

        public object ConvertFromBson(object data)
        {
            return TimeSpan.FromTicks((long)data);
        }

        #endregion
    }
}
