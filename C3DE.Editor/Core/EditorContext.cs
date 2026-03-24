using System;
using C3DE.Editor.Assets;
using C3DE.Editor.ProjectSystem;

namespace C3DE.Editor.Core
{
    public sealed class EditorContext
    {
        public ProjectData CurrentProject { get; private set; }

        public string CurrentProjectPath { get; private set; }

        public EditorScene CurrentScene { get; private set; }

        public string CurrentScenePath { get; private set; }

        public GameObject SelectedGameObject { get; private set; }

        public AssetDatabase AssetDatabase { get; private set; }

        public bool IsSceneDirty { get; private set; }

        public bool IsProjectDirty { get; private set; }

        public event Action ProjectChanged;

        public event Action SceneChanged;

        public event Action SelectionChanged;

        public event Action DirtyStateChanged;

        public void SetProject(ProjectData project, string projectPath)
        {
            CurrentProject = project;
            CurrentProjectPath = projectPath;
            AssetDatabase = string.IsNullOrEmpty(projectPath) ? null : new AssetDatabase(projectPath);
            AssetDatabase?.Scan();
            IsProjectDirty = false;
            ProjectChanged?.Invoke();
            DirtyStateChanged?.Invoke();
        }

        public void SetScene(EditorScene scene, string scenePath, bool isDirty = false)
        {
            CurrentScene = scene;
            CurrentScenePath = scenePath;
            SelectedGameObject = null;
            IsSceneDirty = isDirty;
            SceneChanged?.Invoke();
            SelectionChanged?.Invoke();
            DirtyStateChanged?.Invoke();
        }

        public void SetSelection(GameObject gameObject)
        {
            if (SelectedGameObject == gameObject)
                return;

            SelectedGameObject = gameObject;
            SelectionChanged?.Invoke();
        }

        public void MarkSceneDirty()
        {
            if (IsSceneDirty)
                return;

            IsSceneDirty = true;
            DirtyStateChanged?.Invoke();
        }

        public void ClearSceneDirty()
        {
            if (!IsSceneDirty)
                return;

            IsSceneDirty = false;
            DirtyStateChanged?.Invoke();
        }

        public void MarkProjectDirty()
        {
            if (IsProjectDirty)
                return;

            IsProjectDirty = true;
            DirtyStateChanged?.Invoke();
        }

        public void ClearProjectDirty()
        {
            if (!IsProjectDirty)
                return;

            IsProjectDirty = false;
            DirtyStateChanged?.Invoke();
        }
    }
}
