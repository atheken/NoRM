using System;
using Norm.BSON.TypeConverters;
using Xunit;

namespace NoRM.Tests.BSON.TypeConverters
{
    public class TimeSpanTypeConverterTests
    {
        [Fact]
        public void Converter_serialised_type_is_a_long()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();

            // Assert
            Assert.True(converter.SerializedType == typeof(long));
        }

        [Fact]
        public void Converts_TimeSpan_to_a_long()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            var timeSpan = new TimeSpan(8, 5, 3);

            // Act
            var result = converter.ConvertToBson(timeSpan);

            // Assert
            Assert.True(result is long);
        }

        [Fact]
        public void Conversion_to_BSON_does_not_swallow_invalid_type()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            const int integer = 5;

            // Act/Assert
            Assert.Throws<InvalidCastException>(() => converter.ConvertToBson(integer));
        }

        [Fact]
        public void Converts_a_long_to_Timespan()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            const long value = 4234234234;

            // Act
            var result = converter.ConvertFromBson(value);

            // Assert
            Assert.True(result is TimeSpan);
        }

        [Fact]
        public void Conversion_from_BSON_does_not_swallow_invalid_type()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            var obj = new Object();

            // Act/Assert
            Assert.Throws<InvalidCastException>(() => converter.ConvertFromBson(obj));
        }

        [Fact]
        public void Conversion_to_BSON_produces_correct_value()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            var timeSpan = new TimeSpan(8, 5, 3);

            // Act
            var result = (long)converter.ConvertToBson(timeSpan);

            // Assert
            Assert.Equal(timeSpan.Ticks, result);
        }

        [Fact]
        public void Conversion_from_BSON_produces_correct_value()
        {
            // Arrange
            var converter = new TimeSpanTypeConverter();
            const long value = 65435432112312;
            var timeSpan = TimeSpan.FromTicks(value);

            // Act
            var result = (TimeSpan)converter.ConvertFromBson(value);

            // Assert
            Assert.Equal(timeSpan, result);
        }
    }
}
