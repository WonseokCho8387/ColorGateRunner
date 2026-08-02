using System;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FrontendContextTests
    {
        [Test]
        public void GuestProfileAndSettings_BindToImmutableDisplayContext()
        {
            AppServiceGraph graph = CreateInitializedGraph();

            bool created = FrontendDisplayContext.TryCreate(
                graph,
                "1.2.3",
                out FrontendDisplayContext context,
                out string error);

            Assert.That(created, Is.True, error);
            Assert.That(context.DisplayName, Is.EqualTo("GUEST"));
            Assert.That(context.AccountLabel, Is.EqualTo("GUEST"));
            Assert.That(context.VersionLabel, Is.EqualTo("v1.2.3"));
            Assert.That(context.SettingsAvailable, Is.True);
            Assert.That(context.SettingsSummary, Does.Contain("MASTER 100%"));

            graph.Profile.Current.DisplayName = "MUTATED";
            graph.Settings.Current.MasterVolume = 0f;
            Assert.That(context.DisplayName, Is.EqualTo("GUEST"));
            Assert.That(context.SettingsSummary, Does.Contain("MASTER 100%"));
        }

        [Test]
        public void MissingProfile_ProducesBootRequiredError()
        {
            AppServiceGraph graph = CreateGraph();

            bool created = FrontendDisplayContext.TryCreate(
                graph,
                "1.0.0",
                out FrontendDisplayContext context,
                out string error);

            Assert.That(created, Is.False);
            Assert.That(context, Is.Null);
            Assert.That(error, Is.EqualTo("BOOT REQUIRED"));
        }

        [Test]
        public void MissingOptionalSettings_HidesSettingsFeature()
        {
            AppServiceGraph graph = CreateGraph();
            var save = LocalSaveData.CreateEmpty();
            graph.Profile.LoadOrCreateGuest(save);
            graph.Account.Load(graph.Profile.Current);

            bool created = FrontendDisplayContext.TryCreate(
                graph,
                "1.0.0",
                out FrontendDisplayContext context,
                out string error);

            Assert.That(created, Is.True, error);
            Assert.That(context.SettingsAvailable, Is.False);
            Assert.That(
                context.SettingsSummary,
                Is.EqualTo("SETTINGS UNAVAILABLE"));
        }

        private static AppServiceGraph CreateInitializedGraph()
        {
            AppServiceGraph graph = CreateGraph();
            var save = LocalSaveData.CreateEmpty();
            graph.Profile.LoadOrCreateGuest(save);
            graph.Settings.LoadOrCreateDefaults(save);
            graph.Account.Load(graph.Profile.Current);
            return graph;
        }

        private static AppServiceGraph CreateGraph()
        {
            var clock = new FixedClock();
            var profile = new ProfileService(clock, new FixedIdGenerator());
            var account = new LocalAccountService();
            var settings = new SettingsService();
            var save = new MemorySaveService();
            return new AppServiceGraph(
                clock,
                profile,
                account,
                settings,
                save,
                new AppInitializationPipeline(
                    save,
                    profile,
                    settings,
                    account));
        }

        private sealed class FixedClock : IClockService
        {
            public DateTime UtcNow =>
                new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc);
        }

        private sealed class FixedIdGenerator : IProfileIdGenerator
        {
            public string CreateProfileId() => "frontend-guest";
        }

        private sealed class MemorySaveService : ILocalSaveService
        {
            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Success(
                    LocalSaveData.CreateEmpty(),
                    true,
                    false);

            public LocalSaveWriteResult Save(LocalSaveData data) =>
                LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
        }
    }
}
