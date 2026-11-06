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

namespace ColorGateRunner.Tests.EditMode
{
    // Finds a Stage by its authored number (stage-NN), which stays fixed
    // when the Campaign order moves Stages to other display numbers.
    internal static class AuthoredStage
    {
        internal static string Id(int authoredNumber) =>
            $"stage-{authoredNumber:00}";

        internal static Core.StageDefinition Get(int authoredNumber) =>
            Core.StageCatalog.GetById(Id(authoredNumber));

        internal static Core.StageDefinition From(
            Core.IStageCatalog catalog,
            int authoredNumber) =>
            catalog.GetById(Id(authoredNumber));

        internal static int Number(int authoredNumber) =>
            Get(authoredNumber).DisplayNumber;
    }
}
