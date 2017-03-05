using C3DE.Components;
using C3DE.UI;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A scene manager. A default scene is created during the construction and can be directly used.
    /// </summary>
    public class SceneManager
    {
        private List<Scene> _scenes;
        private int _levelToLoad;
        private int _activeSceneIndex;

        public Scene this[int index]
        {
            get { return _scenes[index]; }
        }

        public Scene ActiveScene
        {
            get { return _activeSceneIndex >= 0 ? _scenes[_activeSceneIndex] : null; }
        }

        public int Count
        {
            get { return _scenes.Count; }
        }

        /// <summary>
        /// Create the scene manager and initialize a default scene.
        /// </summary>
        public SceneManager()
        {
            _scenes = new List<Scene>(3);
            _levelToLoad = -1;
            _activeSceneIndex = -1;
        }

        /// <summary>
        /// Load a scene and add it to the manager if it doesn't yet added.
        /// </summary>
        /// <param name="scene">The scene to load.</param>
        public void LoadLevel(Scene scene)
        {
            if (scene != null)
            {
                var index = _scenes.IndexOf(scene);

                if (index == -1)
                {
                    _scenes.Add(scene);
                    index = _scenes.Count - 1;
                }

                _levelToLoad = index;
            }
        }

        /// <summary>
        /// Load a scene by it's name.
        /// </summary>
        /// <param name="name">The scene's name to load.</param>
        public void LoadLevel(string name)
        {
            var scene = GetSceneIndexByName(name);
            LoadLevel(scene);
        }

        /// <summary>
        /// Load a scene by its index. 0 is the default scene.
        /// </summary>
        /// <param name="index">The scene's index to load.</param>
        public void LoadLevel(int index)
        {
            if (index >= 0 && index < _scenes.Count)
                _levelToLoad = index;
        }

        /// <summary>
        /// Add a scene to the manager.
        /// </summary>
        /// <param name="scene">The scene to add.</param>
        /// <param name="isActive">Set to true to use this scene as active scene.</param>
        public void Add(Scene scene, bool isActive = false)
        {
            if (!_scenes.Contains(scene))
            {
                _scenes.Add(scene);

                if (isActive)
                    _levelToLoad = _scenes.Count - 1;
            }
        }

        /// <summary>
        /// Remove a scene from the manager. Notice that you can't remove the default scene.
        /// </summary>
        /// <param name="scene">The scene to remove.</param>
        public void Remove(Scene scene)
        {
            var index = _scenes.IndexOf(scene);

            if (index > 0) // Exclude the default scene
            {
                if (_activeSceneIndex == index)
                    _activeSceneIndex = _scenes.Count - 1;

                _scenes[index].Unload();

                _scenes.RemoveAt(index);
            }
        }

        /// <summary>
        /// Check if a scene need to be loaded (and load it) and update the active scene.
        /// </summary>
        public void Update()
        {
            if (_activeSceneIndex == -1 && _levelToLoad == -1)
                return;

            if (_levelToLoad > -1)
            {
                if (_activeSceneIndex > -1)
                    _scenes[_activeSceneIndex].Unload();

                _activeSceneIndex = _levelToLoad;

                Camera.main = null;
                GUI.Enabled = true;
                GUI.Effect = null;
                Scene.current = _scenes[_activeSceneIndex];

                _levelToLoad = -1;
                _scenes[_activeSceneIndex].Initialize();
            }

            _scenes[_activeSceneIndex].Update();
        }

        /// <summary>
        /// Gets a scene index by its name.
        /// </summary>
        /// <param name="name">The scene's name to find.</param>
        /// <returns>Returns the index of the scene or -1.</returns>
        private int GetSceneIndexByName(string name)
        {
            for (int i = 0, l = _scenes.Count; i < l; i++)
                if (_scenes[i].Name == name)
                    return i;

            return -1;
        }
    }
}
