using System.Collections.Generic;

namespace C3DE
{
    public class SceneManager
    {
        private List<Scene> _scenes;
        private int _levelToLoad;
        private int _activeSceneIndex;

        public Scene ActiveScene
        {
            get { return _scenes[_activeSceneIndex]; }
            set { LoadLevel(value); }
        }

        public SceneManager()
        {
            _scenes = new List<Scene>(3);
            _levelToLoad = -1;
            _activeSceneIndex = 0;

            _scenes.Add(new Scene("DefaultScene"));
        }

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

        public void LoadLevel(string name)
        {
            var scene = GetSceneIndexByName(name);
            LoadLevel(scene);
        }

        public void LoadLevel(int index)
        {
            if (index >= 0 && index < _scenes.Count)
                _levelToLoad = index;
        }

        public void Initialize()
        {
            if (_activeSceneIndex > -1)
                _scenes[_activeSceneIndex].Initialize();
        }

        public void Update()
        {
            if (_levelToLoad > -1)
            {
                _activeSceneIndex = _levelToLoad;
                _levelToLoad = -1;
                _scenes[_activeSceneIndex].Initialize();
            }

            _scenes[_activeSceneIndex].Update();
        }

        private int GetSceneIndexByName(string name)
        {
            for (int i = 0, l = _scenes.Count; i < l; i++)
                if (_scenes[i].Name == name)
                    return i;

            return -1;
        }
    }
}
