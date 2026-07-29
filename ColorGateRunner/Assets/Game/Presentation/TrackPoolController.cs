using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class TrackPoolController : MonoBehaviour
    {
        [SerializeField] private TrackSegmentView[] segments;
        [SerializeField] private float segmentLength = 40f;
        [SerializeField] private float initialStartZ = -20f;
        [SerializeField] private float recycleBehindDistance = 20f;

        internal int SegmentCount => segments == null ? 0 : segments.Length;
        internal float SegmentLength => segmentLength;

        internal TrackSegmentView GetSegment(int index)
        {
            return segments[index];
        }

        internal void ResetPool()
        {
            for (int index = 0; index < segments.Length; index++)
            {
                segments[index].PlaceAt(
                    new Vector3(
                        0f,
                        0f,
                        initialStartZ + (index * segmentLength)),
                    Quaternion.identity);
            }
        }

        internal void Tick(float playerZ)
        {
            for (int pass = 0; pass < segments.Length; pass++)
            {
                int recycleIndex = FindSegmentBehind(playerZ);
                if (recycleIndex < 0)
                {
                    return;
                }

                int farthestIndex = FindFarthestSegment(recycleIndex);
                segments[recycleIndex].PlaceAt(
                    segments[farthestIndex].EndAnchorPosition,
                    Quaternion.identity);
            }
        }

        internal void SetSurfaceMaterial(Material material)
        {
            for (int index = 0; index < segments.Length; index++)
            {
                segments[index].SetSurfaceMaterial(material);
            }
        }

        internal bool HasRequiredReferences()
        {
            if (segments == null || segments.Length < 3 || segmentLength <= 0f)
            {
                return false;
            }

            for (int index = 0; index < segments.Length; index++)
            {
                if (segments[index] == null ||
                    !segments[index].HasRequiredReferences)
                {
                    return false;
                }
            }

            return true;
        }

        internal void Configure(
            TrackSegmentView[] trackSegments,
            float length,
            float firstStartZ,
            float behindDistance)
        {
            segments =
                trackSegments ?? throw new ArgumentNullException(nameof(trackSegments));
            segmentLength = length;
            initialStartZ = firstStartZ;
            recycleBehindDistance = behindDistance;
        }

        private int FindSegmentBehind(float playerZ)
        {
            float threshold = playerZ - recycleBehindDistance;
            for (int index = 0; index < segments.Length; index++)
            {
                if (segments[index].EndAnchorPosition.z < threshold)
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindFarthestSegment(int excludedIndex)
        {
            int farthestIndex = excludedIndex == 0 ? 1 : 0;
            float farthestZ = segments[farthestIndex].EndAnchorPosition.z;
            for (int index = 0; index < segments.Length; index++)
            {
                if (index == excludedIndex)
                {
                    continue;
                }

                float endZ = segments[index].EndAnchorPosition.z;
                if (endZ > farthestZ)
                {
                    farthestZ = endZ;
                    farthestIndex = index;
                }
            }

            return farthestIndex;
        }
    }
}
