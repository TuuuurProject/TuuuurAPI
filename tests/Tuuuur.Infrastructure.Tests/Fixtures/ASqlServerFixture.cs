using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Tests.Data;
using Microsoft.SqlServer.Dac;

namespace Tuuuur.Infrastructure.Tests.Fixtures
{
    public abstract class ASqlServerFixture : ISqlFixture
    {
        #region Constants

        protected const string Tudbname = "TUUUUR_TU";
        protected const string Dacpacpathkey = "DACPACPATH";

        #endregion Constants

        protected ASqlServerFixture(string p_ConnectionString)
        {
            ConnectionString = p_ConnectionString;
            TestContext = DbContextFactory<TestContext>.InitContext(p_ConnectionString);
            TuuuurContext = DbContextFactory<TuuuurContext>.InitContext(p_ConnectionString);
        }

        public string ConnectionString { get; }
        public TestContext TestContext { get; }
        public TuuuurContext TuuuurContext { get; }

        /// <summary>
        /// Get dacpac file path from environment variables
        /// </summary>
        /// <returns></returns>
        protected string GetDacPacPath()
        {
            string v_DacPacFilePath = Environment.GetEnvironmentVariable(Dacpacpathkey);

            if (string.IsNullOrWhiteSpace(v_DacPacFilePath))
            {
                Dispose(true);
                throw new FileNotFoundException("Path of the dacpac is null or empty");
            }

            string v_Path = Path.GetFullPath(v_DacPacFilePath);
            if (!File.Exists(v_Path))
            {
                Dispose(true);
                throw new FileNotFoundException($"File {v_Path} is not exist");
            }

            return v_Path;
        }

        /// <summary>
        /// Deploy dacpac to SQL Server on the docker container
        /// </summary>
        /// <param name="p_Path"></param>
        protected void DeployDacPac(string p_Path)
        {
            using (DacPackage v_DacPac = DacPackage.Load(p_Path))
            {
                DacServices v_DbInstance = new(ConnectionString);
                DacDeployOptions v_DacOptions = new(){ AllowIncompatiblePlatform = true, VerifyDeployment = true };
                v_DbInstance.Deploy(v_DacPac, Tudbname, upgradeExisting: true, options: v_DacOptions);
            }
        }

        /// <summary>
        /// Dispose the fixture
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool p_Disposing);
    }
}