using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace C3DE.Navigation
{
    /// <summary>
    /// AStar Algorithm adapted from https://github.com/davecusatis/A-Star-Sharp/blob/master/Astar.cs
    /// </summary>
    public class AStar
    {
        private Node[,] _grid;

        public int GridRows => _grid.GetLength(1);
        public int GridCols => _grid.GetLength(0);

        public AStar(Node[,] grid)
        {
            _grid = grid;
        }

        public Stack<Node> FindPath(float sx, float sy, float ex, float ey)
        {
            var start = new Node(new Vector2((int)(sx), (int)(sy)), true);
            var end = new Node(new Vector2((int)(ex), (int)(ey)), true);

            var path = new Stack<Node>();
            var openList = new List<Node>();
            var closedList = new List<Node>();
            var adjacencies = new List<Node>();
            var current = start;

            // add start node to Open List
            openList.Add(start);

            while (openList.Count != 0 && !closedList.Exists(x => x.Position == end.Position))
            {
                current = openList[0];
                openList.Remove(current);
                closedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach (var node in adjacencies)
                {
                    if (!closedList.Contains(node) && node.Walkable)
                    {
                        if (!openList.Contains(node))
                        {
                            node.Parent = current;
                            node.DistanceToTarget = Math.Abs(node.Position.X - end.Position.X) + Math.Abs(node.Position.Y - end.Position.Y);
                            node.Cost = 1 + node.Parent.Cost;
                            openList.Add(node);
                            openList = openList.OrderBy(n => n.F).ToList<Node>();
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!closedList.Exists(x => x.Position == end.Position))
                return null;

            // if all good, return path
            var index = closedList.IndexOf(current);

            if (index > -1)
            {
                var temp = closedList[index];
                while (temp != null && temp.Parent != start)
                {
                    path.Push(temp);
                    temp = temp.Parent;
                }
            }

            return path;
        }

        private List<Node> GetAdjacentNodes(Node node)
        {
            var nodes = new List<Node>();
            var row = (int)node.Position.Y;
            var col = (int)node.Position.X;

            if (row + 1 < GridRows)
                nodes.Add(_grid[col, row + 1]);

            if (row - 1 >= 0)
                nodes.Add(_grid[col, row - 1]);

            if (col - 1 >= 0)
                nodes.Add(_grid[col - 1, row]);

            if (col + 1 < GridCols)
                nodes.Add(_grid[col + 1, row]);

            return nodes;
        }
    }

    public class Node
    {
        public Node Parent;
        public Vector2 Position;
        public float DistanceToTarget;
        public float Cost;
        public bool Walkable;

        public Vector2 Center => new Vector2(Position.X, Position.Y);

        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                return -1;
            }
        }

        public Node(Vector2 position, bool walkable)
        {
            Parent = null;
            Position = position;
            DistanceToTarget = -1;
            Cost = 1;
            Walkable = walkable;
        }
    }
}