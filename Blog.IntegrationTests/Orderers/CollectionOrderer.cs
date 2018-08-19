using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Orderers
{
    public class CollectionOrderer: ITestCollectionOrderer
    {
        public const string TypeName = "IntegrationTests.Orderers.CollectionOrderer";

        public const string AssemblyName = "IntegrationTests";

        /// <inheritdoc />
        /// <summary>Orders test collections for execution.</summary>
        /// <param name="testCollections">The test collections to be ordered.</param>
        /// <returns>The test collections in the order to be run.</returns>
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(GetOrder);
        }

        /// <summary>
        /// Test collections are not bound to a specific class, however they
        /// are named by default with the type name as a suffix. We try to
        /// get the class name from the DisplayName and then use reflection to
        /// find the class and OrderAttribute.
        /// </summary>
        private static int GetOrder(ITestCollection testCollection)
        {
            var collectionName = testCollection.CollectionDefinition.Name;
            var collectionType = Type.GetType(collectionName);
            if (collectionType != null)
            {
                var att = collectionType.GetCustomAttributes<CollectionOrderAttribute>();
                return att?.FirstOrDefault()?.Order ?? 100;
            }

            return 100;
        }
    }
}
