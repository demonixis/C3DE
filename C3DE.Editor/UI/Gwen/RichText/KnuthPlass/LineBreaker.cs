using System;
using System.Collections.Generic;
using System.Text;

namespace Gwen.RichText.KnuthPlass
{
	// Knuth and Plass line breaking algorithm
	//
	// Original JavaScript implementation by Bram Stein
	// from https://github.com/bramstein/typeset
	// licensed under the new BSD License.
	internal class LineBreaker : RichText.LineBreaker
	{
		public const int Infinity = 10000;

		public const int DemeritsLine = 10;
		public const int DemeritsFlagged = 100;
		public const int DemeritsFitness = 3000;

		private Paragraph m_Paragraph;
		private int m_TotalWidth;
		private int m_Tolerance;

		private List<Node> m_Nodes;

		private Sum m_Sum = new Sum(0, 0, 0);

		private LinkedList<BreakPoint> m_ActiveNodes = new LinkedList<BreakPoint>();

		private Formatter m_Formatter;

		public LineBreaker(Renderer.RendererBase renderer, Font defaultFont)
			: base(renderer, defaultFont)
		{
			m_Formatter = new LeftFormatter(renderer, defaultFont);
		}

		public override List<TextBlock> LineBreak(Paragraph paragraph, int width)
		{
			List<TextBlock> textBlocks = null;

			// Todo: Find out why tolerance needs to be quite high sometimes, depending on the line width.
			// Maybe words need to be hyphenated or there is still a bug somewhere in the code.
			for (int tolerance = 4; tolerance < 30; tolerance += 2)
			{
				textBlocks = DoLineBreak(paragraph, m_Formatter, width, tolerance);
				if (textBlocks != null)
				{
					break;
				}
			}

			return textBlocks;
		}

		private int GetLineLength(int currentLine)
		{
			return m_TotalWidth - m_Paragraph.Margin.Left - m_Paragraph.Margin.Right - (currentLine == 1 ? m_Paragraph.FirstIndent : m_Paragraph.RemainigIndent);
		}

		private float ComputeCost(int start, int end, Sum activeTotals, int currentLine)
		{
			int width = m_Sum.Width - activeTotals.Width;
			int stretch = 0;
			int shrink = 0;

			int lineLength = GetLineLength(currentLine);

			if (m_Nodes[end].Type == NodeType.Penalty)
			{
				width += m_Nodes[end].Width;
			}

			if (width < lineLength)
			{
				stretch = m_Sum.Stretch - activeTotals.Stretch;

				if (stretch > 0)
				{
					return (float)(lineLength - width) / stretch;
				}
				else
				{
					return Infinity;
				}
			}
			else if (width > lineLength)
			{
				shrink = m_Sum.Shrink - activeTotals.Shrink;

				if (shrink > 0)
				{
					return (float)(lineLength - width) / shrink;
				}
				else
				{
					return Infinity;
				}
			}
			else
			{
				return 0.0f;
			}
		}

		private Sum ComputeSum(int breakPointIndex)
		{
			Sum result = new Sum(m_Sum.Width, m_Sum.Stretch, m_Sum.Shrink);
			
			for (int i = breakPointIndex; i < m_Nodes.Count; i++)
			{
				if (m_Nodes[i].Type == NodeType.Glue)
				{
					result.Width += m_Nodes[i].Width;
					result.Stretch += ((GlueNode)m_Nodes[i]).Stretch;
					result.Shrink += ((GlueNode)m_Nodes[i]).Shrink;
				}
				else if (m_Nodes[i].Type == NodeType.Box || (m_Nodes[i].Type == NodeType.Penalty && ((PenaltyNode)m_Nodes[i]).Penalty == -Infinity && i > breakPointIndex))
				{
					break;
				}
			}

			return result;
		}

		private void MainLoop(int index)
		{
			Node node = m_Nodes[index];

			LinkedListNode<BreakPoint> active = m_ActiveNodes.First;
			LinkedListNode<BreakPoint> next = null;
			float ratio = 0.0f;
			int demerits = 0;
			Candidate[] candidates = new Candidate[4];
			int badness;
			int currentLine = 0;
			Sum tmpSum;
			int currentClass = 0;
			int fitnessClass;
			Candidate candidate;
			LinkedListNode<BreakPoint> newNode;

			while (active != null)
			{
				candidates[0].Demerits = Infinity;
				candidates[1].Demerits = Infinity;
				candidates[2].Demerits = Infinity;
				candidates[3].Demerits = Infinity;

				while (active != null)
				{
					next = active.Next;
					currentLine = active.Value.Line + 1;
					ratio = ComputeCost(active.Value.Position, index, active.Value.Totals, currentLine);

					if (ratio < -1 || (node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty == -Infinity))
					{
						m_ActiveNodes.Remove(active);
					}

					if (-1 <= ratio && ratio <= m_Tolerance)
					{
						badness = (int)(100.0f * Math.Pow(Math.Abs(ratio), 3));

						if (node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty >= 0)
							demerits = (DemeritsLine + badness) * (DemeritsLine + badness) + ((PenaltyNode)node).Penalty * ((PenaltyNode)node).Penalty;
						else if (node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty != -Infinity)
							demerits = (DemeritsLine + badness) * (DemeritsLine + badness) - ((PenaltyNode)node).Penalty * ((PenaltyNode)node).Penalty;
						else
							demerits = (DemeritsLine + badness) * (DemeritsLine + badness);

						if (node.Type == NodeType.Penalty && m_Nodes[active.Value.Position].Type == NodeType.Penalty)
							demerits += DemeritsFlagged * ((PenaltyNode)node).Flagged * ((PenaltyNode)m_Nodes[active.Value.Position]).Flagged;

						if (ratio < -0.5f)
							currentClass = 0;
						else if (ratio <= 0.5f)
							currentClass = 1;
						else if (ratio <= 1.0f)
							currentClass = 2;
						else
							currentClass = 3;

						if (Math.Abs(currentClass - active.Value.FitnessClass) > 1)
							demerits += DemeritsFitness;

						demerits += active.Value.Demerits;

						if (demerits < candidates[currentClass].Demerits)
						{
							candidates[currentClass].Active = active;
							candidates[currentClass].Demerits = demerits;
							candidates[currentClass].Ratio = ratio;
						}
					}

					active = next;

					if (active != null && active.Value.Line >= currentLine)
						break;
				}

				tmpSum = ComputeSum(index);

				for (fitnessClass = 0; fitnessClass < candidates.Length; fitnessClass++)
				{
					candidate = candidates[fitnessClass];

					if (candidate.Demerits < Infinity)
					{
						newNode = new LinkedListNode<BreakPoint>(new BreakPoint(index, candidate.Demerits, candidate.Ratio, candidate.Active.Value.Line + 1, fitnessClass, tmpSum, candidate.Active));
						if (active != null)
							m_ActiveNodes.AddBefore(active, newNode);
						else
							m_ActiveNodes.AddLast(newNode);
					}
				}
			}
		}

		private List<TextBlock> DoLineBreak(Paragraph paragraph, Formatter formatter, int width, int tolerance)
		{
			m_Paragraph = paragraph;
			m_TotalWidth = width;
			m_Tolerance = tolerance;

			m_Nodes = formatter.FormatParagraph(paragraph);

			m_Sum = new Sum(0, 0, 0);

			m_ActiveNodes.Clear();
			m_ActiveNodes.AddLast(new BreakPoint(0, 0, 0, 0, 0, new Sum(0, 0, 0), null));

			for (int index = 0; index < m_Nodes.Count; index++)
			{
				Node node = m_Nodes[index];

				if (node.Type == NodeType.Box)
				{
					m_Sum.Width += node.Width;
				}
				else if (node.Type == NodeType.Glue)
				{
					if (index > 0 && m_Nodes[index - 1].Type == NodeType.Box)
					{
						MainLoop(index);
					}
					m_Sum.Width += node.Width;
					m_Sum.Stretch += ((GlueNode)node).Stretch;
					m_Sum.Shrink += ((GlueNode)node).Shrink;
				}
				else if (node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty != Infinity)
				{
					MainLoop(index);
				}
			}

			if (m_ActiveNodes.Count != 0)
			{
				LinkedListNode<BreakPoint> node = m_ActiveNodes.First;
				LinkedListNode<BreakPoint> tmp = null;
				while (node != null)
				{
					if (tmp == null || node.Value.Demerits < tmp.Value.Demerits)
					{
						tmp = node;
					}

					node = node.Next;
				}

				List<Break> breaks = new List<Break>();

				while (tmp != null)
				{
					breaks.Add(new Break(tmp.Value.Position, tmp.Value.Ratio));
					tmp = tmp.Value.Previous;
				}

				// breaks.Reverse();

				int lineStart = 0;
				int y = 0;
				int x = 0;
				StringBuilder str = new StringBuilder(1000);
				List<TextBlock> textBlocks = new List<TextBlock>();

				for (int i = breaks.Count - 2; i >= 0; i--)
				{
					int position = breaks[i].Position;
					float r = breaks[i].Ratio;

					for (int j = lineStart; j < m_Nodes.Count; j++)
					{
						if (m_Nodes[j].Type == NodeType.Box || (m_Nodes[j].Type == NodeType.Penalty && ((PenaltyNode)m_Nodes[j]).Penalty == -Infinity))
						{
							lineStart = j;
							break;
						}
					}

					int height = 0;
					int baseline = 0;
					for (int nodeIndex = lineStart; nodeIndex <= position; nodeIndex++)
					{
						if (m_Nodes[nodeIndex].Type == NodeType.Box)
						{
							height = Math.Max(height, ((BoxNode)m_Nodes[nodeIndex]).Height);
							baseline = Math.Max(baseline, (int)((TextPart)((BoxNode)m_Nodes[nodeIndex]).Part).Font.FontMetrics.Baseline);
						}
					}

					Part part = ((BoxNode)m_Nodes[lineStart]).Part;
					int blockStart = lineStart;
					for (int nodeIndex = lineStart; nodeIndex <= position; nodeIndex++)
					{
						if ((m_Nodes[nodeIndex].Type == NodeType.Box && ((BoxNode)m_Nodes[nodeIndex]).Part != part) || nodeIndex == position)
						{
							TextBlock textBlock = new TextBlock();
							textBlock.Part = part;
							str.Clear();

							for (int k = blockStart; k < (nodeIndex - 1); k++)
							{
								if (m_Nodes[k].Type == NodeType.Glue)
								{
									if (m_Nodes[k].Width > 0)
										str.Append(' ');
								}
								else if (m_Nodes[k].Type == NodeType.Box)
								{
									str.Append(((BoxNode)m_Nodes[k]).Value);
								}
							}

							textBlock.Position = new Point(x, y + baseline - (int)((TextPart)part).Font.FontMetrics.Baseline);
							textBlock.Text = str.ToString();
							textBlock.Size = new Size(formatter.MeasureText(((TextPart)part).Font, textBlock.Text).Width, height);

							x += textBlock.Size.Width;

							textBlocks.Add(textBlock);

							if (m_Nodes[nodeIndex].Type == NodeType.Box)
								part = ((BoxNode)m_Nodes[nodeIndex]).Part;
							blockStart = nodeIndex;
						}
					}

					x = 0;
					y += height;

					lineStart = position;
				}

				return textBlocks;
			}

			return null;
		}

		private struct Candidate
		{
			public LinkedListNode<BreakPoint> Active;
			public int Demerits;
			public float Ratio;

#if DEBUG
			public override string ToString()
			{
				return String.Format("Candidate: Demerits = {0} Ratio = {1} Active = {2}", Demerits, Ratio, Active.Value.ToString());
			}
#endif
		}

		private struct Break
		{
			public int Position;
			public float Ratio;

			public Break(int position, float ratio)
			{
				Position = position;
				Ratio = ratio;
			}
		}

		private struct Sum
		{
			public int Width;
			public int Stretch;
			public int Shrink;

			public Sum(int width, int stretch, int shrink)
			{
				Width = width;
				Stretch = stretch;
				Shrink = shrink;
			}

#if DEBUG
			public override string ToString()
			{
				return String.Format("Sum: Width = {0} Stretch = {1} Shrink = {2}", Width, Stretch, Shrink);
			}
#endif
		}

		private struct BreakPoint
		{
			public int Position;
			public int Demerits;
			public float Ratio;
			public int Line;
			public int FitnessClass;
			public Sum Totals;
			public LinkedListNode<BreakPoint> Previous;

			public BreakPoint(int position, int demerits, float ratio, int line, int fitnessClass, Sum totals, LinkedListNode<BreakPoint> previous)
			{
				Position = position;
				Demerits = demerits;
				Ratio = ratio;
				Line = line;
				FitnessClass = fitnessClass;
				Totals = totals;
				Previous = previous;
			}

#if DEBUG
			public override string ToString()
			{
				return String.Format("BreakPoint: Position = {0} Demerits = {1} Ratio = {2} Line = {3} FitnessClass = {4} Totals = {{{5}}} Previous = {{{6}}}", Position, Demerits, Ratio, Line, FitnessClass, Totals.ToString(), Previous != null ? Previous.Value.ToString() : "Null");
			}
#endif
		}
	}
}
