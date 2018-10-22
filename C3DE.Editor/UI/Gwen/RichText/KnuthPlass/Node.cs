using System;

namespace Gwen.RichText.KnuthPlass
{
	internal enum NodeType { Box, Glue, Penalty }

	internal abstract class Node
	{
		private NodeType m_Type;
		private int m_Width;

		public NodeType Type { get { return m_Type; } }
		public int Width { get { return m_Width; } }

		public Node(NodeType type, int width)
		{
			m_Type = type;
			m_Width = width;
		}
	}

	internal class BoxNode : Node
	{
		private string m_Value;
		private int m_Height;
		private Part m_Part;

		public string Value { get { return m_Value; } }
		public int Height { get { return m_Height; } }
		public Part Part { get { return m_Part; } }

		public BoxNode(int width, string value, Part part, int height)
			: base (NodeType.Box, width)
		{
			m_Value = value;
			m_Height = height;
			m_Part = part;
		}

#if DEBUG
		public override string ToString()
		{
			return String.Format("Box: Width = {0} Value = {1}", Width, Value);
		}
#endif
	}

	internal class GlueNode : Node
	{
		private int m_Stretch;
		private int m_Shrink;

		public int Stretch { get { return m_Stretch; } }
		public int Shrink { get { return m_Shrink; } }

		public GlueNode(int width, int stretch, int shrink)
			: base(NodeType.Glue, width)
		{
			m_Stretch = stretch;
			m_Shrink = shrink;
		}

#if DEBUG
		public override string ToString()
		{
			return String.Format("Glue: Width = {0} Stretch = {1} Shrink = {2}", Width, Stretch, Shrink);
		}
#endif
	}

	internal class PenaltyNode : Node
	{
		private int m_Penalty;
		private int m_Flagged;

		public int Penalty { get { return m_Penalty; } }
		public int Flagged { get { return m_Flagged; } }

		public PenaltyNode(int width, int penalty, int flagged)
			: base(NodeType.Penalty, width)
		{
			m_Penalty = penalty;
			m_Flagged = flagged;
		}

#if DEBUG
		public override string ToString()
		{
			return String.Format("Penalty: Width = {0} Penalty = {1} Flagged = {2}", Width, Penalty, Flagged);
		}
#endif
	}
}
