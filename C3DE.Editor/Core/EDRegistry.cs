using C3DE.Components;
using C3DE.Editor.Core.Components;

namespace C3DE.Editor.Core
{
    public sealed class EDRegistry
    {
        public const string ContentTempPath = "Temp";
        public static Camera Camera { get; internal set; }
        public static EDMouseComponent Mouse { get; internal set; }
        public static EDKeyboardComponent Keys { get; internal set; }
    }
}
