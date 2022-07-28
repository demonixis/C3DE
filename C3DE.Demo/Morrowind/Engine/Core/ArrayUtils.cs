using System;
using System.Collections.Generic;

public static class ArrayUtils
{
    public static T Last<T>(T[] array)
    {
        return array[array.Length - 1];
    }

    public static T Last<T>(List<T> list)
    {
        return list[list.Count - 1];
    }

    /// <summary>
    /// Calculates the minimum and maximum values of an array.
    /// </summary>
    public static void GetExtrema(float[] array, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;

        foreach (var element in array)
        {
            min = Math.Min(min, element);
            max = Math.Max(max, element);
        }
    }

    /// <summary>
    /// Calculates the minimum and maximum values of a 2D array.
    /// </summary>
    public static void GetExtrema(float[,] array, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;

        foreach (var element in array)
        {
            min = Math.Min(min, element);
            max = Math.Max(max, element);
        }
    }

    /// <summary>
    /// Calculates the minimum and maximum values of a 3D array.
    /// </summary>
    public static void GetExtrema(float[,,] array, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;

        foreach (var element in array)
        {
            min = Math.Min(min, element);
            max = Math.Max(max, element);
        }
    }

    public static void Flip2DArrayVertically<T>(T[] arr, int rowCount, int columnCount)
    {
        Flip2DSubArrayVertically(arr, 0, rowCount, columnCount);
    }

    /// <summary>
    /// Flips a portion of a 2D array vertically.
    /// </summary>
    /// <param name="arr">A 2D array represented as a 1D row-major array.</param>
    /// <param name="startIndex">The 1D index of the top left element in the portion of the 2D array we want to flip.</param>
    /// <param name="rowCount">The number of rows in the sub-array.</param>
    /// <param name="columnCount">The number of columns in the sub-array.</param>
    public static void Flip2DSubArrayVertically<T>(T[] arr, int startIndex, int rowCount, int columnCount)
    {
        var tmpRow = new T[columnCount];
        var lastRowIndex = rowCount - 1;

        for (int rowIndex = 0; rowIndex < (rowCount / 2); rowIndex++)
        {
            var otherRowIndex = lastRowIndex - rowIndex;

            var rowStartIndex = startIndex + (rowIndex * columnCount);
            var otherRowStartIndex = startIndex + (otherRowIndex * columnCount);

            Array.Copy(arr, otherRowStartIndex, tmpRow, 0, columnCount); // other -> tmp
            Array.Copy(arr, rowStartIndex, arr, otherRowStartIndex, columnCount); // row -> other
            Array.Copy(tmpRow, 0, arr, rowStartIndex, columnCount); // tmp -> row
        }
    }
}