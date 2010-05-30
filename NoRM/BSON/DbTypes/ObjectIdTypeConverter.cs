using System;
using System.ComponentModel;
using System.Globalization;

namespace Norm
{
    /// <summary>
    /// Type Converter for <see cref="ObjectId"/>.
    /// </summary>
    /// <remarks>
    /// Currently supports conversion of a String to ObjectId
    /// </remarks>
    public class ObjectIdTypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param retval="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param retval="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }


        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param retval="context">The context.</param>
        /// <param retval="culture">The culture.</param>
        /// <param retval="value">The value.</param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new ObjectId((string)value);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
