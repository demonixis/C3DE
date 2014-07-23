using C3DE.Components;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// The scene is responsible to store scene objects.
    /// </summary>
    public class Scene : SceneObject
    {
        private List<SceneObject> members;
        private int count;
        private List<ModelRenderer> _renderList;

        public SceneObject this[int index]
        {
            get { return members[index]; }
            set { members[index] = value; }
        }

        /// <summary>
        /// Gets the size of the scene. This value is cached so you can use it in a for loop.
        /// </summary>
        public int Size
        {
            get { return count; }
        }

        /// <summary>
        /// Gets the collection of renderable scene objects.
        /// </summary>
        public List<ModelRenderer> RenderList
        {
            get { return _renderList; }
        }

        public Scene()
            : base()
        {
            transform.Root = transform;
            isStatic = false;
            members = new List<SceneObject>();
            count = 0;
            _renderList = new List<ModelRenderer>();
        }

        /// <summary>
        /// Add a scene object to the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object to add.</param>
        /// <returns>Return true if the scene object is added, otherwise return false.</returns>
        public override bool Add(SceneObject sceneObject)
        {
            bool canAdd = base.Add(sceneObject);

            if (canAdd)
            {
                members.Add(sceneObject);
                count++;

                ModelRenderer modelRenderer = sceneObject.GetComponent<ModelRenderer>();

                if (modelRenderer != null)
                    _renderList.Add(modelRenderer);

                sceneObject.ComponentChanged += sceneObject_ComponentsChanged;
            }

            return canAdd;
        }

        /// <summary>
        /// Remove a scene object from the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object to remove.</param>
        /// <returns>Return true if the component has been removed, otherwise return false.</returns>
        public override bool Remove(SceneObject sceneObject)
        {
            bool canRemove = base.Remove(sceneObject);
            int index = -1;

            if (canRemove)
            {
                index = members.IndexOf(sceneObject);

                if (index > -1)
                {
                    members.RemoveAt(index);
                    count--;

                    ModelRenderer modelRenderer = sceneObject.GetComponent<ModelRenderer>();

                    if (modelRenderer != null)
                        _renderList.Remove(modelRenderer);

                    sceneObject.ComponentChanged -= sceneObject_ComponentsChanged;
                }
            }

            return canRemove && (index > -1);
        }

        /// <summary>
        /// Update all scene object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            for (int i = 0; i < count; i++)
            {
                if (members[i].Enabled)
                    members[i].Update();
            }
        }

        /// <summary>
        /// Called when a component is added to a registered scene object.
        /// It's actually used to update the render list.
        /// </summary>
        /// <param name="sender">The scene object which as added or removed a component.</param>
        /// <param name="e">An object which contains the component and a flag to know if it's added or removed.</param>
        private void sceneObject_ComponentsChanged(object sender, ComponentChangedEventArgs e)
        {
            ModelRenderer modelRenderer = e.Component as ModelRenderer;

            if (modelRenderer != null)
            {
                if (e.Added)
                    _renderList.Add(modelRenderer);
                else
                    _renderList.Remove(modelRenderer);
            }
        }
    }
}
