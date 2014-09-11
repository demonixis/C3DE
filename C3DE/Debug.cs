namespace C3DE
{
    /// <summary>
    /// An utility class to safely debug this application. Methods works in debug mode,
    /// when you switch to release they do nothing.
    /// </summary>
	public class Debug
	{
        /// <summary>
        /// Log one or more messages.
        /// </summary>
        /// <param name="value">A single message or an array of messages.</param>
		public static void Log(params object[] value)
		{
#if DEBUG
			for (int i = 0, l = value.Length; i < l; i++)
				System.Console.WriteLine(value[i].ToString());
#endif
		}
		
        /// <summary>
        /// Log an exception with a trace message.
        /// </summary>
        /// <param name="value">The message to display.</param>
		public static void LogException(string value)
		{
#if DEBUG
			throw new System.Exception(value);
#endif
		}
	}
}