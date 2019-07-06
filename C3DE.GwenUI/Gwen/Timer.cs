using System;
using System.Collections.Generic;

namespace Gwen
{
	public delegate void ElapsedEventHandler(object sender, EventArgs args);

	/// <summary>
	/// Render based timer.
	/// </summary>
	/// <remarks>
	/// This is not very accurate timer because it depends on render events.
	/// </remarks>
	public class Timer : IDisposable
	{
		private int m_Interval;
		private bool m_OneTime;
		private bool m_Enabled;

		private int m_TimerValue;

		/// <summary>
		/// Invoked when the timeout occurs.
		/// </summary>
		public event ElapsedEventHandler Elapsed;

		/// <summary>
		/// Timer interval in milliseconds.
		/// </summary>
		public int Interval { get { return m_Interval; } set { m_Interval = value; } }

		/// <summary>
		/// If true, timer is disabled when timeout occurs.
		/// </summary>
		public bool IsOneTime { get { return m_OneTime; } set { m_OneTime = value; } }

		/// <summary>
		/// Is timer enabled.
		/// </summary>
		public bool IsEnabled { get { return m_Enabled; } set { if (value) Start(); else Stop(); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Timer"/> class.
		/// </summary>
		public Timer()
		{
			m_Interval = 0;
			m_OneTime = false;
			m_Enabled = false;

			m_Timers.Add(this);
		}

		/// <summary>
		/// Start the timer.
		/// </summary>
		public void Start()
		{
			m_Enabled = true;
			m_TimerValue = m_Interval;
		}

		/// <summary>
		/// Stop the timer.
		/// </summary>
		public void Stop()
		{
			m_Enabled = false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_Timers.Remove(this);
			}
		}

		private static List<Timer> m_Timers = new List<Timer>();
		private static float m_LastTime;
		private static bool m_Started;

		internal static void Tick()
		{
			if (m_Timers.Count == 0)
				return;

			float currentTime = Platform.Platform.GetTimeInSeconds();

			if (!m_Started)
			{
				m_LastTime = currentTime;
				m_Started = true;
				return;
			}

			int diff = (int)((currentTime - m_LastTime) * 1000.0f);

			foreach (Timer timer in m_Timers)
			{
				if (timer.m_Enabled)
				{
					timer.m_TimerValue -= diff;
					if (timer.m_TimerValue <= 0)
					{
						if (timer.Elapsed != null)
							timer.Elapsed(timer, EventArgs.Empty);

						if (!timer.m_OneTime)
						{
							timer.m_TimerValue = timer.m_Interval + timer.m_TimerValue;
						}
						else
						{
							timer.m_Enabled = false;
						}
					}
				}
			}

			m_LastTime = currentTime;
		}
	}
}
