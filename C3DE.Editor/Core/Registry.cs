using C3DE.Editor.Core.Components;

namespace C3DE.Editor.Core
{
    public sealed class Registry
    {
        public static EDMouseComponent Mouse { get; internal set; }
        public static EDKeyboardComponent Keys { get; internal set; }
    }
}
