using System;
using System.Collections.Generic;

namespace C3DE.Serialization
{
    [Serializable]
    public class SerializedScene
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public SerializedCollection RenderSettings { get; set; }
        public SerializedCollection[] Materials { get; set; }
        public SerializedCollection[] SceneObjects { get; set; }
        public SerializedCollection[] Components { get; set; }
    }
}
