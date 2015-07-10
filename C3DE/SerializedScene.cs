using System;

namespace C3DE
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
