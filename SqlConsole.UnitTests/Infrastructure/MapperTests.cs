using SqlConsole.Host.Infrastructure;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure
{
    class Source
    {
        public int IntProperty { get; set; }
        public int? NullableInSourceNonNullableInTargetIntProperty { get; set; }
        public string StringProperty { get; set; }
        public decimal? NullableDecimalProperty { get; set; }
        public string GetOnly { get; }
    }
    class Target
    {
        public int IntProperty { get; set; }
        public int NullableInSourceNonNullableInTargetIntProperty { get; set; }
        public string StringProperty { get; set; }
        public decimal? NullableDecimalProperty { get; set; }
        public string NotInSource { get; set; }
    }

    public class MapperTests
    {
        private (PropertyInfo sourceproperty, PropertyInfo targetproperty)[] _properties;

        public MapperTests()
        {
            _properties = Mapper.GetProperties<Source, Target>();

        }

        [Fact]
        public void GetProperties_All_expected_properties_Are_Present()
        {
            var expected = new[] 
            {
                "IntProperty", "NullableInSourceNonNullableInTargetIntProperty", "StringProperty", "NullableDecimalProperty"
            };
            Assert.Equal(4, _properties.Count());
            Assert.Equal(expected, _properties.Select(p => p.sourceproperty.Name));
            Assert.Equal(expected, _properties.Select(p => p.sourceproperty.Name));
        }

        [Fact]
        public void WhenNoPropertiesAreExplicitlySetInSource_OnlyValueTypesAreCopiedToTarget()
        {
            var source = new Source();
            var target = Mapper.Map<Source, Target>(source);

            Assert.Equal(default, target.IntProperty);
            Assert.Equal(default, target.NullableInSourceNonNullableInTargetIntProperty);
            Assert.Equal(default, target.StringProperty);
            Assert.Equal(default, target.NullableDecimalProperty);
        }
        [Fact]
        public void WhenRefTypeSetInSource_ItIsCopiedToTarget()
        {
            var source = new Source();
            source.StringProperty = "foo";
            var target = Mapper.Map<Source, Target>(source);

            Assert.Equal(default, target.IntProperty);
            Assert.Equal(default, target.NullableInSourceNonNullableInTargetIntProperty);
            Assert.Equal("foo", target.StringProperty);
            Assert.Equal(default, target.NullableDecimalProperty);
        }
        [Fact]
        public void WhenValueTypeSetInSource_ItIsCopiedToTarget()
        {
            var source = new Source();
            source.IntProperty = 42;
            var target = Mapper.Map<Source, Target>(source);

            Assert.Equal(42, target.IntProperty);
            Assert.Equal(default, target.NullableInSourceNonNullableInTargetIntProperty);
            Assert.Equal(default, target.StringProperty);
            Assert.Equal(default, target.NullableDecimalProperty);
        }
        [Fact]
        public void WhenNullableValueTypeSetInSource_ItIsCopiedToTarget()
        {
            var source = new Source();
            source.NullableDecimalProperty = 42m;
            var target = Mapper.Map<Source, Target>(source);

            Assert.Equal(default, target.IntProperty);
            Assert.Equal(default, target.NullableInSourceNonNullableInTargetIntProperty);
            Assert.Equal(default, target.StringProperty);
            Assert.Equal(42m, target.NullableDecimalProperty);
        }
        [Fact]
        public void WhenNullableValueTypeSetInSource_ItIsCopiedToTargetIfTargetIsNon()
        {
            var source = new Source();
            source.NullableInSourceNonNullableInTargetIntProperty = 42;
            var target = Mapper.Map<Source, Target>(source);

            Assert.Equal(default, target.IntProperty);
            Assert.Equal(42, target.NullableInSourceNonNullableInTargetIntProperty);
            Assert.Equal(default, target.StringProperty);
            Assert.Equal(default, target.NullableDecimalProperty);
        }
    }
}
