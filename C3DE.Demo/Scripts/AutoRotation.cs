using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Demo.Scripts
{
    public class AutoRotation : Component
    {
        public Vector3 Rotation { get; set; }

        public override void Update()
        {
            sceneObject.Transform.Rotate(Rotation);
        }
    }
}
