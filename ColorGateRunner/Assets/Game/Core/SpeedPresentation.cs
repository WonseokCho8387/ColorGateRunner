namespace ColorGateRunner.Core
{
    public enum SpeedStage
    {
        Start,
        Accelerating,
        Fast,
        MaximumPressure
    }

    public readonly struct SpeedPresentation
    {
        public SpeedPresentation(
            SpeedStage stage,
            float intensity,
            float cameraFieldOfView,
            float trailIntensity,
            float speedLineRate,
            float markerIntensity)
        {
            Stage = stage;
            Intensity = intensity;
            CameraFieldOfView = cameraFieldOfView;
            TrailIntensity = trailIntensity;
            SpeedLineRate = speedLineRate;
            MarkerIntensity = markerIntensity;
        }

        public SpeedStage Stage { get; }
        public float Intensity { get; }
        public float CameraFieldOfView { get; }
        public float TrailIntensity { get; }
        public float SpeedLineRate { get; }
        public float MarkerIntensity { get; }
    }
}
