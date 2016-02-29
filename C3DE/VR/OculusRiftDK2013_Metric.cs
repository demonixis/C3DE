namespace C3DE.VR
{
    public sealed class OculusRiftDK2013_Metric
    {
        public const int HResolution = 1280;
        public const int VResolution = 800;
        public const float HScreenSize = 0.149759993f;
        public const float VScreenSize = 0.0935999975f;
        public const float VScreenCenter = 0.0467999987f;
        public const float EyeToScreenDistance = 0.0410000011f;
        public const float LensSeparationDistance = 0.0635000020f;
        public const float InterpupillaryDistance = 0.0640000030f;
        public static readonly float[] DistortionK = new float[4] { 1.0f, 0.219999999f, 0.239999995f, 0.0f };
        public static readonly float[] ChromaAbCorrection = new float[4] { 0.995999992f, -0.00400000019f, 1.01400006f, 0.0f };
        public const float PostProcessScaleFactor = 1.714605507808412f;
        public const float LensCenterOffset = 0.151976421f;
    }
}
