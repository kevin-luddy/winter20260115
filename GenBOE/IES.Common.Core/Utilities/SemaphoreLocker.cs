namespace IES.Common.Core.Utilities
{
	public class SemaphoreLocker : IDisposable
	{
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private bool disposedValue;

		public async Task LockAsync(Func<Task> worker)
		{
			bool isTaken = false;
			try
			{
				do
				{
					try
					{
					}
					finally
					{
						isTaken = await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
					}
				}
				while (!isTaken);
				await worker();
			}
			finally
			{
				if (isTaken)
				{
					_semaphore.Release();
				}
			}
		}

		// overloading variant for non-void methods with return type (generic T)
		public async Task<T> LockAsync<T>(Func<Task<T>> worker)
		{
			bool isTaken = false;
			try
			{
				do
				{
					try
					{
					}
					finally
					{
						isTaken = await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
					}
				}
				while (!isTaken);
				return await worker();
			}
			finally
			{
				if (isTaken)
				{
					_semaphore.Release();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
					this._semaphore.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~SemaphoreLocker()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}