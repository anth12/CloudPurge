using System;
using System.Threading.Tasks;

namespace Our.Umbraco.CloudPurge.Extensions
{
	internal static class TaskExtensions
	{
		public static T RunWithTimeoutSync<T>(this Task<T> func, TimeSpan timeout)
		{
			Task.Run(() => func).GetAwaiter().GetResult();

			var task = Task.Run(() => func.Result);
			try
			{
				Task.WaitAny(task, Task.Delay(timeout));

				if (!task.IsCompleted)
					throw new TimeoutException();

				return task.Result;
			}
			catch (AggregateException ex)
			{
				throw ex.InnerException;
			}
		}
    }
}
