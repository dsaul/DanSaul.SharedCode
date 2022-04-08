using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class RecurringTask
	{
		public static void Run(Action action, int milliseconds, CancellationToken token)
		{
			if (action == null)
				return;
			Task.Run(async () => {
				while (!token.IsCancellationRequested)
				{
					action();
					await Task.Delay(TimeSpan.FromMilliseconds(milliseconds), token);
				}
			}, token);
		}
	}
}
