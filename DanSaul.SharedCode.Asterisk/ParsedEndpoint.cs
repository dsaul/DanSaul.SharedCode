using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedCode.Asterisk
{
	public record ParsedEndpoint(
		string? Name,
		string? State,
		string? Channels,
		List<ParsedAor> Aors,
		List<ParsedContact> Contacts,
		List<ParsedTransport> Transports,
		List<string> Identify,
		List<string> Match,
		List<string> InAuth,
		List<string> OutAuth,
		List<ParsedExten> Exten
		)
	{
		static char[] validNameChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwzyz0123456789-".ToCharArray();

		public static ParsedEndpoint? FromOutputSection(string section)
		{
			//Console.WriteLine($"section '{section}'");


			string? endpointName = null;
			string? endpointState = null;
			string? endpointActiveChannelCount = null;
			List<ParsedAor> aors = new();
			List<ParsedContact> contacts = new();
			List<ParsedTransport> transports = new();
			List<string> identify = new();
			List<string> match = new();
			List<string> inAuth = new();
			List<string> outAuth = new();
			List<ParsedChannel> channels = new();
			List<ParsedExten> exten = new();

			string[] lines = section.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("Endpoint:"))
				{
					// Endpoint:  <Endpoint/CID.....................................>  <State.....>  <Channels.>

					string nameRegex = @"(?<=Endpoint:).+(?=\ +(Not|Avail|Unavailable))";
					Match nameMatch = Regex.Match(trimmed, nameRegex);
					if (!nameMatch.Success)
						return null;
					endpointName = nameMatch.Value.Trim();
					//Console.WriteLine($"name '{name}'");

					string stateRegex = @"(?<=" + Regex.Escape(endpointName) + @" ).+(?=\ +([0-9]))";
					Match stateMatch = Regex.Match(trimmed, stateRegex);
					if (!stateMatch.Success)
						return null;
					endpointState = stateMatch.Value.Trim();
					//Console.WriteLine($"state '{endpointState}'");

					string channelsRegex = @"(?<=" + Regex.Escape(endpointState) + @" ).+$";
					Match channelsMatch = Regex.Match(trimmed, channelsRegex);
					if (!channelsMatch.Success)
						return null;
					endpointActiveChannelCount = channelsMatch.Value.Trim();
					//Console.WriteLine($"channels '{endpointChannels}'");


				}
				else if (trimmed.StartsWith("Aor:"))
				{
					// Aor:  <Aor............................................>  <MaxContact>
					string nameRegex = @"(?<=Aor:).+(?=\ +[0-9])";
					Match nameMatch = Regex.Match(trimmed, nameRegex);
					if (!nameMatch.Success)
						throw new InvalidOperationException();
					string aorName = nameMatch.Value.Trim();
					//Console.WriteLine($"aorName '{aorName}'");

					string maxContactsRegex = @"(?<=" + Regex.Escape(aorName) + @" ) +[0-9]+";
					Match maxContactsMatch = Regex.Match(trimmed, maxContactsRegex);
					if (!maxContactsMatch.Success)
						throw new InvalidOperationException();
					string maxContactsStr = maxContactsMatch.Value.Trim();
					if (!int.TryParse(maxContactsStr, out int maxContacts))
						throw new InvalidOperationException();

					//Console.WriteLine($"maxContacts '{maxContacts}'");

					aors.Add(new ParsedAor(endpointName, maxContacts));
				}
				else if (trimmed.StartsWith("Contact:"))
				{
					// Contact:  <Aor/ContactUri..........................> <Hash....> <Status> <RTT(ms)..>

					string startSentinel = "Contact:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					int nameStartIdx = trimmed.IndexOfAny(validNameChars, startIdx);
					if (nameStartIdx == -1)
						throw new InvalidOperationException();
					int nameEndIdx = trimmed.IndexOf(' ', nameStartIdx);
					if (nameEndIdx == -1)
						throw new InvalidOperationException();

					string contactName = trimmed.Substring(nameStartIdx, nameEndIdx - nameStartIdx);

					int hashStartIdx = trimmed.IndexOfAny(validNameChars, nameEndIdx);
					if (hashStartIdx == -1)
						throw new InvalidOperationException();
					int hashEndIdx = trimmed.IndexOf(' ', hashStartIdx);
					if (hashEndIdx == -1)
						throw new InvalidOperationException();

					string contactHash = trimmed.Substring(hashStartIdx, hashEndIdx - hashStartIdx);


					int statusStartIdx = trimmed.IndexOfAny(validNameChars, hashEndIdx);
					if (statusStartIdx == -1)
						throw new InvalidOperationException();
					int statusEndIdx = trimmed.IndexOf(' ', statusStartIdx);
					if (statusEndIdx == -1)
						throw new InvalidOperationException();

					// Created/Avail/NonQual/Unavail
					string contactStatus = trimmed.Substring(statusStartIdx, statusEndIdx - statusStartIdx);

					// RTT
					string contactRttStr = trimmed.Substring(statusEndIdx).Trim();
					double contactRtt = double.Parse(contactRttStr);

					contacts.Add(new ParsedContact(contactName, contactHash, contactStatus, contactRtt));
				}
				else if (trimmed.StartsWith("Transport:"))
				{
					// Transport:  <TransportId........>  <Type>  <cos>  <tos>  <BindAddress..................>
					// Transport:  transport-udp             udp      3     96  0.0.0.0:5060

					string startSentinel = "Transport:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					int transportIdStartIdx = trimmed.IndexOfAny(validNameChars, startIdx);
					if (transportIdStartIdx == -1)
						throw new InvalidOperationException();
					int transportIdEndIdx = trimmed.IndexOf(' ', transportIdStartIdx);
					if (transportIdEndIdx == -1)
						throw new InvalidOperationException();
					string transportId = trimmed.Substring(transportIdStartIdx, transportIdEndIdx - transportIdStartIdx);


					int typeStartIdx = trimmed.IndexOfAny(validNameChars, transportIdEndIdx);
					if (typeStartIdx == -1)
						throw new InvalidOperationException();
					int typeEndIdx = trimmed.IndexOf(' ', typeStartIdx);
					if (typeEndIdx == -1)
						throw new InvalidOperationException();
					string transportType = trimmed.Substring(typeStartIdx, typeEndIdx - typeStartIdx);


					int cosStartIdx = trimmed.IndexOfAny(validNameChars, typeEndIdx);
					if (cosStartIdx == -1)
						throw new InvalidOperationException();
					int cosEndIdx = trimmed.IndexOf(' ', cosStartIdx);
					if (cosEndIdx == -1)
						throw new InvalidOperationException();
					string transportCos = trimmed.Substring(cosStartIdx, cosEndIdx - cosStartIdx);

					int tosStartIdx = trimmed.IndexOfAny(validNameChars, cosEndIdx);
					if (tosStartIdx == -1)
						throw new InvalidOperationException();
					int tosEndIdx = trimmed.IndexOf(' ', tosStartIdx);
					if (tosEndIdx == -1)
						throw new InvalidOperationException();
					string transportTos = trimmed.Substring(tosStartIdx, tosEndIdx - tosStartIdx);


					string transportBindAddress = trimmed.Substring(tosEndIdx).Trim();

					transports.Add(new ParsedTransport(transportId, transportType, transportCos, transportTos, transportBindAddress));
				}
				else if (trimmed.StartsWith("Identify:"))
				{
					//   Identify:  <Identify/Endpoint.........................................................>
					// Identify:  prod-client-2architecture-wildix-1-identify/prod-client-2architecture-wildix-1

					string startSentinel = "Identify:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					string identifyVal = trimmed.Substring(startIdx).Trim();

					identify.Add(identifyVal);
				}
				else if (trimmed.StartsWith("Match:"))
				{
					// Match:  <criteria.........................>
					// Match: 3.96.171.234/32

					string startSentinel = "Match:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					string matchVal = trimmed.Substring(startIdx).Trim();

					match.Add(matchVal);
				}
				else if (trimmed.StartsWith("InAuth:"))
				{
					// I/OAuth:  <AuthId/UserName...........................................................>
					// InAuth:  prod-client-bisonfirehardware-wildix-2-iauth/prod-client-bisonfirehardware-wildix-2

					string startSentinel = "InAuth:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					string val = trimmed.Substring(startIdx).Trim();

					inAuth.Add(val);
				}
				else if (trimmed.StartsWith("OutAuth:"))
				{
					// I/OAuth:  <AuthId/UserName...........................................................>
					// OutAuth:  prod-egress-toronto-1-les-1-oauth/E098D2DCB8D6A

					string startSentinel = "OutAuth:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					string val = trimmed.Substring(startIdx).Trim();

					outAuth.Add(val);
				}
				else if (trimmed.StartsWith("Channel:"))
				{
					// Channel:  <ChannelId......................................>  <State.....>  <Time.....>
					// Channel: PJSIP/prod-egress-calgary-1-distributel-east-000097 Down          00:00:25

					string startSentinel = "Channel:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					int channelIdStartIdx = trimmed.IndexOfAny(validNameChars, startIdx);
					if (channelIdStartIdx == -1)
						throw new InvalidOperationException();
					int channelIdEndIdx = trimmed.IndexOf(' ', channelIdStartIdx);
					if (channelIdEndIdx == -1)
						throw new InvalidOperationException();
					string channelId = trimmed.Substring(channelIdStartIdx, channelIdEndIdx - channelIdStartIdx);


					int stateStartIdx = trimmed.IndexOfAny(validNameChars, channelIdEndIdx);
					if (stateStartIdx == -1)
						throw new InvalidOperationException();
					int stateEndIdx = trimmed.IndexOf(' ', stateStartIdx);
					if (stateEndIdx == -1)
						throw new InvalidOperationException();
					string state = trimmed.Substring(stateStartIdx, stateEndIdx - stateStartIdx);

					string time = trimmed.Substring(stateEndIdx).Trim();

					channels.Add(new ParsedChannel(channelId, state, time));

				}
				else if (trimmed.StartsWith("Exten:"))
				{
					string startSentinel = "Exten:";
					int startIdx = trimmed.IndexOf(startSentinel) + startSentinel.Length;
					if (startIdx == -1)
						throw new InvalidOperationException();

					int extenStartIdx = trimmed.IndexOfAny(validNameChars, startIdx);
					if (extenStartIdx == -1)
						throw new InvalidOperationException();
					int extenEndIdx = trimmed.IndexOf(' ', extenStartIdx);
					if (extenEndIdx == -1)
						throw new InvalidOperationException();
					string e = trimmed.Substring(extenStartIdx, extenEndIdx - extenStartIdx);

					string nextSentinel = "CLCID:";
					int clcStartIdx = trimmed.IndexOf(nextSentinel, extenEndIdx) + nextSentinel.Length;
					if (clcStartIdx == -1)
						throw new InvalidOperationException();

					string clc = trimmed.Substring(clcStartIdx);

					exten.Add(new ParsedExten(e, clc));
				}
				//else if (trimmed.StartsWith("Endpoint:"))
				//{

				//}
				else
				{
					Log.Error("Unhandled Line {line}", line);
					throw new InvalidOperationException();
				}


			}

			return new ParsedEndpoint(
				endpointName,
				endpointState,
				endpointActiveChannelCount,
				aors,
				contacts,
				transports,
				identify,
				match,
				inAuth,
				outAuth,
				exten
				);

		}
	}
}
