using DinkToPdf;
using DinkToPdf.Contracts;
using System.Collections.Concurrent;

namespace orch.report.libwkhtmltox
{
	internal class STASynchronizedConverter : BasicConverter
	{
		private Thread? conversionThread;

		private readonly BlockingCollection<Task> conversions = new();

		private bool kill = false;

		private readonly object startLock = new();

		public STASynchronizedConverter(ITools tools) : base(tools)
		{
		}

		public override byte[] Convert(IDocument document)
		{
			return Invoke(() => base.Convert(document));
		}

		public TResult Invoke<TResult>(Func<TResult> @delegate)
		{
			StartThread();

			Task<TResult> task = new(@delegate);

			lock (task)
			{
				conversions.Add(task);
				Monitor.Wait(task);
			}
			if (task.Exception != null)
			{
				throw task.Exception;
			}

			return task.Result;
		}

		private void StartThread()
		{
			lock (startLock)
			{
				if (conversionThread == null)
				{
					conversionThread = new Thread(Run)
					{
						IsBackground = true,
						Name = "wkhtmltopdf worker thread"
					};
#if WINDOWS
					conversionThread.SetApartmentState(ApartmentState.STA);
#endif

					kill = false;

					conversionThread.Start();
				}
			}
		}

		private void Run()
		{
			while (!kill)
			{
				Task task = conversions.Take();

				lock (task)
				{
					task.RunSynchronously();

					Monitor.Pulse(task);
				}
			}
		}
	}
}