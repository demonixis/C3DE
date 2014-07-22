namespace C3DE
{
    /// <summary>
    /// An utility class to safely debug this application.
    /// </summary>
	public class Debug
	{
		public static void Log(params object[] value)
		{
#if DEBUG
			for (int i = 0, l = value.Length; i < l; i++)
				System.Console.WriteLine(value.ToString());
#endif
		}
		
		public static void LogException(string value)
		{
#if DEBUG
			throw new System.Exception(value);
#endif
		}
	}
}