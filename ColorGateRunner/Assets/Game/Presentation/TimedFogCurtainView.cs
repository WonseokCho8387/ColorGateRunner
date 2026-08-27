using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorGateRunner.Presentation
{
    public sealed class TimedFogCurtainView : MonoBehaviour
    {
        internal const int RequiredSectionCount = 10;
        internal const float ForwardTravelSeconds = 0.9f;
        internal const float HeightOffset = 3.5f;
        internal const int FogWispParticleCapacity = 24;
        internal const int RainParticleCapacity = 96;
        private const float MinimumBandLength = 18f;
        private const float MaximumBandLength = 32f;
        private const float BandTravelSeconds = 0.68f;
        private const float FogWispEmissionRate = 8f;
        private const float RainEmissionRate = 70f;
        private static readonly float[] LateralOffsets =
        {
            -2.6f, 1.8f, -1.2f, 2.4f, 0f,
            -2.1f, 1.4f, -0.4f, 2.7f, -1.7f
        };
        private static readonly float[] VerticalOffsets =
        {
            1f, 2.2f, 0.2f, 1.5f, -0.3f,
            2.5f, 0.6f, 1.8f, 0f, 1.1f
        };

        [SerializeField] private Transform[] bankSections;
        [SerializeField] private Renderer[] bankRenderers;
        [SerializeField] private ParticleSystem fogWispParticles;
        [SerializeField] private ParticleSystem rainParticles;
        [SerializeField] private Volume weatherVolume;

        private readonly TimedFogCurtainState _state =
            new TimedFogCurtainState();
        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor = Color.white;

        internal bool HasTriggered => _state.HasTriggered;
        internal bool IsVisible => gameObject.activeSelf;
        internal float Alpha => _state.Alpha;
        internal float ElapsedSeconds => _state.ElapsedSeconds;
        internal int SectionCount =>
            bankSections == null ? 0 : bankSections.Length;
        internal int RendererCount =>
            bankRenderers == null ? 0 : bankRenderers.Length;
        internal ParticleSystem FogWispParticles => fogWispParticles;
        internal ParticleSystem RainParticles => rainParticles;
        internal float WeatherToneWeight =>
            weatherVolume == null ? 0f : weatherVolume.weight;

        private void Awake()
        {
            EnsurePresentationState();
        }

        internal void Configure(
            Transform[] sections,
            Renderer[] renderers,
            ParticleSystem fogWisps,
            ParticleSystem rain,
            Volume toneVolume)
        {
            bankSections = sections;
            bankRenderers = renderers;
            fogWispParticles = fogWisps;
            rainParticles = rain;
            weatherVolume = toneVolume;
            EnsurePresentationState();
            ResetCurtain();
        }

        internal bool HasRequiredReferences()
        {
            if (bankSections == null ||
                bankSections.Length != RequiredSectionCount ||
                bankRenderers == null || bankRenderers.Length == 0 ||
                fogWispParticles == null || rainParticles == null ||
                weatherVolume == null || weatherVolume.sharedProfile == null)
            {
                return false;
            }
            for (int index = 0; index < bankSections.Length; index++)
            {
                if (bankSections[index] == null)
                {
                    return false;
                }
            }
            for (int index = 0; index < bankRenderers.Length; index++)
            {
                if (bankRenderers[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool TryActivate(
            Vector3 playerPosition,
            float speed,
            FogCurtainSettings settings)
        {
            bool activated = _state.TryTrigger(settings);
            Synchronize(playerPosition, speed);
            return activated;
        }

        internal void Tick(
            float deltaSeconds,
            Vector3 playerPosition,
            float speed)
        {
            _state.Advance(Mathf.Max(0f, deltaSeconds));
            Synchronize(playerPosition, speed);
        }

        internal bool TryActivateOnPath(
            CampaignSplinePathView path,
            float playerDistance,
            float speed,
            FogCurtainSettings settings)
        {
            bool activated = _state.TryTrigger(settings);
            SynchronizeOnPath(path, playerDistance, speed);
            return activated;
        }

        internal void TickOnPath(
            float deltaSeconds,
            CampaignSplinePathView path,
            float playerDistance,
            float speed)
        {
            _state.Advance(Mathf.Max(0f, deltaSeconds));
            SynchronizeOnPath(path, playerDistance, speed);
        }

        internal void SynchronizeOnPath(
            CampaignSplinePathView path,
            float playerDistance,
            float speed)
        {
            if (!_state.IsActive)
            {
                HidePresentation();
                return;
            }

            float safeSpeed = Mathf.Max(0f, speed);
            float centerDistance = playerDistance +
                (safeSpeed * ForwardTravelSeconds);
            float bandLength = ResolveBandLength(safeSpeed);
            path.EvaluatePose(
                centerDistance,
                HeightOffset,
                out Vector3 centerPosition,
                out Quaternion centerRotation);

            gameObject.SetActive(true);
            transform.SetPositionAndRotation(
                centerPosition,
                centerRotation);
            for (int index = 0; index < bankSections.Length; index++)
            {
                float normalized = bankSections.Length <= 1
                    ? 0.5f
                    : index / (float)(bankSections.Length - 1);
                float distance = centerDistance +
                    ((normalized - 0.5f) * bandLength);
                path.EvaluatePose(
                    distance,
                    HeightOffset,
                    out Vector3 position,
                    out Quaternion rotation);
                position += rotation * new Vector3(
                    LateralOffsets[index],
                    VerticalOffsets[index],
                    0f);
                bankSections[index].SetPositionAndRotation(
                    position,
                    rotation);
            }
            ApplyAlpha(_state.Alpha);
            SynchronizeWeather(_state.Alpha);
        }

        internal void Synchronize(Vector3 playerPosition, float speed)
        {
            if (!_state.IsActive)
            {
                HidePresentation();
                return;
            }

            float safeSpeed = Mathf.Max(0f, speed);
            float centerZ = playerPosition.z +
                (safeSpeed * ForwardTravelSeconds);
            float bandLength = ResolveBandLength(safeSpeed);
            Vector3 center = new Vector3(
                playerPosition.x,
                playerPosition.y + HeightOffset,
                centerZ);
            gameObject.SetActive(true);
            transform.SetPositionAndRotation(center, Quaternion.identity);
            for (int index = 0; index < bankSections.Length; index++)
            {
                float normalized = bankSections.Length <= 1
                    ? 0.5f
                    : index / (float)(bankSections.Length - 1);
                bankSections[index].SetPositionAndRotation(
                    new Vector3(
                        center.x + LateralOffsets[index],
                        center.y + VerticalOffsets[index],
                        centerZ + ((normalized - 0.5f) * bandLength)),
                    Quaternion.identity);
            }
            ApplyAlpha(_state.Alpha);
            SynchronizeWeather(_state.Alpha);
        }

        internal Transform GetSection(int index)
        {
            return bankSections[index];
        }

        internal Renderer GetRenderer(int index)
        {
            return bankRenderers[index];
        }

        internal void ResetCurtain()
        {
            _state.Reset();
            HidePresentation();
        }

        internal void PauseWeather()
        {
            PauseParticles(fogWispParticles);
            PauseParticles(rainParticles);
        }

        internal void ResumeWeather()
        {
            if (!_state.IsActive)
            {
                return;
            }
            ResumeParticles(fogWispParticles);
            ResumeParticles(rainParticles);
        }

        private static float ResolveBandLength(float speed)
        {
            return Mathf.Clamp(
                speed * BandTravelSeconds,
                MinimumBandLength,
                MaximumBandLength);
        }

        private void HidePresentation()
        {
            ApplyAlpha(0f);
            StopAndClear(fogWispParticles);
            StopAndClear(rainParticles);
            if (weatherVolume != null)
            {
                weatherVolume.weight = 0f;
            }
            gameObject.SetActive(false);
        }

        private void SynchronizeWeather(float alpha)
        {
            float strength = Mathf.Clamp01(alpha);
            if (weatherVolume != null)
            {
                weatherVolume.weight = strength;
            }
            SynchronizeParticles(
                fogWispParticles,
                FogWispEmissionRate * strength);
            SynchronizeParticles(
                rainParticles,
                RainEmissionRate * strength);
        }

        private static void SynchronizeParticles(
            ParticleSystem particles,
            float emissionRate)
        {
            if (particles == null)
            {
                return;
            }
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTimeMultiplier = Mathf.Max(0f, emissionRate);
            if (!particles.isPlaying && !particles.isPaused)
            {
                particles.Play(true);
            }
        }

        private static void PauseParticles(ParticleSystem particles)
        {
            if (particles != null && particles.isPlaying)
            {
                particles.Pause(true);
            }
        }

        private static void ResumeParticles(ParticleSystem particles)
        {
            if (particles != null && particles.isPaused)
            {
                particles.Play(true);
            }
        }

        private static void StopAndClear(ParticleSystem particles)
        {
            if (particles != null)
            {
                particles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void EnsurePresentationState()
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            if (bankRenderers == null)
            {
                return;
            }
            for (int index = 0; index < bankRenderers.Length; index++)
            {
                Renderer renderer = bankRenderers[index];
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    _baseColor = renderer.sharedMaterial.color;
                    return;
                }
            }
        }

        private void ApplyAlpha(float alpha)
        {
            if (bankRenderers == null)
            {
                return;
            }
            EnsurePresentationState();
            Color color = _baseColor;
            color.a = Mathf.Clamp01(alpha);
            _propertyBlock.Clear();
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            for (int index = 0; index < bankRenderers.Length; index++)
            {
                if (bankRenderers[index] != null)
                {
                    bankRenderers[index].SetPropertyBlock(_propertyBlock);
                }
            }
        }
    }
}
