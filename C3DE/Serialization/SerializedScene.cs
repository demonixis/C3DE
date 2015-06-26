using System;
using System.Collections.Generic;

namespace C3DE.Serialization
{
    [Serializable]
    public class SerializedScene
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Dictionary<string, object> RenderSettings { get; set; }
        public Dictionary<string, object>[] Materials { get; set; }
        public Dictionary<string, object>[] SceneObjects { get; set; }
    }
}
