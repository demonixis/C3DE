using C3DE.Editor;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scripts.Editor
{
    class CommandManager
    {
        private EditorDemo m_EditorScene;

        #region New / Save and Load scene

        public void NewScene()
        {
            m_EditorScene.Reset();
        }

        public bool SaveScene(string path)
        {
            var result = true;

            try
            {
                var serScene = new SerializedScene()
                {
                    Materials = m_EditorScene.Materials.ToArray(),
                    GameObjects = m_EditorScene.GetUsedSceneObjects(),
                    RenderSettings = m_EditorScene.RenderSettings
                };

                Serializer.Serialize(path, serScene);
            }
            catch (Exception ex)
            {
                result = false;
                Debug.Log(ex.Message);
            }

            return result;
        }

        public bool LoadScene(string path)
        {
            var result = true;

            try
            {
                var data = Serializer.Deserialize(path, typeof(SerializedScene));
                var serializedScene = data as SerializedScene;
                if (serializedScene != null)
                {
                    NewScene();

                    foreach (var so in serializedScene.GameObjects)
                    {
                        so.PostDeserialize();
                        m_EditorScene.Add(so);
                    }

                    m_EditorScene.RenderSettings.Set(serializedScene.RenderSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                result = false;
            }

            return result;
        }

        #endregion

        #region Live import

        public Texture2D LoadTempTexture(string assetName) => null;
        public Model LoadTempModel(string assetName) => null;
        public SpriteFont LoadTempFont(string assetName) => null;
        public Effect LoadTempEffect(string assetName) => null;
        public Model AddModelFromTemp(string assetName) => null;

        #endregion
    }
}
