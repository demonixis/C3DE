using System;
using C3DE.Editor.UI.Items;
using C3DE.Graphics.PostProcessing;
using Gwen.Control;
using Microsoft.Xna.Framework;

namespace C3DE.Editor.UI.Panels
{
    public sealed class SceneSettingsPanel : ControlBase
    {
        private readonly PropertyTree _tree;

        public SceneSettingsPanel(ControlBase parent) : base(parent)
        {
            _tree = new PropertyTree(this)
            {
                Dock = Gwen.Dock.Fill
            };
        }

        public void Bind(Scene scene, Action onDirty)
        {
            _tree.DeleteAllChildren();
            if (scene == null)
                return;

            var settings = scene.RenderSettings;
            var render = _tree.Add("Render Settings");
            AddBool(render, "Fog Enabled", settings.FogEnabled, value => { settings.FogEnabled = value; onDirty?.Invoke(); });
            AddEnum(render, "Fog Mode", settings.FogMode.ToString(), Enum.GetNames(typeof(FogMode)), value =>
            {
                settings.FogMode = (FogMode)Enum.Parse(typeof(FogMode), value);
                onDirty?.Invoke();
            });
            AddFloat(render, "Fog Density", settings.FogDensity, value => { settings.FogDensity = value; onDirty?.Invoke(); });
            AddFloat(render, "Fog Start", settings.FogStart, value => { settings.FogStart = value; onDirty?.Invoke(); });
            AddFloat(render, "Fog End", settings.FogEnd, value => { settings.FogEnd = value; onDirty?.Invoke(); });
            AddVector3(render, "Ambient", settings.AmbientColor.ToVector3(), value => { settings.AmbientColor = new Microsoft.Xna.Framework.Color(value); onDirty?.Invoke(); });
            AddVector3(render, "Fog Color", settings.FogColor.ToVector3(), value => { settings.FogColor = new Microsoft.Xna.Framework.Color(value); onDirty?.Invoke(); });
            AddBool(render, "Skybox Enabled", settings.Skybox.Enabled, value => { settings.Skybox.Enabled = value; onDirty?.Invoke(); });

            var post = settings.PostProcessing;
            var postSection = _tree.Add("Post Processing");
            AddBool(postSection, "Stack Enabled", post.Enabled, value => { post.Enabled = value; onDirty?.Invoke(); });
            AddEnum(postSection, "Debug View", post.DebugView.ToString(), Enum.GetNames(typeof(PostProcessDebugView)), value =>
            {
                post.DebugView = (PostProcessDebugView)Enum.Parse(typeof(PostProcessDebugView), value);
                onDirty?.Invoke();
            });
            AddBool(postSection, "Tonemapping", post.Tonemapping.Enabled, value => { post.Tonemapping.Enabled = value; onDirty?.Invoke(); });
            AddFloat(postSection, "Exposure", post.Tonemapping.Exposure, value => { post.Tonemapping.Exposure = value; onDirty?.Invoke(); });
            AddBool(postSection, "Bloom", post.Bloom.Enabled, value => { post.Bloom.Enabled = value; onDirty?.Invoke(); });
            AddFloat(postSection, "Bloom Intensity", post.Bloom.Intensity, value => { post.Bloom.Intensity = value; onDirty?.Invoke(); });
            AddFloat(postSection, "Bloom Threshold", post.Bloom.Threshold, value => { post.Bloom.Threshold = value; onDirty?.Invoke(); });
            AddBool(postSection, "AO", post.AmbientOcclusion.Enabled, value => { post.AmbientOcclusion.Enabled = value; onDirty?.Invoke(); });
            AddFloat(postSection, "AO Intensity", post.AmbientOcclusion.Intensity, value => { post.AmbientOcclusion.Intensity = value; onDirty?.Invoke(); });
            AddBool(postSection, "Sharpen", post.Sharpen.Enabled, value => { post.Sharpen.Enabled = value; onDirty?.Invoke(); });
            AddFloat(postSection, "Sharpen Intensity", post.Sharpen.Intensity, value => { post.Sharpen.Intensity = value; onDirty?.Invoke(); });
            AddBool(postSection, "FXAA", post.AntiAliasing.Enabled, value => { post.AntiAliasing.Enabled = value; onDirty?.Invoke(); });
            AddBool(postSection, "Vignette", post.Vignette.Enabled, value => { post.Vignette.Enabled = value; onDirty?.Invoke(); });
            AddFloat(postSection, "Vignette Intensity", post.Vignette.Intensity, value => { post.Vignette.Intensity = value; onDirty?.Invoke(); });

            _tree.ExpandAll();
        }

        private static void AddBool(Properties parent, string label, bool value, Action<bool> changed)
        {
            var control = new BoolControl(parent);
            control.SetBool(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddFloat(Properties parent, string label, float value, Action<float> changed)
        {
            var control = new FloatControl(parent);
            control.SetFloat(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddEnum(Properties parent, string label, string value, string[] values, Action<string> changed)
        {
            var control = new EnumControl(parent, values);
            control.SetSelected(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddVector3(Properties parent, string label, Vector3 value, Action<Vector3> changed)
        {
            var control = new Vector3Control(parent);
            control.SetVector(value);
            control.Vector3Changed += (_, x, y, z) => changed(new Vector3(x, y, z));
            parent.Add(label, control);
        }
    }
}
