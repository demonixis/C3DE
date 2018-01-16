using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE
{
    public class GridNode
    {
        private int m_X;
        private int m_Y;
        private float m_Weight;

        public bool IsWall => m_Weight == 0;

        public GridNode(int x, int y, float weight)
        {
            m_X = x;
            m_Y = y;
            m_Weight = weight;
        }

        public float GetCost(GridNode fromNeighbor = null)
        {
            if (fromNeighbor != null && fromNeighbor.m_X != m_X && fromNeighbor.m_Y != m_Y)
                return m_Weight * 1.41421f;

            return m_Weight;
        }
    }

    public class BinaryHeap
    {
        private List<GridNode> m_Content;
        private ScoreFunc m_ScoreFunction;

        public delegate int ScoreFunc(GridNode node);

        public BinaryHeap(ScoreFunc scoreFunc)
        {
            m_Content = new List<GridNode>();
            m_ScoreFunction = scoreFunc;
        }

        public void Push(GridNode node)
        {
            m_Content.Add(node);
            SinkDown(m_Content.Count - 1);
        }

        public GridNode Pop()
        {
            var result = m_Content[0];

            var index = m_Content.Count - 1;
            var end = m_Content[index];
            m_Content.RemoveAt(index);

            if (m_Content.Count > 0)
            {
                m_Content[0] = end;
                BubbleUp(0);
            }

            return result;
        }

        public void Remove(GridNode node)
        {
            var i = m_Content.IndexOf(node);
            var index = m_Content.Count - 1;
            var end = m_Content[index];
            m_Content.RemoveAt(index);

            if (i != m_Content.Count - 1)
            {
                m_Content[i] = end;

                if (m_ScoreFunction(end) < m_ScoreFunction(node))
                    SinkDown(i);
                else
                    BubbleUp(i);
            }
        }

        public int Size => m_Content.Count;

        public void RescoreEmement(GridNode node) => SinkDown(m_Content.IndexOf(node));

        public void SinkDown(int index)
        {
            var element = m_Content[index];

            while (index > 0)
            {
                var parentN = ((index + 1) >> 1) - 1;
                var parent = m_Content[parentN];

                if (m_ScoreFunction(element) < m_ScoreFunction(parent))
                {
                    m_Content[parentN] = element;
                    m_Content[index] = parent;
                    index = parentN;
                }
                else
                    break;
            }
        }

        public void BubbleUp(int n)
        {
            // Look up the target element and its score.
            var length = m_Content.Count;
            var element = m_Content[n];
            var elemScore = m_ScoreFunction(element);
            var running = true;

            while (running)
            {
                var child2N = (n + 1) << 1;
                var child1N = child2N - 1;
                var swap = -1;
                var child1Score = 0;

                if (child1N < length)
                {
                    var child1 = m_Content[child1N];
                    child1Score = m_ScoreFunction(child1);

                    if (child1Score < elemScore)
                    {
                        swap = child1N;
                    }
                }

                if (child2N < length)
                {
                    var child2 = m_Content[child2N];
                    var child2Score = m_ScoreFunction(child2);
                    if (child2Score < (swap == -1 ? elemScore : child1Score))
                    {
                        swap = child2N;
                    }
                }

                if (swap != -1)
                {
                    m_Content[n] = m_Content[swap];
                    m_Content[swap] = element;
                    n = swap;
                }
                else
                    running = false;
            }
        }
    }

    public class AStar
    {
    }
}
