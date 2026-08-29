using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class GateBreakEffectPool : MonoBehaviour
    {
        [SerializeField] private GateBreakEffectView[] effects;
        private int _nextIndex;

        internal int Capacity => effects == null ? 0 : effects.Length;
        internal Material LastPlayedMaterial { get; private set; }

        internal void Configure(GateBreakEffectView[] pooledEffects)
        {
            effects = pooledEffects;
            ResetPool();
        }

        internal bool HasRequiredReferences()
        {
            if (effects == null || effects.Length != 6)
            {
                return false;
            }
            for (int index = 0; index < effects.Length; index++)
            {
                if (effects[index] == null || !effects[index].HasRequiredReferences())
                {
                    return false;
                }
            }
            return true;
        }

        internal void Play(Vector3 worldPosition, Material colorMaterial)
        {
            LastPlayedMaterial = colorMaterial;
            GateBreakEffectView effect = effects[_nextIndex];
            _nextIndex = (_nextIndex + 1) % effects.Length;
            effect.Play(worldPosition, colorMaterial);
        }

        internal void ResetPool()
        {
            _nextIndex = 0;
            LastPlayedMaterial = null;
            if (effects == null)
            {
                return;
            }
            for (int index = 0; index < effects.Length; index++)
            {
                effects[index]?.ResetEffect();
            }
        }

    }
}
