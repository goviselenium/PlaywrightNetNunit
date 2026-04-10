using NUnit.Framework;
using PlaywrightFramework.Config;

namespace PlaywrightFramework.Tests
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            LoggerConfiguration.Configure();
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            LoggerConfiguration.Shutdown();
        }
    }
}
