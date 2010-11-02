using System;
using System.Reflection;

namespace Norm.BSON
{
    public class MagicPropertyConfiguration
    {
        public PropertyInfo Property { get; internal set; }

        public object DefaultValue { get; internal set; }
        public bool? HasDefaultValue { get; internal set; }
        public bool? IgnoreIfNull { get; internal set; }

        public Func<object, object> CustomGetter { get; internal set; }
        public Action<object, object> CustomSetter { get; internal set; }
        public Func<object, bool> CustomShouldSerializeRule { get; internal set; }
    }
}
