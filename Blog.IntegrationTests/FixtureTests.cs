using IntegrationTests.Orderers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    [Collection(nameof(BootstrapCollection))]
    [CollectionOrder(0)]
    public class BootstrapTests
    {
        private readonly BootstrapFixture _fixture;

        public BootstrapTests(BootstrapFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void OneTest()
        {
            Assert.NotNull(_fixture);
        }
    }
    
    [Collection(nameof(IntegrationTestCollection))]
    [CollectionOrder(1)]
    public class FixtureTests : IClassFixture<TestClassFixture>
    {
        private readonly IntegrationTestFixture _integrationTestFixture;
        private readonly TestClassFixture _testClassFixture;

        public FixtureTests(IntegrationTestFixture integrationTestFixture, TestClassFixture testClassFixture)
        {
            _integrationTestFixture = integrationTestFixture;
            _testClassFixture = testClassFixture;
        }


        [Fact]
        public async Task Test1()
        {
            var client = _testClassFixture.CreateClient();
            var response = await client.GetAsync("/api/values");
            var values = JsonConvert.DeserializeObject<ICollection<string>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, values.Count);
            Assert.NotNull(_integrationTestFixture);
            Assert.NotNull(_testClassFixture);
        }
    }


    [Collection(nameof(IntegrationTestCollection))]
    [CollectionOrder(2)]
    public class AnotherTestSet : IClassFixture<TestClassFixture>
    {
        private readonly IntegrationTestFixture _integrationTestFixture;
        private readonly TestClassFixture _testClassFixture;

        public AnotherTestSet(IntegrationTestFixture integrationTestFixture, TestClassFixture testClassFixture)
        {
            _integrationTestFixture = integrationTestFixture;
            _testClassFixture = testClassFixture;
        }


        [Fact]
        public void Test1()
        {
            Assert.NotNull(_integrationTestFixture);
            Assert.NotNull(_testClassFixture);
        }
    }
}
