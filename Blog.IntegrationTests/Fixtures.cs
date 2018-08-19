using Blog.Api;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;

namespace IntegrationTests
{
    public class AppFactory : WebApplicationFactory<Startup>
    {
        protected string SqlLocalDb { get; set; }

        protected string SqlInstanceName { get; set; }

        public static string CurrentConnectionString { get; set; }


        //protected void SetConnectionString(string sqlInstanceName)
        //{
        //    CurrentConnectionString = $"Data Source=(localdb)\\{sqlInstanceName};Initial Catalog={TestConstants.SqlDbName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False";
        //}

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            // This creates the Builder that can be similar to your CreateWebHostBuilder from your API. Program.cs
            // It also ensures you are testing precisely how your application works. 
            return WebHost.CreateDefaultBuilder(Array.Empty<string>())
                          .UseStartup<Startup>();
        }
    }

    /// <summary>
    /// This is a collection fixture that inherits from the Custom Factory. 
    /// It ensures you have a working test environment for the collection and since it implements <see cref="IDisposable"/>, it behaves exactly like the xUnit fixtures.
    /// </summary>
    public class BootstrapFixture: AppFactory
    {
        public BootstrapFixture()
        {
            //SqlInstanceName = TestConstants.SqlInstanceName;
            //SetConnectionString(TestConstants.SqlInstanceName);
            //CreateSqlInstance();
            //FixtureHelper.DeployDatabase(CurrentConnectionString);
        }


        /// <summary>
        /// If you need to perform more actions on the dispose, make sure you dispose the base as it ensures the correct disposal of the base factory resources.
        /// </summary>
        public new void Dispose()
        {
            Console.WriteLine("Dispose called from BootstrapFixture");
            base.Dispose();
        }
    }

    /// <summary>
    /// Collection fixture that will be triggered at the beginning of the first test class within the test collection.
    /// It's dispose will be called at the end of the last test class of the collection.
    /// </summary>
    public class IntegrationTestFixture: AppFactory
    {
        internal static string Test { get; private set; }

        public IntegrationTestFixture()
        {
            Console.WriteLine("Integration Test Fixture kicked in");
            Test = "Yes, it works";
        }
    }

    /// <summary>
    /// This fixture can be used to perform operations at the beginning and end of each test class.
    /// </summary>
    public class TestClassFixture : AppFactory
    {
        public string Test { get; private set; }

        public TestClassFixture()
        {
            Console.WriteLine("Test Fixt class fixture kicked in");
            Test = IntegrationTestFixture.Test;
        }
    }
}
