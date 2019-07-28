using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering.Particles
{
    public struct ParticleVertex
    {
        public Vector3 Position;
        public Vector2 Corner;
        public Vector3 Velocity;
        public Color Random;
        public float Time;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.Normal, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),
            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
        );

        public const int SizeInBytes = 40;
    }
}
