using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    [SetUpFixture]
    public sealed class StageCatalogTestBootstrap
    {
        [OneTimeSetUp]
        public void ConfigureStageCatalog()
        {
            StageCatalogProvider.EnsureConfigured();
        }
    }
}
