using C3DE.Components;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class WaterMaterial : Material
    {
        private Vector4 _specularColor;
        private float _totalTime;

        public Texture2D BumpTexture { get; set; }
        public float WaterTransparency { get; set; }
        public float DiffuseIntensity { get; set; }
        public float SpecularIntensity { get; set; }
        public float Shininess { get; set; }

        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector4(); }
        }

        public WaterMaterial(Scene scene)
            : base(scene)
        {
            WaterTransparency = 0.45f;
            DiffuseColor = Color.White;
            DiffuseIntensity = 1.0f;
            SpecularIntensity = 0.0f;
            Shininess = 250.0f;
            _specularColor = Color.Red.ToVector4();// new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
            _totalTime = 0.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/WaterEffect");
        }

        public override void PrePass()
        {
            _totalTime += Time.DeltaTime / 10.0f;

            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["CameraPosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);

            // FIXME Do a loop when ok
            var light0 = scene.Lights[0];
            if (light0.Type != LightType.Directional)
            {
                Vector3 cDir = light0.SceneObject.Transform.Position;
                cDir.Normalize();
                effect.Parameters["LightDirection"].SetValue(cDir);
            }
            else
                effect.Parameters["LightDirection"].SetValue(light0.Direction);

            // Light
            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(1.0f);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["DiffuseIntensity"].SetValue(DiffuseIntensity);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["TotalTime"].SetValue(_totalTime);
            effect.Parameters["WaterTexture"].SetValue(mainTexture);
            effect.Parameters["NormalTexture"].SetValue(BumpTexture);
            effect.Parameters["Alpha"].SetValue(WaterTransparency);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
