using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace C3DE.Editor.Serialization
{
    public sealed class SceneData
    {
        public string SceneGuid { get; set; }

        public string Name { get; set; }

        public RenderSettingsData RenderSettings { get; set; } = new RenderSettingsData();

        public List<GameObjectData> GameObjects { get; set; } = new List<GameObjectData>();
    }

    public sealed class GameObjectData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; }

        public bool Enabled { get; set; }

        public bool IsStatic { get; set; }

        public string ParentId { get; set; }

        public List<ComponentData> Components { get; set; } = new List<ComponentData>();
    }

    public sealed class ComponentData
    {
        public string Type { get; set; }

        public JObject Data { get; set; } = new JObject();
    }

    public sealed class RenderSettingsData
    {
        public bool FogEnabled { get; set; }
        public int FogMode { get; set; }
        public float FogDensity { get; set; }
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        public float[] FogColor { get; set; } = new float[3];
        public float[] AmbientColor { get; set; } = new float[3];
        public bool SkyboxEnabled { get; set; } = true;
        public PostProcessingData PostProcessing { get; set; } = new PostProcessingData();
    }

    public sealed class PostProcessingData
    {
        public bool Enabled { get; set; }
        public int DebugView { get; set; }
        public bool TonemappingEnabled { get; set; }
        public float Exposure { get; set; }
        public bool ColorAdjustmentsEnabled { get; set; }
        public float Contrast { get; set; }
        public float Saturation { get; set; }
        public float Temperature { get; set; }
        public float Tint { get; set; }
        public float[] Lift { get; set; } = new float[3];
        public float[] Gamma { get; set; } = new float[3];
        public float[] Gain { get; set; } = new float[3];
        public bool BloomEnabled { get; set; }
        public float BloomThreshold { get; set; }
        public float BloomSoftKnee { get; set; }
        public float BloomIntensity { get; set; }
        public float BloomBlurSize { get; set; }
        public int BloomBlurIterations { get; set; }
        public int BloomResolution { get; set; }
        public bool AmbientOcclusionEnabled { get; set; }
        public float AmbientOcclusionIntensity { get; set; }
        public float AmbientOcclusionRadius { get; set; }
        public float AmbientOcclusionBias { get; set; }
        public float AmbientOcclusionBlurSharpness { get; set; }
        public bool SharpenEnabled { get; set; }
        public float SharpenIntensity { get; set; }
        public bool AntiAliasingEnabled { get; set; }
        public float FxaaSpanMax { get; set; }
        public float FxaaReduceMin { get; set; }
        public float FxaaReduceMul { get; set; }
        public bool VignetteEnabled { get; set; }
        public float VignetteIntensity { get; set; }
        public float VignetteSmoothness { get; set; }
        public float VignetteRoundness { get; set; }
    }
}
