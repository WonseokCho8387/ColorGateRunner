using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.PlayMode
{
    // Temporarily renders every root overlay Canvas through a camera with a
    // fixed portrait render texture, so layout, audit and evidence do not
    // depend on the Editor window size or batchmode screen.
    internal sealed class UiPortraitFrame : IDisposable
    {
        private const int UiLayer = 5;
        private readonly List<Canvas> _roots = new List<Canvas>();
        private readonly List<RenderMode> _modes = new List<RenderMode>();
        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<float> _distances = new List<float>();
        private readonly List<Transform> _layerTransforms = new List<Transform>();
        private readonly List<int> _layers = new List<int>();
        private readonly GameObject _cameraObject;

        internal UiPortraitFrame(int width, int height)
        {
            Width = width;
            Height = height;
            Target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            _cameraObject = new GameObject("UiPortraitFrameCamera");
            Camera = _cameraObject.AddComponent<Camera>();
            Camera.enabled = false;
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Color.black;
            Camera.cullingMask = 1 << UiLayer;
            Camera.nearClipPlane = 0.01f;
            Camera.farClipPlane = 100f;
            Camera.targetTexture = Target;
            _cameraObject.transform.position = new Vector3(0f, 0f, -50f);

            Canvas[] all = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int index = 0; index < all.Length; index++)
            {
                if (all[index].isRootCanvas && all[index].isActiveAndEnabled &&
                    all[index].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    _roots.Add(all[index]);
                }
            }
            _roots.Sort((left, right) => left.sortingOrder.CompareTo(right.sortingOrder));
            for (int index = 0; index < _roots.Count; index++)
            {
                Canvas canvas = _roots[index];
                _modes.Add(canvas.renderMode);
                _cameras.Add(canvas.worldCamera);
                _distances.Add(canvas.planeDistance);
                Transform[] transforms = canvas.GetComponentsInChildren<Transform>(true);
                for (int child = 0; child < transforms.Length; child++)
                {
                    _layerTransforms.Add(transforms[child]);
                    _layers.Add(transforms[child].gameObject.layer);
                    transforms[child].gameObject.layer = UiLayer;
                }
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera;
                canvas.planeDistance = 10f - (index * 0.5f);
            }
        }

        internal int Width { get; }
        internal int Height { get; }
        internal Camera Camera { get; }
        internal RenderTexture Target { get; }

        // CanvasScaler re-evaluates in Update, so callers wait one frame.
        internal IEnumerator Settle()
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        internal void WritePng(string path)
        {
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            try
            {
                Camera.Render();
                RenderTexture.active = Target;
                image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(image);
            }
        }

        public void Dispose()
        {
            for (int index = 0; index < _roots.Count; index++)
            {
                if (_roots[index] == null)
                {
                    continue;
                }
                _roots[index].renderMode = _modes[index];
                _roots[index].worldCamera = _cameras[index];
                _roots[index].planeDistance = _distances[index];
            }
            for (int index = 0; index < _layerTransforms.Count; index++)
            {
                if (_layerTransforms[index] != null)
                {
                    _layerTransforms[index].gameObject.layer = _layers[index];
                }
            }
            Camera.targetTexture = null;
            Object.Destroy(_cameraObject);
            Target.Release();
            Object.Destroy(Target);
            Canvas.ForceUpdateCanvases();
        }
    }

    internal static class UiCanvasCapture
    {
        internal static IEnumerator Capture(string path, int width, int height)
        {
            using (var frame = new UiPortraitFrame(width, height))
            {
                yield return frame.Settle();
                frame.WritePng(path);
            }
        }
    }
}
