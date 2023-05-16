using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsterNET.IO
{
	/// <summary>
	/// ServerSocket using standard socket classes.
	/// </summary>
	public class ServerSocket
	{
		private TcpListener tcpListener;
		private Encoding encoding;

		public ServerSocket(int port, IPAddress bindAddress, Encoding encoding)
		{
			this.encoding = encoding;
			tcpListener = new TcpListener(new IPEndPoint(bindAddress, port));
			try
			{
				tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception Enabling Keep Alive on Server Socket (1)");
			}
			try
			{
				tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception Enabling Keep Alive on Server Socket (2)");
			}
			try
			{
				tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception Enabling Keep Alive on Server Socket (3)");
			}
			try
			{
				tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception Enabling Keep Alive on Server Socket (4)");
			}

			tcpListener.Start();
		}
		
		public IO.SocketConnection? Accept()
		{
			if (tcpListener != null)
			{
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				try
				{
					tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception Enabling Keep Alive on Client Socket (1)");
				}
				try
				{
					tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception Enabling Keep Alive on Client Socket (2)");
				}
				try
				{
					tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception Enabling Keep Alive on Client Socket (3)");
				}
				try
				{
					tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception Enabling Keep Alive on Client Socket (4)");
				}


				if (tcpClient != null)
					return new IO.SocketConnection(tcpClient, encoding);
			}
			return null;
		}
		
		public void Close()
		{
			tcpListener.Stop();
		}
	}
}
