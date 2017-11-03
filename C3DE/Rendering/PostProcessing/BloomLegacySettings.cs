using System.Collections.Generic;

namespace C3DE.Rendering.PostProcessing
{
    /// <summary>
    /// Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomLegacySettings
    {
        #region Fields

        // Name of a preset bloom setting, for display to the user.
        public readonly string Name;

        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;

        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;

        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;

        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;

        #endregion

        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomLegacySettings(string name, float bloomThreshold, float blurAmount, float bloomIntensity, float baseIntensity, float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }

        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomLegacySettings[] PresetSettings =
        {
            //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
            new BloomLegacySettings("Default",     0.25f,  4,   1.25f, 1,    1,       1),
            new BloomLegacySettings("Soft",        0,      3,   1,     1,    1,       1),
            new BloomLegacySettings("Desaturated", 0.5f,   8,   2,     1,    0,       1),
            new BloomLegacySettings("Saturated",   0.25f,  4,   2,     1,    2,       0),
            new BloomLegacySettings("Blurry",      0,      2,   1,     0.1f, 1,       1),
            new BloomLegacySettings("Subtle",      0.5f,   2,   1,     1,    1,       1),
        };

        public static Dictionary<BloomPreset, BloomLegacySettings> Presets = new Dictionary<BloomPreset, BloomLegacySettings>()
        {
            { BloomPreset.Default, new BloomLegacySettings("Default",     0.25f,  4,   1.25f, 1,    1,       1) },
            { BloomPreset.Soft, new BloomLegacySettings("",        0,      3,   1,     1,    1,       1) },
            { BloomPreset.Desaturated, new BloomLegacySettings("Desaturated", 0.5f,   8,   2,     1,    0,       1)},
            { BloomPreset.Saturated, new BloomLegacySettings("Saturated",   0.25f,  4,   2,     1,    2,       0)},
            { BloomPreset.Blurry, new BloomLegacySettings("Blurry",      0,      2,   1,     0.1f, 1,       1)},
            { BloomPreset.Subtle, new BloomLegacySettings("Subtle",      0.5f,   2,   1,     1,    1,       1)}
        };
    }

    public enum BloomPreset
    {
        Default, Soft, Desaturated, Saturated, Blurry, Subtle
    }
}
