using Blog.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;

namespace IntegrationTests
{
    /// <summary>
    /// Base class that inherits from <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
    /// </summary>
    internal abstract class AppFactory : WebApplicationFactory<Startup>
    {
        /// <summary>
        /// Sql Instance Name set during the runtime on the constructor of each fixture, 
        /// allowing us to have complete isolation of the database for each test class. 
        /// Essential to prevent dirty data that may cause your Integration Tests to be flaky due to possible unwanted data.
        /// </summary>
        protected const string SqlInstanceName = "IntegrationTest";

        /// <summary>
        /// Constant for the database name.
        /// </summary>
        protected const string Catalog = "IntegrationTest";

        /// <summary>
        /// The Path for the DacPac file.
        /// </summary>
        protected const string DacPacPath = @"Blog.Database\bin\{0}\Blog.Database.dacpac";

        /// <summary>
        /// Gets or sets the path of the Sql Local Db external process.
        /// </summary>
        protected string SqlLocalDb { get; set; }
        
        /// <summary>
        /// Gets or sets the connection string based on the individual test class.
        /// </summary>
        public static string CurrentConnectionString { get; set; }

        /// <summary>
        /// Overriding the configure here allows you to modify settings on the server host as necessary for you application. 
        /// In this sample, we are simply overriding the connection string for the database so EntityFramework and the <see cref="DbContext"/> 
        /// starts against our LocalDb Instance.
        /// </summary>
        /// <param name="builder"></param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<BloggingContext>(opt => 
                    opt.UseSqlServer(CurrentConnectionString, 
                    b => b.MigrationsAssembly("Blog.Api")));
            });
        }

        /// <summary>
        /// WebHost builder to be invoked by the test suite during run time. Essentially it is initializing the Api in a similar way you it would
        /// normally being executed.
        /// </summary>
        /// <returns></returns>
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return Program.CreateWebHostBuilder(Array.Empty<string>());
        }

        /// <summary>
        /// Creates and set the connection string to be used withing the fixture for the test server.
        /// </summary>
        /// <param name="sqlInstanceName"></param>
        /// <param name="catalog"></param>
        protected void SetConnectionString(string sqlInstanceName, string catalog)
        {
            var connStringBuilder = new SqlConnectionStringBuilder
            {
                InitialCatalog = catalog,
                DataSource = $"(localdb)\\{sqlInstanceName}",
                IntegratedSecurity = true,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = false
            };

            CurrentConnectionString = connStringBuilder.ToString();
        }

        /// <summary>
        /// Finds the SqlLocalDb.exe and create a new instance.
        /// </summary>
        protected void CreateSqlInstance()
        {
            SqlLocalDb = FindSqlLocalDb();
            RunExternalProcess(SqlLocalDb, $"create \"{SqlInstanceName}\" -s");
        }

        protected void DeployDatabase()
        {
            var dacPacPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..", FixtureHelper.ComplianceDacPac);
            var fileInfo = new FileInfo(dacPacPath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Couldn't find dacpac file. Please make sure the database project has been built.");
            }

            var args = $"/Action:Publish /SourceFile:\"{fileInfo.FullName}\" /TargetConnectionString:\"{connString}";
            FixtureHelper.RunExternalProcess(FindSqlPackage(), args);
        }

        /// <summary>
        /// Method to find a SqlLocalDb. This method assumes you have Sql Server installed on both, the local machine and build server.
        /// Should modify this if you are not running the typical setup.
        /// </summary>
        /// <returns></returns>
        private string FindSqlLocalDb()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var sqlServer = Path.Combine(programFiles, "Microsoft Sql Server");

            foreach (var version in Directory.GetDirectories(sqlServer).OrderByDescending(x => x))
            {
                var sqlLocalDbPath = Path.Combine(sqlServer, version, @"Tools\Binn\SqlLocalDB.exe");

                if (File.Exists(sqlLocalDbPath))
                {
                    return sqlLocalDbPath;
                }
            }

            throw new FileNotFoundException("Couldn't find a valid SqlLocalDb file");
        }

        /// <summary>
        /// Static method that starts a new external process.
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="args">The arguments of the process.</param>
        private static void RunExternalProcess(string processName, string args)
        {
            var processStartInfo = new ProcessStartInfo(processName, args);
            var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        }
    }

    /// <summary>
    /// This is a collection fixture that inherits from the Custom Factory. 
    /// It ensures you have a working test environment for the collection and since it implements <see cref="IDisposable"/>, it behaves exactly like the xUnit fixtures.
    /// </summary>
    internal class BootstrapFixture: AppFactory
    {
        public BootstrapFixture()
        {
            // We define the connection string to be used withing this fixture.
            SetConnectionString(SqlInstanceName, Catalog);

            // 
            CreateSqlInstance();
            FixtureHelper.DeployDatabase(CurrentConnectionString);
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
    internal class IntegrationTestFixture: AppFactory
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
    internal class TestClassFixture : AppFactory
    {
        public string Test { get; private set; }

        public TestClassFixture()
        {
            Console.WriteLine("Test Fixt class fixture kicked in");
            Test = IntegrationTestFixture.Test;
        }
    }
}
