using System.IO;
using ColorGateRunner.Core;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    public static class ExperimentReportWriter
    {
        [MenuItem("Tools/Color Gate Runner/Run Step 10 Experiment Matrix")]
        public static void RunStep10ExperimentMatrix()
        {
            ExperimentSimulationBatch batch =
                ExperimentSimulationRunner.RunFullMatrix(1000);
            ExperimentSimulationRunner.RunCandidateItemMatrix(batch, 1000);
            string projectPath =
                Directory.GetParent(Application.dataPath).FullName;
            string repositoryPath = Directory.GetParent(projectPath).FullName;
            string outputPath = Path.Combine(
                repositoryPath,
                "Artifacts",
                "Experiment");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(
                Path.Combine(outputPath, "Step10-ColorCapacitySummary.md"),
                ExperimentReportFormatter.ToColorCapacityMarkdown(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step10-ColorCapacityResults.json"),
                ExperimentReportFormatter.ToJson(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step10-ColorCapacityResults.csv"),
                ExperimentReportFormatter.ToCsv(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step10-MechanicComparison.md"),
                ExperimentReportFormatter.ToMechanicMarkdown(batch));
            File.WriteAllText(
                Path.Combine(outputPath, "Step10-Shortlist.md"),
                ExperimentReportFormatter.ToShortlistMarkdown(batch));
            Debug.Log(
                $"Step 10 experiment matrix wrote {batch.Results.Count} rows and {batch.TotalRuns} runs.");
        }

        public static void RunStep10ExperimentMatrixFromCommandLine()
        {
            RunStep10ExperimentMatrix();
        }
    }
}
