using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace IntegrationTests.Orderers
{
    internal class TestOrderer : ITestCaseOrderer
    {
        public const string TypeName = nameof(TestOrderer);
        public const string AssemblyName = "Lucsan.Workbench.IntegrationTests";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (var testCase in testCases)
            {
                var priority = 0;
                var assemblyQualifiedAttributeTypeName = typeof(TestOrderAttribute).AssemblyQualifiedName;
                if (assemblyQualifiedAttributeTypeName != null)
                {
                    foreach (var attr in testCase.TestMethod.Method.GetCustomAttributes(assemblyQualifiedAttributeTypeName))
                    {
                        priority = attr.GetNamedArgument<int>("Order");
                    }
                }

                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
            {
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                foreach (TTestCase testCase in list)
                {
                    yield return testCase;
                }
            }
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            result = new TValue();
            dictionary[key] = result;

            return result;
        }
    }
}
