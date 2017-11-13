using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Primitives
{
    [DataContract]
    public class CubeMesh : Mesh
    {
        protected override void CreateGeometry()
        {
            Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, 1.0f);
            Vector3 bottomLeftFront = new Vector3(-1.0f, -1.0f, 1.0f);
            Vector3 topRightFront = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 bottomRightFront = new Vector3(1.0f, -1.0f, 1.0f);
            Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, -1.0f);
            Vector3 topRightBack = new Vector3(1.0f, 1.0f, -1.0f);
            Vector3 bottomLeftBack = new Vector3(-1.0f, -1.0f, -1.0f);
            Vector3 bottomRightBack = new Vector3(1.0f, -1.0f, -1.0f);

            //Texture Positions
            Vector2 TtopLeftBack = new Vector2(0.0f, 0.0f);
            Vector2 TtopRightBack = new Vector2(1.0f, 0.0f);
            Vector2 TtopLeftFront = new Vector2(0.0f, 1.0f);
            Vector2 TtopRightFront = new Vector2(1.0f, 1.0f);

            Vector2 TbottomLeftBack = new Vector2(1.0f, 1.0f);
            Vector2 TbottomLeftFront = new Vector2(0.0f, 1.0f);
            Vector2 TbottomRightBack = new Vector2(1.0f, 0.0f);
            Vector2 TbottomRightFront = new Vector2(0.0f, 0.0f);

            Vector2 frontTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 frontTopRight = new Vector2(1.0f, 0.0f);
            Vector2 frontBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 frontBottomRight = new Vector2(1.0f, 1.0f);

            Vector2 backTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 backTopRight = new Vector2(1.0f, 0.0f);
            Vector2 backBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 backBottomRight = new Vector2(1.0f, 1.0f);

            Vector2 rightTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 rightTopRight = new Vector2(1.0f, 0.0f);
            Vector2 rightBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 rightBottomRight = new Vector2(1.0f, 1.0f);

            Vector2 leftTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 leftTopRight = new Vector2(1.0f, 0.0f);
            Vector2 leftBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 leftBottomRight = new Vector2(1.0f, 1.0f);

            // Normals
            Vector3 frontNormal = new Vector3(0, 0, 1);
            Vector3 backNormal = new Vector3(0, 0, -1);
            Vector3 leftNormal = new Vector3(-1, 0, 0);
            Vector3 rightNormal = new Vector3(1, 0, 0);
            Vector3 topNormal = new Vector3(0, 1, 0);
            Vector3 bottomNormal = new Vector3(0, -1, 0);

            Vertices = new VertexPositionNormalTexture[]
            {
                // Front Surface
                new VertexPositionNormalTexture(bottomLeftFront,frontNormal,frontBottomLeft),
                new VertexPositionNormalTexture(topLeftFront ,frontNormal,frontTopLeft), 
                new VertexPositionNormalTexture(bottomRightFront,frontNormal,frontBottomRight),
                new VertexPositionNormalTexture(topRightFront,frontNormal,frontTopRight),  

                // Back Surface
                new VertexPositionNormalTexture(bottomRightBack,backNormal,backBottomLeft),
                new VertexPositionNormalTexture(topRightBack,backNormal,backTopLeft), 
                new VertexPositionNormalTexture(bottomLeftBack,backNormal,backBottomRight),
                new VertexPositionNormalTexture(topLeftBack,backNormal,backTopRight), 

                // Left Surface
                new VertexPositionNormalTexture(bottomLeftBack,leftNormal,leftBottomRight),
                new VertexPositionNormalTexture(topLeftBack,leftNormal,leftTopRight),
                new VertexPositionNormalTexture(bottomLeftFront,leftNormal,leftBottomLeft),
                new VertexPositionNormalTexture(topLeftFront,leftNormal,leftTopLeft),
                // Right Surface
                new VertexPositionNormalTexture(bottomRightFront,rightNormal,rightBottomLeft),
                new VertexPositionNormalTexture(topRightFront,rightNormal,rightTopLeft),
                new VertexPositionNormalTexture(bottomRightBack,rightNormal,rightBottomRight),
                new VertexPositionNormalTexture(topRightBack,rightNormal,rightTopRight),
                // Top Surface
                new VertexPositionNormalTexture(topLeftFront,topNormal,TtopLeftFront),
                new VertexPositionNormalTexture(topLeftBack,topNormal,TtopLeftBack),
                new VertexPositionNormalTexture(topRightFront,topNormal,TtopRightFront),
                new VertexPositionNormalTexture(topRightBack,topNormal,TtopRightBack),

                // Bottom Surface
                new VertexPositionNormalTexture(bottomLeftBack,bottomNormal,TbottomRightBack),
                new VertexPositionNormalTexture(bottomLeftFront,bottomNormal,TbottomRightFront),
                new VertexPositionNormalTexture(bottomRightBack,bottomNormal,TbottomLeftBack),
                new VertexPositionNormalTexture(bottomRightFront,bottomNormal,TbottomLeftFront),
            };

            Indices = new ushort[] { 
                0, 1, 2, 2, 1, 3,   
                4, 5, 6, 6, 5, 7,
                8, 9, 10, 10, 9, 11, 
                12, 13, 14, 14, 13, 15, 
                16, 17, 18, 18, 17, 19,
                20, 21, 22, 22, 21, 23
            };
        }
    }
}
