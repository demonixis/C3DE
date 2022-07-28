using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TES3Unity
{
    public static class Convert
    {
        public const int YardInMWUnits = 64;
        public const float MeterInYards = 1.09361f;
        public const float MeterInMWUnits = MeterInYards * YardInMWUnits;

        public const int ExteriorCellSideLengthInMWUnits = 8192;
        public const float ExteriorCellSideLengthInMeters = ExteriorCellSideLengthInMWUnits / MeterInMWUnits;

        private static StringBuilder WordsBuilder = new StringBuilder();
        private static string[] Temp = null;
        private const string RegexTerm = @"(?<!^)(?=[A-Z])";


        public static Quaternion RotationMatrixToQuaternion(Matrix matrix)
        {
            return Quaternion.Identity;
            // FIXME
            //return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        public static string CharToString(char[] array)
        {
            var list = new List<char>();

            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] != '\0')
                {
                    list.Add(array[i]);
                }
            }

            return new string(list.ToArray());
        }

        public static string RemoveNullChar(string str)
        {
            var list = new List<char>();

            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != '\0')
                {
                    list.Add(str[i]);
                }
            }

            return new string(list.ToArray());
        }

        public static string NormalizeFromEnum(string input)
        {
            WordsBuilder.Length = 0;

            Temp = Regex.Split(input, RegexTerm);

            var length = Temp.Length;

            for (var i = 0; i < length; i++)
            {
                if (Temp[i].Length > 1)
                {
                    WordsBuilder.Append($"{Temp[i]} ");
                }
                else
                {
                    WordsBuilder.Append(Temp[i]);

                    if (i < length - 1 && Temp[i + 1].Length > 1)
                    {
                        WordsBuilder.Append(" ");
                    }
                }
            }

            return WordsBuilder.ToString().TrimEnd();
        }

    }
}