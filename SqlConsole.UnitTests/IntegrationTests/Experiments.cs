using SqlConsole.Host;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Towel;

using Xunit;
using Xunit.Abstractions;

namespace SqlConsole.UnitTests.IntegrationTests
{
    public class Experiments
    {
        ITestOutputHelper _output;
        public Experiments(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Experiment1()
        {
            foreach (var provider in Provider.All)
            {
                _output.WriteLine(provider.Name);
                foreach (var p in provider.ConnectionConfigurationProperties())
                {
                    var doc = p.GetDescriptionFromDocumentation();
                    _output.WriteLine(p.Name + ": " + doc);
                }

            }
        }
    }
}
