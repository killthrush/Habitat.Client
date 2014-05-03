using System.Collections.Generic;

namespace Habitat.Client.Tests.TestClasses
{
    internal class SampleApplicationConfig
    {
        public string ConnectionString { get; set; }
        public string RestUrl { get; set; }
        public int Timeout { get; set; }
        public CompositeConfigObject ConfigObject { get; set; }
        public Dictionary<string, int> KeyValues { get; set; }
    }
}