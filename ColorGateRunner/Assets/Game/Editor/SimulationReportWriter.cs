using System.IO;
using ColorGateRunner.Core;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    public static class SimulationReportWriter
    {
        [MenuItem("Tools/Color Gate Runner/Run Step 9A Simulation")]
        public static void RunStep9ASimulation()
        {
            StageCatalogAssetBuilder.EnsureAndConfigure();
            SimulationBatchResult batch =
                StageSimulationRunner.RunFullMatrix(1000);
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            string repositoryPath = Directory.GetParent(projectPath).FullName;
            string outputPath = Path.Combine(
                repositoryPath,
                "Artifacts",
                "Simulation");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(
                Path.Combine(
                    outputPath,
                    "MechanicCampaign-SimulationSummary.md"),
                SimulationReportFormatter.ToMarkdown(batch));
            File.WriteAllText(
                Path.Combine(
                    outputPath,
                    "MechanicCampaign-SimulationResults.json"),
                SimulationReportFormatter.ToJson(batch));
            File.WriteAllText(
                Path.Combine(
                    outputPath,
                    "MechanicCampaign-SimulationResults.csv"),
                SimulationReportFormatter.ToCsv(batch));
            Debug.Log(
                $"Mechanic campaign simulation wrote {batch.Results.Count} " +
                $"matrix rows to {outputPath}.");
        }

        public static void RunStep9ASimulationFromCommandLine()
        {
            RunStep9ASimulation();
        }
    }
}
