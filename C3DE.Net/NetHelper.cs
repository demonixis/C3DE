using Microsoft.Xna.Framework;

namespace C3DE.Net
{
    public static class NetHelper
    {
        private static char[] _separator = new char[] { '_' };

        public static string Vector3ToString(Vector3 vec3)
        {
            return vec3.X + "_" + vec3.Y + "_" + vec3.Z;
        }

        public static Vector3 StringToVector3(string vec3str)
        {
            Vector3 vec3;
            StringToVector3(vec3str, out vec3);
            return vec3;
        }

        public static void StringToVector3(string vec3str, out Vector3 vec3)
        {
            string[] temp = vec3str.Split(_separator);
            vec3.X = float.Parse(temp[0]);
            vec3.Y = float.Parse(temp[1]);
            vec3.Z = float.Parse(temp[2]);
        }
    }
}
