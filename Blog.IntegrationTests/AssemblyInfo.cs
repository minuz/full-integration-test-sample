
using IntegrationTests.Orderers;
using Xunit;

[assembly: TestCollectionOrderer(CollectionOrderer.TypeName, CollectionOrderer.AssemblyName)]
[assembly: TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]

// Need to turn off test parallelization so we can validate the run order
[assembly: CollectionBehavior(DisableTestParallelization = true)]