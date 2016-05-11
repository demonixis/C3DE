using C3DE.Components;
using C3DE.PostProcess;
using C3DE.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    public abstract class Renderer
    {
        protected GraphicsDevice m_graphicsDevice;
        protected SpriteBatch m_spriteBatch;
        protected internal GUI uiManager;

        public bool NeedsBufferUpdate { get; set; }

        public Renderer()
        {
            NeedsBufferUpdate = false;
        }

        public virtual void Initialize(ContentManager content)
        {
            m_graphicsDevice = Application.GraphicsDevice;
            m_spriteBatch = new SpriteBatch(m_graphicsDevice);
            uiManager = new GUI(m_spriteBatch);
            uiManager.LoadContent(content);
        }

        protected virtual void RenderShadowMaps(Scene scene)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(m_graphicsDevice, scene.renderList);
        }

        protected virtual void RenderObjects(Scene scene, Camera camera)
        {
            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            // Pass, Update matrix, material attributes, etc.
            // Note: The renderList contains ONLY enabled components/objects.
            for (int i = 0; i < scene.renderList.Count; i++)
            {
                scene.renderList[i].Material.Pass(scene.RenderList[i]);
                scene.renderList[i].Draw(m_graphicsDevice);
            }
        }

        protected abstract void renderPostProcess(List<PostProcessPass> passes);

        protected virtual void RenderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;
            if (size > 0 && GUI.Enabled)
            {
                m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GUI.uiEffect, GUI.uiMatrix);

                for (int i = 0; i < size; i++)
                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(uiManager);

                m_spriteBatch.End();
            }
        }

        /// <summary>
        /// Render the scene.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        public abstract void Render(Scene scene);

        public abstract void RenderEditor(Scene scene, Camera camera, RenderTarget2D target);
    }
}
