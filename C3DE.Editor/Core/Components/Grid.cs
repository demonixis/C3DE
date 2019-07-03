using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using C3DE.Components;
using C3DE.Components.Rendering;

namespace C3DE.Editor.GameComponents
{
    public sealed class Grid : Renderer
    {
        private BasicEffect _effect;
        private VertexPositionColor[] _vertexData;
        private Color _lineColor = Color.AntiqueWhite;
        private Color _highlightColor = Color.DarkBlue;
        private int _gridSpacing = 1;
        private int _gridSize = 512;
        private int _numberOfLines;

        public int GridSpacing
        {
            get => _gridSpacing;
            set
            {
                _gridSpacing = value;
                ComputeGrid();
            }
        }

        public int GridSize
        {
            get => _gridSize;
            set
            {
                _gridSize = value;
                ComputeGrid();
            }
        }

        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                ComputeGrid();
            }
        }

        public Color HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                ComputeGrid();
            }
        }

        public override void Start()
        {
            base.Start();

            _effect = new BasicEffect(Application.GraphicsDevice);
            _effect.VertexColorEnabled = true;
            _effect.World = Matrix.Identity;
            ComputeGrid();
        }

        public void ComputeGrid()
        {
            // calculate nr of lines, +2 for the highlights, +12 for boundingbox
            _numberOfLines = ((_gridSize / _gridSpacing) * 4) + 2 + 12;

            var vertexList = new List<VertexPositionColor>(_numberOfLines);

            for (int i = 1; i < (_gridSize / _gridSpacing) + 1; i++)
            {
                vertexList.Add(new VertexPositionColor(new Vector3((i * _gridSpacing), 0, _gridSize), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((i * _gridSpacing), 0, -_gridSize), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((-i * _gridSpacing), 0, _gridSize), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3((-i * _gridSpacing), 0, -_gridSize), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(_gridSize, 0, (i * _gridSpacing)), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(-_gridSize, 0, (i * _gridSpacing)), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(_gridSize, 0, (-i * _gridSpacing)), _lineColor));
                vertexList.Add(new VertexPositionColor(new Vector3(-_gridSize, 0, (-i * _gridSpacing)), _lineColor));
            }

            // add highlights
            vertexList.Add(new VertexPositionColor(Vector3.Forward * _gridSize, _highlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Backward * _gridSize, _highlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Right * _gridSize, _highlightColor));
            vertexList.Add(new VertexPositionColor(Vector3.Left * _gridSize, _highlightColor));

            // add boundingbox
            var box = new BoundingBox(new Vector3(-_gridSize, -_gridSize, -_gridSize), new Vector3(_gridSize, _gridSize, _gridSize));
            Vector3[] corners = new Vector3[8];

            box.GetCorners(corners);
            vertexList.Add(new VertexPositionColor(corners[0], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[1], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[0], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[3], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[0], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[4], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[1], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[2], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[1], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[5], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[2], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[3], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[2], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[6], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[3], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[7], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[4], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[5], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[4], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[7], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[5], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[6], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[6], _lineColor));
            vertexList.Add(new VertexPositionColor(corners[7], _lineColor));

            _vertexData = vertexList.ToArray();
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            device.DepthStencilState = DepthStencilState.Default;
            _effect.View = Camera.Main.ViewMatrix;
            _effect.Projection = Camera.Main.ProjectionMatrix;
            _effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives(PrimitiveType.LineList, _vertexData, 0, _numberOfLines);
        }
    }
}
