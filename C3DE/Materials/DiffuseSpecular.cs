using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Materials
{
    public class DiffuseSpecular : Material
    {
        public DiffuseSpecular(Scene scene)
            : base (scene)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/DiffuseSpecularTextureEffect");
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // FIXME Do a loop when ok
            var light0 = scene.Lights[0];

            effect.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(0.6f);
            effect.Parameters["DiffuseColor"].SetValue(new Color(0.8f, 0.8f, 0.8f).ToVector4());
            effect.Parameters["SpecularColor"].SetValue(new Color(0.6f, 0.6f, 0.6f).ToVector4());
        }

        public override void Pass(RenderableComponent renderable)
        {
            Matrix m = Matrix.Transpose(Matrix.Invert(renderable.SceneObject.Transform.world));
            Vector3 camView = scene.MainCamera.SceneObject.Transform.Position;
            camView.Normalize();

            effect.Parameters["ModelTexture"].SetValue(mainTexture);
            //effect.Parameters["ViewVector"].SetValue(camView);
            effect.Parameters["WorldInverseTranspose"].SetValue(m);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
