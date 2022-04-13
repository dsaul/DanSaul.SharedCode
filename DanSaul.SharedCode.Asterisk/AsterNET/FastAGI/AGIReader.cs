using System.Collections.Generic;
using System.IO;
using AsterNET.IO;
using Serilog;

namespace AsterNET.FastAGI
{
    public class AGIReader
    {

        private readonly SocketConnection socket;

        public AGIReader(SocketConnection socket)
        {
            this.socket = socket;
        }

        public AGIRequest ReadRequest()
        {
            var lines = new List<string>();
            try
            {
                Log.Debug("AGIReader.ReadRequest():");
                string line;
                while ((line = socket.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        break;
                    lines.Add(line);
                    Log.Debug(line);
                }
            }
            catch (IOException ex)
            {
                throw new AGINetworkException("Unable to read request from Asterisk: " + ex.Message, ex);
            }

            var request = new AGIRequest(lines)
            {
                LocalAddress = socket.LocalAddress,
                LocalPort = socket.LocalPort,
                RemoteAddress = socket.RemoteAddress,
                RemotePort = socket.RemotePort
            };

            return request;
        }

        public AGIReply ReadReply()
        {
            string line;
            var badSyntax = ((int) AGIReplyStatuses.SC_INVALID_COMMAND_SYNTAX).ToString();

            var lines = new List<string>();
            try
            {
                line = socket.ReadLine();
            }
            catch (IOException ex)
            {
                throw new AGINetworkException("Unable to read reply from Asterisk: " + ex.Message, ex);
            }
            if (line == null)
                throw new AGIHangupException();

            lines.Add(line);
            // read synopsis and usage if statuscode is 520
            if (line.StartsWith(badSyntax))
                try
                {
                    while ((line = socket.ReadLine()) != null)
                    {
                        lines.Add(line);
                        if (line.StartsWith(badSyntax))
                            break;
                    }
                }
                catch (IOException ex)
                {
                    throw new AGINetworkException("Unable to read reply from Asterisk: " + ex.Message, ex);
                }
            return new AGIReply(lines);
        }
    }
}