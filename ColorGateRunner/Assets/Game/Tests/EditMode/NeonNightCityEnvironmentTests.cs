using System.Collections.Generic;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class NeonNightCityEnvironmentTests
    {
        [Test]
        public void NightShaders_AreAvailableWithoutCompileErrors()
        {
            string[] names =
            {
                GrayboxSceneBuilder.NightSkyShaderName,
                GrayboxSceneBuilder.CityFacadeShaderName
            };
            for (int index = 0; index < names.Length; index++)
            {
                Shader shader = Shader.Find(names[index]);
                Assert.That(shader, Is.Not.Null, names[index]);
                Assert.That(ShaderUtil.ShaderHasError(shader), Is.False,
                    names[index]);
            }
        }

        [Test]
        public void CampaignScene_RendersNightSkyAmbientAndDistanceFog()
        {
            WithActiveCampaignScene(scene =>
            {
                Camera camera = FindSingle<Camera>(scene);
                Assert.That(camera.clearFlags,
                    Is.EqualTo(CameraClearFlags.Skybox));
                Assert.That(RenderSettings.skybox, Is.Not.Null);
                Assert.That(RenderSettings.skybox.shader.name,
                    Is.EqualTo(GrayboxSceneBuilder.NightSkyShaderName));
                Assert.That(RenderSettings.ambientMode,
                    Is.EqualTo(AmbientMode.Trilight));
                Assert.That(RenderSettings.ambientSkyColor.maxColorComponent,
                    Is.LessThan(0.4f));
                Assert.That(RenderSettings.ambientGroundColor.maxColorComponent,
                    Is.LessThan(0.05f));
                Assert.That(RenderSettings.fog, Is.True);
                Assert.That(RenderSettings.fogMode, Is.EqualTo(FogMode.Linear));
                Assert.That(RenderSettings.fogStartDistance,
                    Is.GreaterThanOrEqualTo(GrayboxSceneBuilder.NightFogStart));
                Assert.That(RenderSettings.fogEndDistance,
                    Is.GreaterThan(RenderSettings.fogStartDistance));
            });
        }

        [Test]
        public void CampaignCity_UsesFacadeBodiesAndCyanMagentaGlows()
        {
            WithActiveCampaignScene(scene =>
            {
                SplineCityPoolView city = FindSingle<SplineCityPoolView>(scene);
                Renderer[] renderers =
                    city.GetComponentsInChildren<Renderer>(true);
                var glowMaterials = new HashSet<string>();
                int bodyCount = 0;
                for (int index = 0; index < renderers.Length; index++)
                {
                    Material material = renderers[index].sharedMaterial;
                    Assert.That(material, Is.Not.Null, renderers[index].name);
                    if (renderers[index].name.Contains("Glow"))
                    {
                        glowMaterials.Add(material.name);
                        continue;
                    }
                    bodyCount++;
                    Assert.That(material.shader.name,
                        Is.EqualTo(GrayboxSceneBuilder.CityFacadeShaderName),
                        renderers[index].name);
                }

                Assert.That(bodyCount, Is.EqualTo(city.SlotCount));
                Assert.That(glowMaterials,
                    Is.EquivalentTo(new[] { "CityMagenta", "Cyan" }));
            });
        }

        private static void WithActiveCampaignScene(System.Action<Scene> body)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(
                GrayboxSceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                body(scene);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded)
                {
                    SceneManager.SetActiveScene(previous);
                }
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindSingle<T>(Scene scene)
            where T : Component
        {
            var found = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                found.AddRange(roots[index].GetComponentsInChildren<T>(true));
            }
            Assert.That(found.Count, Is.EqualTo(1), typeof(T).Name);
            return found[0];
        }
    }
}
