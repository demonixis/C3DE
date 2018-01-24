using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gwen
{
    /// <summary>
    /// Misc utility functions.
    /// </summary>
    public static class Util
    {
        public static int Ceil(float x)
        {
            return (int)Math.Ceiling(x);
        }

        public static Rectangle FloatRect(float x, float y, float w, float h)
        {
            return new Rectangle((int)x, (int)y, (int)w, (int)h);
        }

        public static int Clamp(int x, int min, int max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;
            return x;
        }

        public static float Clamp(float x, float min, float max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;
            return x;
        }

        public static Rectangle ClampRectToRect(Rectangle inside, Rectangle outside, bool clampSize = false)
        {
            if (inside.X < outside.X)
                inside.X = outside.X;

            if (inside.Y < outside.Y)
                inside.Y = outside.Y;

            if (inside.Right > outside.Right)
            {
                if (clampSize)
                    inside.Width = outside.Width;
                else
                    inside.X = outside.Right - inside.Width;
            }
            if (inside.Bottom > outside.Bottom)
            {
                if (clampSize)
                    inside.Height = outside.Height;
                else
                    inside.Y = outside.Bottom - inside.Height;
            }

            return inside;
        }

        public static HSV ToHSV(this Color color)
        {
			HSV hsv = new HSV();

			float r = (float)color.R / 255.0f;
			float g = (float)color.G / 255.0f;
			float b = (float)color.B / 255.0f;

			float max = Math.Max(r, Math.Max(g, b));
			float min = Math.Min(r, Math.Min(g, b));

			hsv.V = max;

			float delta = max - min;

			if (max != 0)
			{
				hsv.S = delta / max;
			}
			else
			{
				hsv.S = 0;
			}

			if (delta != 0)
			{
				if (r == max)
					hsv.H = (g - b) / delta;
				else if (g == max)
					hsv.H = 2 + (b - r) / delta;
				else
					hsv.H = 4 + (r - g) / delta;

				hsv.H *= 60;
				if (hsv.H < 0)
					hsv.H += 360;
			}
			else
			{
				hsv.H = 0;
			}

            return hsv;
        }

        public static Color HSVToColor(float h, float s, float v)
        {
            int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
            float f = h / 60 - (float)Math.Floor(h / 60);

            v = v * 255;
            int va = Convert.ToInt32(v);
            int p = Convert.ToInt32(v * (1 - s));
            int q = Convert.ToInt32(v * (1 - f * s));
            int t = Convert.ToInt32(v * (1 - (1 - f) * s));

            if (hi == 0)
                return new Color(255, va, t, p);
            if (hi == 1)
                return new Color(255, q, va, p);
            if (hi == 2)
                return new Color(255, p, va, t);
            if (hi == 3)
                return new Color(255, p, q, va);
            if (hi == 4)
                return new Color(255, t, p, va);
            return new Color(255, va, p, q);
        }

        // can't create extension operators
        public static Color Subtract(this Color color, Color other)
        {
            return new Color(color.A - other.A, color.R - other.R, color.G - other.G, color.B - other.B);
        }

        public static Color Add(this Color color, Color other)
        {
            return new Color(color.A + other.A, color.R + other.R, color.G + other.G, color.B + other.B);
        }

        public static Color Multiply(this Color color, float amount)
        {
            return new Color(color.A, (int)(color.R * amount), (int)(color.G * amount), (int)(color.B * amount));
        }

        public static Rectangle Add(this Rectangle r, Rectangle other)
        {
            return new Rectangle(r.X + other.X, r.Y + other.Y, r.Width + other.Width, r.Height + other.Height);
        }

        /// <summary>
        /// Splits a string but keeps the separators intact.
        /// </summary>
        /// <param name="text">String to split.</param>
        /// <param name="separators">Separator characters.</param>
        /// <returns>Split strings.</returns>
        public static string[] SplitAndKeep(string text, string separators)
        {
			List<string> strs = new List<string>();
			int offset = 0;
			int length = text.Length;
			int sepLen = separators.Length;
			int i = text.IndexOf(separators);
			string word;

			while (i != -1)
			{
				word = text.Substring(offset, i - offset);
				if (!String.IsNullOrWhiteSpace(word))
					strs.Add(word);
				offset = i + sepLen;
				i = text.IndexOf(separators, offset);
				offset -= sepLen;
			}

			strs.Add(text.Substring(offset, length - offset));

			return strs.ToArray();
        }

		public const int Ignore = -1;
		public static bool IsIgnore(int value)
		{
			return value == Ignore;
		}

		public const int Infinity = 0xfffffff;
		public static bool IsInfinity(int value)
		{
			return value > 0xffffff;
		}
    }
}
