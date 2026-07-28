namespace ColorGateRunner.Core
{
    public readonly struct StartItemSelection
    {
        public StartItemSelection(bool shield, bool booster)
        {
            Shield = shield;
            Booster = booster;
        }

        public bool Shield { get; }
        public bool Booster { get; }
        public bool UsesAnyItem => Shield || Booster;
    }
}
