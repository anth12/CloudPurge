using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.CloudPurge.Utilities
{
	internal static class IEnumerableExtensions
	{
		public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int batchSize, Func<IEnumerable<TSource>, TResult> batchProcessor)
		{
			var batchCounter = 0;
			var batches = source.GroupBy(u => batchCounter++ / batchSize).ToArray();

			return batches.Select(batchProcessor.Invoke);
		}
	}
}
