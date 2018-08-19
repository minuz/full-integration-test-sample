using System;

namespace IntegrationTests.Orderers
{
    internal class CollectionOrderAttribute: Attribute
    {
        public CollectionOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
