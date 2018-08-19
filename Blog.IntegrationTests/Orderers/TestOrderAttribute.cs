using System;

namespace IntegrationTests.Orderers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestOrderAttribute : Attribute
    {
        public TestOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }
    }
}
