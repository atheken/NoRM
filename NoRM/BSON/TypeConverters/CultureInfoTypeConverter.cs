using System;
using System.Globalization;


namespace Norm.BSON.TypeConverters
{
    public class CultureInfoTypeConverter : IBsonTypeConverter
    {
        #region IBsonTypeConverter Members

        public Type SerializedType
        {
            get { return typeof(string); }
        }

        public object ConvertToBson(object data)
        {
            return ((CultureInfo)data).Name;
        }

        public object ConvertFromBson(object data)
        {
            return new CultureInfo((string)data);
        }

        #endregion
    }
}
