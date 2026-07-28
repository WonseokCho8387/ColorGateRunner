namespace ColorGateRunner.Core
{
    public readonly struct StageSectionInfo
    {
        public StageSectionInfo(
            string name,
            int startGate,
            int gateCount,
            GatePatternType pattern,
            float cadenceMultiplier)
        {
            Name = name;
            StartGate = startGate;
            GateCount = gateCount;
            Pattern = pattern;
            CadenceMultiplier = cadenceMultiplier;
        }

        public string Name { get; }
        public int StartGate { get; }
        public int GateCount { get; }
        public GatePatternType Pattern { get; }
        public float CadenceMultiplier { get; }
    }

    public static class StageSectionCatalog
    {
        private static readonly StageSectionInfo[] StageTwo =
        {
            new StageSectionInfo("Steady", 0, 6, GatePatternType.Steady, 1f),
            new StageSectionInfo("Compression", 6, 6, GatePatternType.Compression, 0.92f),
            new StageSectionInfo("Release", 12, 6, GatePatternType.Release, 1.08f),
            new StageSectionInfo("Syncopation", 18, 5, GatePatternType.Syncopation, 0.9f),
            new StageSectionInfo("Mixed Final", 23, 5, GatePatternType.Compression, 0.86f)
        };

        private static readonly StageSectionInfo[] StageThree =
        {
            new StageSectionInfo("Red-led exception", 0, 8, GatePatternType.SameColorBait, 1.05f),
            new StageSectionInfo("Blue-led exception", 8, 6, GatePatternType.SingleColorBreak, 0.96f),
            new StageSectionInfo("Long attention", 14, 8, GatePatternType.SameColorBait, 1.02f),
            new StageSectionInfo("Release burst", 22, 4, GatePatternType.Release, 1.12f),
            new StageSectionInfo("Exception final", 26, 4, GatePatternType.Burst, 0.9f)
        };

        public static int GetSectionCount(int stageNumber)
        {
            StageSectionInfo[] sections = GetSections(stageNumber);
            return sections == null ? 0 : sections.Length;
        }

        public static StageSectionInfo GetSection(int stageNumber, int index)
        {
            return GetSections(stageNumber)[index];
        }

        public static StageSectionInfo FindSection(
            int stageNumber,
            int gateIndex)
        {
            StageSectionInfo[] sections = GetSections(stageNumber);
            if (sections == null)
            {
                return default;
            }

            for (int index = 0; index < sections.Length; index++)
            {
                StageSectionInfo section = sections[index];
                if (gateIndex >= section.StartGate &&
                    gateIndex < section.StartGate + section.GateCount)
                {
                    return section;
                }
            }

            return sections[sections.Length - 1];
        }

        private static StageSectionInfo[] GetSections(int stageNumber)
        {
            if (stageNumber == 2)
            {
                return StageTwo;
            }
            if (stageNumber == 3)
            {
                return StageThree;
            }
            return null;
        }
    }
}
