using SqlConsole.Host;

using System.Data.Common;
using System.Linq;

using Xunit;

namespace SqlConsole.UnitTests.Configuration
{
    public class ProviderTests
    {
        [Fact]
        public void ConnectionStringProperties_ReturnsInheritedPropertiesButNotPropertiesOfDbConnectionStringBuilder()
        {
            var provider = new Provider<TestDbConnectionStringBuilder>("test", null, null);
            var properties = provider.ConnectionConfigurationProperties();
            Assert.Equal(new[] { "DerivedProperty", "SomeProperty" }, properties.Select(p => p.Name).ToArray());
        }
        class TestDbConnectionStringBuilderBase : DbConnectionStringBuilder
        {
            public string SomeProperty { get; set; }
        }

        class TestDbConnectionStringBuilder : TestDbConnectionStringBuilderBase
        {
            public string DerivedProperty { get; set; }
        }
    }
}
