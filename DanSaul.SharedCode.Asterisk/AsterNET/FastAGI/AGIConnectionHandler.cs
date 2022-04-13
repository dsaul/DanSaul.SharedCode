
using System;
using System.IO;
using System.Threading;
using AsterNET.FastAGI.Command;
using AsterNET.IO;
using Serilog;

namespace AsterNET.FastAGI
{
	/// <summary>
	///     An AGIConnectionHandler is created and run by the AGIServer whenever a new
	///     socket connection from an Asterisk Server is received.<br />
	///     It reads the request using an AGIReader and runs the AGIScript configured to
	///     handle this type of request. Finally it closes the socket connection.
	/// </summary>
	public class AGIConnectionHandler
	{
		private static readonly LocalDataStoreSlot _channel = Thread.AllocateDataSlot();
		private readonly SocketConnection socket;
		private readonly IMappingStrategy mappingStrategy;
		private readonly bool _SC511_CAUSES_EXCEPTION;
		private readonly bool _SCHANGUP_CAUSES_EXCEPTION;

		#region Channel

		/// <summary>
		///     Returns the AGIChannel associated with the current thread.
		/// </summary>
		/// <returns>the AGIChannel associated with the current thread or  null if none is associated.</returns>
		internal static AGIChannel? Channel
		{
			get { return (AGIChannel?) Thread.GetData(_channel); }
		}

		#endregion

		#region AGIConnectionHandler(socket, mappingStrategy)

		/// <summary>
		///     Creates a new AGIConnectionHandler to handle the given socket connection.
		/// </summary>
		/// <param name="socket">the socket connection to handle.</param>
		/// <param name="mappingStrategy">the strategy to use to determine which script to run.</param>
		public AGIConnectionHandler(SocketConnection socket, IMappingStrategy mappingStrategy,
			bool SC511_CAUSES_EXCEPTION, bool SCHANGUP_CAUSES_EXCEPTION)
		{
			this.socket = socket;
			this.mappingStrategy = mappingStrategy;
			_SC511_CAUSES_EXCEPTION = SC511_CAUSES_EXCEPTION;
			_SCHANGUP_CAUSES_EXCEPTION = SCHANGUP_CAUSES_EXCEPTION;
		}

		#endregion

		public void Run()
		{
			try
			{
				var reader = new AGIReader(socket);
				var writer = new AGIWriter(socket);
				AGIRequest request = reader.ReadRequest();

				//Added check for when the request is empty
				//eg. telnet to the service 
				if (request.Request.Count > 0)
				{
					var channel = new AGIChannel(writer, reader, _SC511_CAUSES_EXCEPTION, _SCHANGUP_CAUSES_EXCEPTION);
					AGIScript script = mappingStrategy.DetermineScript(request);
					Thread.SetData(_channel, channel);

					if (script != null)
					{
						Log.Debug("Begin AGIScript " + script.GetType().FullName + " on " + Thread.CurrentThread.Name);
						script.Service(request, channel);
						Log.Debug("End AGIScript " + script.GetType().FullName + " on " + Thread.CurrentThread.Name);
					}
					else
					{
						var error = "No script configured for URL '" + request.RequestURL + "' (script '" + request.Script +
									"')";
						channel.SendCommand(new VerboseCommand(error, 1));
						Log.Error(error);
					}
				}
				else
				{
					var error = "A connection was made with no requests";
					Log.Error(error);
				}
			}
			catch (AGIHangupException)
			{
			}
			catch (IOException)
			{
			}
			catch (AGIException ex)
			{
				Log.Error(ex, "AGIException while handling request");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected Exception while handling request");
				throw;
			}

			Thread.SetData(_channel, null);
			try
			{
				socket.Close();
			}
				catch (IOException ex)
				{
				Log.Error(ex, "Error on close socket");
				}
		}
	}
}