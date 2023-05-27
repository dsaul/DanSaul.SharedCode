// (c) 2023 Dan Saul
using DanSaul.SharedCode.Mongo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.MQTT
{
    public record MQTTMessageNumbers : MQTTMessage
	{
		[JsonProperty]
		public IEnumerable<NumberDocument> Numbers { get; init; } = Array.Empty<NumberDocument>();
	}
}
