using System.Collections.Generic;
using C3DE.Components.Colliders;

namespace C3DE
{
    public class CollisionSystem
    {
        private List<Collider> _colliders;

        public CollisionSystem(Scene scene)
        {
            _colliders = scene.colliders;
        }

        public void Update()
        {
            CheckCollisions();
        }

        public void CheckCollisions()
        {
            int size = _colliders.Count;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (_colliders[i] != _colliders[j] && _colliders[i].Collides(_colliders[j]))
                    {
                        _colliders[i].Transform.Position = _colliders[i].Transform.lastPosition;
                    }
                }
            }
        }

    }
}
