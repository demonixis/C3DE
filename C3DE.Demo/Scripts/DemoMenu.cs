using C3DE.Components;
using C3DE.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Demo.Scripts
{
    public sealed class DemoMenu : Behaviour
    {
        public struct BehaviourItem
        {
            public string Name { get; set; }
            public Behaviour Script { get; set; }
        }

        public override void Start()
        {
            base.Start();
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);
        }
    }
}
