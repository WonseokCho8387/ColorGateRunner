using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorGateRunner.Presentation
{
    internal readonly struct SceneTransitionResult
    {
        private SceneTransitionResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public bool Succeeded { get; }
        public string Error { get; }

        public static SceneTransitionResult Success() =>
            new SceneTransitionResult(true, string.Empty);

        public static SceneTransitionResult Failure(string error) =>
            new SceneTransitionResult(false, error);
    }

    internal interface ISceneTransitionLoader
    {
        void LoadScene(
            string scenePath,
            Action<SceneTransitionResult> completed);
    }

    internal sealed class UnitySceneTransitionLoader : ISceneTransitionLoader
    {
        public void LoadScene(
            string scenePath,
            Action<SceneTransitionResult> completed)
        {
            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(
                    scenePath,
                    LoadSceneMode.Single);
                if (operation == null)
                {
                    completed?.Invoke(SceneTransitionResult.Failure(
                        "SCENE LOAD COULD NOT START"));
                    return;
                }

                operation.completed += _ =>
                    completed?.Invoke(SceneTransitionResult.Success());
            }
            catch (Exception exception)
            {
                completed?.Invoke(SceneTransitionResult.Failure(
                    "SCENE LOAD FAILED: " + exception.Message));
            }
        }
    }
}
