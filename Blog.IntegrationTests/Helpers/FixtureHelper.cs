//using System;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;

//namespace IntegrationTests.Helpers
//{

//    /// <summary>
//    /// Helper class that contaiins static methods to be used within a test fixture.
//    /// </summary>
//    public static class FixtureHelper
//    {
//        private static readonly string[] VsTypes = { "Enterprise", "Professional", "Community" };

//        /// <summary>
//        /// Gets the current execution envirnment based on Pragma condition.
//        /// </summary>
//        public static string ExecutionEnvironment
//        {
//            get
//            {
//#if DEBUG
//                return "Debug";
//#else
//                return "Release";
//#endif
//            }
//        }

//        /// <summary>
//        /// Gets the DacPac path based on the current execution environment
//        /// </summary>
//        public static string ComplianceDacPac => string.Format(TestConstants.DacPacPath, ExecutionEnvironment);

//        /// <summary>
//        /// Static method that starts a new external process.
//        /// </summary>
//        /// <param name="processName">The name of the process</param>
//        /// <param name="args">The arguments of the process.</param>
//        public static void RunExternalProcess(string processName, string args)
//        {
//            var processStartInfo = new ProcessStartInfo(processName, args);
//            var process = Process.Start(processStartInfo);
//            process?.WaitForExit();
//        }

//        /// <summary>
//        /// Find the SqlLocalDb executable based on default installation folders.
//        /// </summary>
//        /// <returns>The fully qualified path of the SqlLocalDb executable</returns>
//        public static string FindLocalDbServer()
//        {
//            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
//            var sqlServer = Path.Combine(programFiles, "Microsoft Sql Server");

//            foreach (var version in Directory.GetDirectories(sqlServer).OrderByDescending(x => x))
//            {
//                var sqlLocalDbPath = Path.Combine(sqlServer, version, @"Tools\Binn\SqlLocalDB.exe");

//                if (File.Exists(sqlLocalDbPath))
//                {
//                    return sqlLocalDbPath;
//                }
//            }

//            throw new FileNotFoundException("Couldn't find a valid SQLLocalDb file");
//        }

//        /// <summary>
//        /// Finds the SqlPackage executablel based on default installation folders
//        /// </summary>
//        /// <returns>The path on which the SqlPackage can be found.</returns>
//        public static string FindSqlPackage()
//        {
//            var dacfx = GetDacFx();
//            foreach (var version in Directory.GetDirectories(dacfx).OrderByDescending(x => x))
//            {
//                var sqlPackagePath = Path.Combine(dacfx, version, @"SqlPackage.exe");

//                if (File.Exists(sqlPackagePath))
//                {
//                    return sqlPackagePath;
//                }
//            }

//            throw new FileNotFoundException("Couldn't find a valid SqlPackage file");
//        }

//        /// <summary>
//        /// Deploys the dac pac based on application standard path.
//        /// </summary>
//        /// <param name="connString">The connection string to be used to deploy the database.</param>
//        public static void DeployDatabase(string connString)
//        {
//            var dacPacPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..", FixtureHelper.ComplianceDacPac);
//            var fileInfo = new FileInfo(dacPacPath);
//            if (!fileInfo.Exists)
//            {
//                throw new FileNotFoundException("Couldn't find dacpac file. Please make sure the database project has been built.");
//            }

//            var args = $"/Action:Publish /SourceFile:\"{fileInfo.FullName}\" /TargetConnectionString:\"{connString}";
//            FixtureHelper.RunExternalProcess(FindSqlPackage(), args);
//        }

//        /// <summary>
//        /// Cleans the folder created by the SqlLocalDb instance
//        /// </summary>
//        /// <param name="instanceName">The name of the SqlLocalDb which creates the default instance folder.</param>
//        public static void CleanLocalDbInstanceDirectory(string instanceName)
//        {
//            var instanceDirectory = Path.Combine(GetLocalDbDirectory(), instanceName);
//            if (Directory.Exists(instanceDirectory))
//            {
//                Directory.Delete(instanceDirectory, true);
//            }
//        }

//        /// <summary>
//        /// Finds the AppData folder on which the SqlLocalDb creates its data folder by default.
//        /// </summary>
//        /// <returns>The full path of the user based local application data.</returns>
//        public static string GetLocalDbDirectory()
//        {
//            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
//            var instanceDirectory = Path.Combine(localAppDataPath, "Microsoft", "Microsoft SQL Server Local DB", "Instances");
//            return instanceDirectory;
//        }

//        /// <summary>
//        /// Get the application directory path.
//        /// </summary>
//        /// <returns>The application directory path.</returns>
//        public static string GetAppDirectory()
//        {
//            var appDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);
//            return appDirectory;
//        }

//        /// <summary>
//        /// Performs a copy of the generated .mdf file so it can be used by other fixtures.
//        /// </summary>
//        /// <param name="instanceName">The sql instance name where the source mdf can be found.</param>
//        public static void CopyMdf(string instanceName)
//        {
//            var instanceDirectory = Path.Combine(GetLocalDbDirectory(), instanceName);
//            if (Directory.Exists(instanceDirectory))
//            {
//                Action<string> CopyFile = (fileName) =>
//                {
//                    var sourceFile = Path.Combine(instanceDirectory, fileName);
//                    if (File.Exists(sourceFile))
//                    {
//                        var destFileName = Path.Combine(GetAppDirectory(), fileName);
//                        File.Copy(sourceFile, destFileName, true);
//                    }
//                };

//                CopyFile($"{TestConstants.DbFileName}.mdf");
//                CopyFile($"{TestConstants.DbFileName}.ldf");
//            }
//        }

//        /// <summary>
//        /// Detaches the .mdf from the source SqlInstance (Performed by BootstrapFixture).
//        /// </summary>
//        /// <param name="connString">The connection string of the db instance</param>
//        /// <param name="sqlDbName">The name of the database used.</param>
//        public static void DetachMdf(string connString, string sqlDbName)
//        {
//            var retryCount = 0;
//            while (retryCount <= 3)
//            {
//                using (var connection = new SqlConnection(connString))
//                {
//                    using (var command = connection.CreateCommand())
//                    {
//                        command.CommandText = $"ALTER DATABASE [{sqlDbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE " +
//                                              $"exec sp_detach_db {sqlDbName}, 'true'";
//                        connection.Open();
//                        try
//                        {
//                            command.ExecuteNonQuery();
//                            retryCount = 100;
//                            Console.WriteLine("Successfully dettached db {0}", sqlDbName);
//                        }
//                        catch (Exception e)
//                        {
//                            retryCount++;
//                            if (retryCount > 3)
//                            {
//                                Console.WriteLine(e);
//                                throw;
//                            }

//                            Console.WriteLine("Failed to dettach mdf. Will retry. Count: {0}", retryCount);
//                            Thread.Sleep(500);
//                        }
//                    }
//                }
//            }
//        }

//        private static string GetDacFx()
//        {
//            var programFiles = Environment.GetEnvironmentVariable("programfiles(x86)");
//            foreach (var vsType in VsTypes)
//            {
//                var vsPath = $@"Microsoft Visual Studio\2017\{vsType}\Common7\IDE\Extensions\Microsoft\SQLDB\DAC";
//                var dacfx = Path.Combine(programFiles, vsPath);
//                if (Directory.Exists(dacfx))
//                {
//                    return dacfx;
//                }
//            }

//            throw new FileNotFoundException("Couldn't find a valid SqlPackage file");
//        }
//    }
//}
