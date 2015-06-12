using System.Collections.Generic;
using C3DE.Components.Colliders;
using C3DE.Components;
using System.Threading.Tasks;

namespace C3DE
{
    public class CollisionSystem
    {
        private List<Collider> _colliders;

        public bool EnableParallelTask { get; set; }

        public CollisionSystem(Scene scene)
        {
            _colliders = scene.colliders;
            EnableParallelTask = true;
        }

        public void Update()
        {
            if (EnableParallelTask)
                ParallelCheckCollision();
            else
                CheckCollisions();
        }

        public void ParallelCheckCollision()
        {
            int size = _colliders.Count;

            Parallel.For(0, size, i =>
            {
                for (int j = 0; j < size; j++)
                {
                    if (_colliders[i] != _colliders[j] && _colliders[i].Collides(_colliders[j]))
                    {
                        var scripts = _colliders[i].GetComponents<Behaviour>();
                        var count = scripts.Length;
                        var inc = 0;

                        if (_colliders[i].IsTrigger)
                        {
                            for (inc = 0; inc < count; inc++)
                                scripts[inc].OnTriggerEnter();
                        }
                        else
                        {
                            _colliders[i].Transform.Position = _colliders[i].Transform.lastPosition;

                            for (inc = 0; inc < count; inc++)
                                scripts[inc].OnCollisionEnter();
                        }
                    }
                }
            });
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
                        var scripts = _colliders[i].GetComponents<Behaviour>();
                        var count = scripts.Length;
                        var inc = 0;

                        if (_colliders[i].IsTrigger)
                        {
                            for (inc = 0; inc < count; inc++)
                                scripts[inc].OnTriggerEnter();
                        }
                        else
                        {
                            _colliders[i].Transform.Position = _colliders[i].Transform.lastPosition;

                            for (inc = 0; inc < count; inc++)
                                scripts[inc].OnCollisionEnter();
                        }
                    }
                }
            }
        }
    }
}
