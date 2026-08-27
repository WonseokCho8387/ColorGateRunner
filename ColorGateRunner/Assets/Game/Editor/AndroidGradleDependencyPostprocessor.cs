using System;
using System.IO;
#if UNITY_ANDROID
using UnityEditor.Android;
using UnityEngine;
#endif

namespace ColorGateRunner.Editor
{
    public sealed class AndroidGradleDependencyPostprocessor
#if UNITY_ANDROID
        : IPostGenerateGradleAndroidProject
#endif
    {
        internal const string KotlinVersion = "1.8.22";
        internal const string Marker =
            "// Color Gate Runner Kotlin dependency alignment";

#if UNITY_ANDROID
        public int callbackOrder => 1000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string rootGradlePath = ResolveRootGradlePath(path);
            string original = File.ReadAllText(rootGradlePath);
            string updated = InjectKotlinResolution(original);
            if (updated == original)
            {
                return;
            }

            File.WriteAllText(rootGradlePath, updated);
            Debug.Log(
                "Android Gradle Kotlin dependencies aligned to " +
                KotlinVersion + ".");
        }
#endif

        internal static string InjectKotlinResolution(string gradle)
        {
            if (gradle == null)
            {
                throw new ArgumentNullException(nameof(gradle));
            }
            int markerIndex = gradle.IndexOf(Marker, StringComparison.Ordinal);
            int pluginsIndex = gradle.IndexOf(
                "plugins {",
                StringComparison.Ordinal);
            if (markerIndex >= 0 && markerIndex > pluginsIndex)
            {
                return gradle;
            }
            if (markerIndex >= 0 && pluginsIndex >= 0 &&
                markerIndex < pluginsIndex)
            {
                gradle = gradle.Substring(pluginsIndex);
            }

            string lineEnding = gradle.Contains("\r\n", StringComparison.Ordinal)
                ? "\r\n"
                : "\n";
            string alignment = string.Join(
                lineEnding,
                Marker,
                "allprojects {",
                "    configurations.configureEach {",
                "        exclude group: 'org.jetbrains.kotlin', module: 'kotlin-stdlib-jdk7'",
                "        exclude group: 'org.jetbrains.kotlin', module: 'kotlin-stdlib-jdk8'",
                "        resolutionStrategy {",
                $"            force 'org.jetbrains.kotlin:kotlin-stdlib:{KotlinVersion}'",
                "        }",
                "    }",
                "}",
                string.Empty,
                string.Empty);

            const string cleanTask = "tasks.register('clean'";
            int insertionIndex = gradle.IndexOf(
                cleanTask,
                StringComparison.Ordinal);
            if (insertionIndex < 0)
            {
                throw new InvalidDataException(
                    "Generated Android root build.gradle has no clean task.");
            }

            return gradle.Insert(insertionIndex, alignment);
        }

#if UNITY_ANDROID
        private static string ResolveRootGradlePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Android Gradle project path is required.",
                    nameof(path));
            }

            string direct = Path.Combine(path, "build.gradle");
            if (IsRootGradleFile(direct))
            {
                return direct;
            }

            DirectoryInfo parent = Directory.GetParent(path);
            string parentGradle = parent == null
                ? string.Empty
                : Path.Combine(parent.FullName, "build.gradle");
            if (IsRootGradleFile(parentGradle))
            {
                return parentGradle;
            }

            throw new FileNotFoundException(
                "Generated Android root build.gradle was not found.",
                direct);
        }

        private static bool IsRootGradleFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            string contents = File.ReadAllText(path);
            return contents.Contains("plugins {", StringComparison.Ordinal) &&
                contents.Contains("tasks.register('clean'", StringComparison.Ordinal);
        }
#endif
    }
}
