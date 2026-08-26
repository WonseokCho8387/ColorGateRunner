using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class IceRunwayView : MonoBehaviour
    {
        internal const int RequiredPanelCount = 50;
        internal const float PanelWidth = 6.8f;
        internal const float PanelThickness = 0.05f;
        internal const float PanelCenterY = 0.125f;
        private const float SeamOverlap = 0.08f;

        [SerializeField] private Renderer[] panels;

        internal int PanelCount => panels == null ? 0 : panels.Length;

        internal int ActivePanelCount
        {
            get
            {
                int count = 0;
                if (panels == null)
                {
                    return count;
                }
                for (int index = 0; index < panels.Length; index++)
                {
                    if (panels[index] != null &&
                        panels[index].gameObject.activeSelf)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        internal void Configure(Renderer[] runwayPanels)
        {
            panels = runwayPanels;
            ResetRunway();
        }

        internal bool HasRequiredReferences()
        {
            if (panels == null || panels.Length != RequiredPanelCount)
            {
                return false;
            }
            for (int index = 0; index < panels.Length; index++)
            {
                if (panels[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void Build(
            StageSession session,
            float playerZ,
            float initialGateLeadDistance)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Ice runway requires exactly 50 configured panels.");
            }
            if (session.Stage.TargetGateCount > panels.Length)
            {
                throw new InvalidOperationException(
                    "The authored stage exceeds the fixed Ice runway pool.");
            }

            float previousGateZ = playerZ + initialGateLeadDistance;
            for (int index = 0; index < panels.Length; index++)
            {
                Renderer panel = panels[index];
                if (index >= session.Stage.TargetGateCount)
                {
                    panel.gameObject.SetActive(false);
                    continue;
                }

                GatePlan plan = session.GetGatePlan(index);
                float gateZ = previousGateZ + plan.Spacing;
                bool isIce = plan.Modifier.IsIce;
                panel.gameObject.SetActive(isIce);
                if (isIce)
                {
                    float length = Mathf.Max(
                        0.01f,
                        gateZ - previousGateZ + SeamOverlap);
                    panel.transform.position = new Vector3(
                        0f,
                        PanelCenterY,
                        previousGateZ + length * 0.5f);
                    panel.transform.localScale = new Vector3(
                        PanelWidth,
                        PanelThickness,
                        length);
                }
                previousGateZ = gateZ;
            }
        }

        internal void Build(
            StageSession session,
            CampaignSplinePathView path,
            float initialGateLeadDistance)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (path == null || path.PathLength <= 0f)
            {
                throw new ArgumentException(
                    "Ice runway requires a built Campaign Spline.",
                    nameof(path));
            }
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Ice runway requires exactly 50 configured panels.");
            }
            if (session.Stage.TargetGateCount > panels.Length)
            {
                throw new InvalidOperationException(
                    "The authored stage exceeds the fixed Ice runway pool.");
            }

            float previousGateDistance = initialGateLeadDistance;
            for (int index = 0; index < panels.Length; index++)
            {
                Renderer panel = panels[index];
                if (index >= session.Stage.TargetGateCount)
                {
                    panel.gameObject.SetActive(false);
                    continue;
                }

                GatePlan plan = session.GetGatePlan(index);
                float gateDistance = previousGateDistance + plan.Spacing;
                bool isIce = plan.Modifier.IsIce;
                panel.gameObject.SetActive(isIce);
                if (isIce)
                {
                    float length = Mathf.Max(
                        0.01f,
                        gateDistance - previousGateDistance + SeamOverlap);
                    float centerDistance = previousGateDistance +
                        (length * 0.5f);
                    path.EvaluatePose(
                        centerDistance,
                        PanelCenterY,
                        out Vector3 position,
                        out Quaternion rotation);
                    panel.transform.SetPositionAndRotation(position, rotation);
                    panel.transform.localScale = new Vector3(
                        PanelWidth,
                        PanelThickness,
                        length);
                }
                previousGateDistance = gateDistance;
            }
        }

        internal void ResetRunway()
        {
            if (panels == null)
            {
                return;
            }
            for (int index = 0; index < panels.Length; index++)
            {
                if (panels[index] != null)
                {
                    panels[index].gameObject.SetActive(false);
                }
            }
        }

        internal Renderer GetPanel(int index)
        {
            return panels[index];
        }
    }
}
