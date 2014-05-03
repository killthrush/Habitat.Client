using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Habitat.Client.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // this is to allow Moq to create instances of internal interfaces
