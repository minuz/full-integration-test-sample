using IntegrationTests.Orderers;
using Xunit;

namespace IntegrationTests
{
    [CollectionDefinition(nameof(BootstrapCollection))]
    [CollectionOrder(0)]
    public class BootstrapCollection : ICollectionFixture<BootstrapFixture>
    {
    }

    [CollectionDefinition(nameof(IntegrationTestCollection))]
    [CollectionOrder(1)]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {

    }
}
