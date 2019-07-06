using System;

namespace Gwen
{
    /// <summary>
    /// Represents inner spacing.
    /// </summary>
    public struct Padding : IEquatable<Padding>
    {
        public readonly int Top;
        public readonly int Bottom;
        public readonly int Left;
        public readonly int Right;

        // common values
        public static Padding Zero = new Padding(0);
        public static Padding One = new Padding(1);
        public static Padding Two = new Padding(2);
        public static Padding Three = new Padding(3);
        public static Padding Four = new Padding(4);
        public static Padding Five = new Padding(5);

        public Padding(int left, int top, int right, int bottom)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

		public Padding(int horizontal, int vertical)
		{
			Top = vertical;
			Bottom = vertical;
			Left = horizontal;
			Right = horizontal;
		}

		public Padding(int padding)
		{
			Top = padding;
			Bottom = padding;
			Left = padding;
			Right = padding;
		}

		public bool Equals(Padding other)
        {
            return other.Top == Top && other.Bottom == Bottom && other.Left == Left && other.Right == Right;
        }

        public static bool operator ==(Padding lhs, Padding rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Padding lhs, Padding rhs)
        {
            return !lhs.Equals(rhs);
        }

		public static Padding operator +(Padding lhs, Padding rhs)
		{
			return new Padding(lhs.Left + rhs.Left, lhs.Top + rhs.Top, lhs.Right + rhs.Right, lhs.Bottom + rhs.Bottom);
		}

		public static Padding operator -(Padding lhs, Padding rhs)
		{
			return new Padding(lhs.Left - rhs.Left, lhs.Top - rhs.Top, lhs.Right - rhs.Right, lhs.Bottom - rhs.Bottom);
		}

		public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Padding)) return false;
            return Equals((Padding) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Top;
                result = (result*397) ^ Bottom;
                result = (result*397) ^ Left;
                result = (result*397) ^ Right;
                return result;
            }
        }

		public static explicit operator Padding(Margin margin)
		{
			return new Padding(margin.Left, margin.Top, margin.Right, margin.Bottom);
		}

		public override string ToString()
		{
			return String.Format("Left = {0} Top = {1} Right = {2} Bottom = {3}", Left, Top, Right, Bottom);
		}
	}
}
