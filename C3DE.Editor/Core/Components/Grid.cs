using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using C3DE.Components;
using C3DE.Components.Rendering;

namespace C3DE.Editor.GameComponents
{
    public sealed class Grid : Renderer
    {
        private BasicEffect m_Effect;
        private VertexPositionColor[] m_VertexData;
        private Color m_LineColor = Color.AntiqueWhite;
        private Color m_HighlightColor = Color.DarkBlue;
        private int m_GridSpacing = 1;
        private int m_GridSize = 512;
        private int m_NumberOfLines;

        public int GridSpacing
        {
            get { return m_GridSpacing; }
            set
            {
                m_GridSpacing = value;
                ComputeGrid();
            }
        }

        public int GridSize
        {
            get => m_GridSize;
            set
            {
                m_GridSize = value;
                ComputeGrid();
            }
        }

        public Color LineColor
        {
            get => m_LineColor;
            set
            {
                m_LineColor = value;
                ComputeGrid();
            }
        }

        public Color HighlightColor
        {
            get => m_HighlightColor;
            set
            {
                m_HighlightColor = value;
                ComputeGrid();
            }
        }

        public override void Start()
        {
            base.Start();

            m_Effect = new BasicEffect(Application.GraphicsDevice);
            m_Effect.VertexColorEnabled = true;
            m_Effect.World = Matrix.Identity;
            ComputeGrid();
        }

        public void ComputeGrid()
        {
            // calculate nr of lines, +2 for the highlights, +12 for boundingbox
            m_NumberOfLines = ((m_GridSize / m_GridSpacing) * 4) + 2 + 12;

            var vertexList = new List<VertexPositionColor>(m_NumberOfLines);

            for (int i = 1; i < (m_GridSize / m_GridSpacing) + 1; i++)
            {
                vertexList.Add(new VertexPositionColor(new Vector3((i * m_GridSpacing), 0, m_GridSize), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((i * m_GridSpacing), 0, -m_GridSize), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((-i * m_GridSpacing), 0, m_GridSize), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((-i * m_GridSpacing), 0, -m_GridSize), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(m_GridSize, 0, (i * m_GridSpacing)), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(-m_GridSize, 0, (i * m_GridSpacing)), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(m_GridSize, 0, (-i * m_GridSpacing)), m_LineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(-m_GridSize, 0, (-i * m_GridSpacing)), m_LineColor));
            }

            // add highlights
            vertexList.Add(new VertexPositionColor(Vector3.Forward * m_GridSize, m_HighlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Backward * m_GridSize, m_HighlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Right * m_GridSize, m_HighlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Left * m_GridSize, m_HighlightColor));

            // add boundingbox
            var box = new BoundingBox(new Vector3(-m_GridSize, -m_GridSize, -m_GridSize), new Vector3(m_GridSize, m_GridSize, m_GridSize));
            Vector3[] corners = new Vector3[8];

            box.GetCorners(corners);
            vertexList.Add(new VertexPositionColor(corners[0], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[1], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[0], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[3], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[0], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[4], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[1], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[2], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[1], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[5], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[2], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[3], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[2], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[6], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[3], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[7], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[4], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[5], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[4], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[7], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[5], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[6], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[6], m_LineColor));
            vertexList.Add(new VertexPositionColor(corners[7], m_LineColor));

            m_VertexData = vertexList.ToArray();
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            device.DepthStencilState = DepthStencilState.Default;
            m_Effect.View = Camera.Main.ViewMatrix;
            m_Effect.Projection = Camera.Main.ProjectionMatrix;
            m_Effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives(PrimitiveType.LineList, m_VertexData, 0, m_NumberOfLines);
        }
    }
}
