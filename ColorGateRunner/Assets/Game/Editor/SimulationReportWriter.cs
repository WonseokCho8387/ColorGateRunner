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
                Path.Combine(outputPath, "Step9A-SimulationSummary.md"),
                SimulationReportFormatter.ToMarkdown(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step9A-SimulationResults.json"),
                SimulationReportFormatter.ToJson(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step9A-SimulationResults.csv"),
                SimulationReportFormatter.ToCsv(batch));
            Debug.Log(
                $"Step 9A simulation wrote {batch.Results.Count} matrix rows to {outputPath}.");
        }

        public static void RunStep9ASimulationFromCommandLine()
        {
            RunStep9ASimulation();
        }
    }
}
