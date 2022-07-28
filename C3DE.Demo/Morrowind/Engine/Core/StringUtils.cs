public static class StringUtils
{
    /// <summary>
    /// Quickly checks if an ASCII encoded string is equal to a C# string.
    /// </summary>
    public static bool Equals(byte[] ASCIIBytes, string str)
    {
        if (ASCIIBytes.Length != str.Length)
        {
            return false;
        }

        for (int i = 0; i < ASCIIBytes.Length; i++)
        {
            if (ASCIIBytes[i] != str[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool StartWith(byte[] ASCIIBytes, string str)
    {
        if (ASCIIBytes.Length < str.Length)
        {
            return false;
        }

        for (var i = 0; i < str.Length; i++)
        {
            if (ASCIIBytes[i] != str[i])
            {
                return false;
            }
        }

        return true;
    }
}