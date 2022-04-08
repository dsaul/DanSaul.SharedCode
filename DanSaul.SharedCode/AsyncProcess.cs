﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedCode
{
	// https://stackoverflow.com/a/39872058
	public static class AsyncProcess
	{
		public static async Task<int> StartProcess(
			string filename,
			IEnumerable<string> arguments,
			string? workingDirectory = null,
			int? timeout = null,
			TextWriter? outputTextWriter = null,
			TextWriter? errorTextWriter = null
			)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo()
			{
				CreateNoWindow = true,
				//Arguments = arguments,
				FileName = filename,
				RedirectStandardOutput = outputTextWriter != null,
				RedirectStandardError = errorTextWriter != null,
				UseShellExecute = false,
				WorkingDirectory = workingDirectory
			};

			foreach (string argument in arguments)
				startInfo.ArgumentList.Add(argument);


			//startInfo.ArgumentList.

			using Process? process = new Process();
			process.StartInfo = startInfo;


			var cancellationTokenSource = timeout.HasValue ?
				new CancellationTokenSource(timeout.Value) :
				new CancellationTokenSource();

			process.Start();

			List<Task> tasks = new List<Task>();

			tasks.Add(process.WaitForExitAsync(cancellationTokenSource.Token));


			if (outputTextWriter != null)
			{
				Task stdoutReaderTask = ReadAsync(
					(x) =>
					{
						process.OutputDataReceived += x;
						process.BeginOutputReadLine();
					},
					(x) =>
					{
						process.OutputDataReceived -= x;
					},
					outputTextWriter,
					cancellationTokenSource.Token
				);
				tasks.Add(stdoutReaderTask);
			}

			if (errorTextWriter != null)
			{
				Task stdErrReaderTask = ReadAsync(
					(x) =>
					{
						process.ErrorDataReceived += x;
						process.BeginErrorReadLine();
					},
					(x) =>
					{
						process.ErrorDataReceived -= x;
					},
					errorTextWriter,
					cancellationTokenSource.Token
				);
				tasks.Add(stdErrReaderTask);
			}

			await Task.WhenAll(tasks);

			return process.ExitCode;
		}

		public static async Task<int> StartProcess(
			string filename,
			string arguments,
			string? workingDirectory = null,
			int? timeout = null,
			TextWriter? outputTextWriter = null,
			TextWriter? errorTextWriter = null
			)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo()
			{
				CreateNoWindow = true,
				Arguments = arguments,
				FileName = filename,
				RedirectStandardOutput = outputTextWriter != null,
				RedirectStandardError = errorTextWriter != null,
				UseShellExecute = false,
				WorkingDirectory = workingDirectory
			};


			//startInfo.ArgumentList.

			using Process? process = new Process();
			process.StartInfo = startInfo;


			var cancellationTokenSource = timeout.HasValue ?
				new CancellationTokenSource(timeout.Value) :
				new CancellationTokenSource();

			process.Start();

			List<Task> tasks = new List<Task>();

			tasks.Add(process.WaitForExitAsync(cancellationTokenSource.Token));


			if (outputTextWriter != null)
			{
				Task stdoutReaderTask = ReadAsync(
					(x) =>
					{
						process.OutputDataReceived += x;
						process.BeginOutputReadLine();
					},
					(x) =>
					{
						process.OutputDataReceived -= x;
					},
					outputTextWriter,
					cancellationTokenSource.Token
				);
				tasks.Add(stdoutReaderTask);
			}

			if (errorTextWriter != null)
			{
				Task stdErrReaderTask = ReadAsync(
					(x) =>
					{
						process.ErrorDataReceived += x;
						process.BeginErrorReadLine();
					},
					(x) =>
					{
						process.ErrorDataReceived -= x;
					},
					errorTextWriter,
					cancellationTokenSource.Token
				);
				tasks.Add(stdErrReaderTask);
			}

			await Task.WhenAll(tasks);

			return process.ExitCode;
		}

		/// <summary>
		/// Waits asynchronously for the process to exit.
		/// </summary>
		/// <param name="process">The process to wait for cancellation.</param>
		/// <param name="cancellationToken">A cancellation token. If invoked, the task will return
		/// immediately as cancelled.</param>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task WaitForExitAsync(
			this Process process,
			CancellationToken cancellationToken = default
			)
		{
			process.EnableRaisingEvents = true;

			var taskCompletionSource = new TaskCompletionSource<object>();

			void ExitHandler(object? sender, EventArgs args)
			{
				process.Exited -= ExitHandler;
				taskCompletionSource.TrySetResult(null!);
			}

			process.Exited += ExitHandler;

			if (cancellationToken != default)
			{
				cancellationToken.Register(
					() =>
					{
						process.Exited -= ExitHandler;
						taskCompletionSource.TrySetCanceled();
					});
			}

			return taskCompletionSource.Task;
		}


		/// <summary>
		/// Reads the data from the specified data recieved event and writes it to the
		/// <paramref name="textWriter"/>.
		/// </summary>
		/// <param name="addHandler">Adds the event handler.</param>
		/// <param name="removeHandler">Removes the event handler.</param>
		/// <param name="textWriter">The text writer.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public static Task ReadAsync(
			Action<DataReceivedEventHandler> addHandler,
			Action<DataReceivedEventHandler> removeHandler,
			TextWriter textWriter,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var taskCompletionSource = new TaskCompletionSource<object>();

			DataReceivedEventHandler? handler = null;
			handler = new DataReceivedEventHandler(
				(sender, e) =>
				{
					if (e.Data == null)
					{
						removeHandler(handler!);
						taskCompletionSource.TrySetResult(null!);
					}
					else
					{
						textWriter.WriteLine(e.Data);
					}
				});

			addHandler(handler);

			if (cancellationToken != default)
			{
				cancellationToken.Register(
					() =>
					{
						removeHandler(handler);
						taskCompletionSource.TrySetCanceled();
					});
			}

			return taskCompletionSource.Task;
		}
	}
}
