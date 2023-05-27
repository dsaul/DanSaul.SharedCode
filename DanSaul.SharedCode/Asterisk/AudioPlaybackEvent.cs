// (c) 2023 Dan Saul
using Amazon.Polly;
using Npgsql;

namespace DanSaul.SharedCode.Asterisk
{
	public record AudioPlaybackEvent
	{
		public AudioPlaybackEventType Type { get; set; } = AudioPlaybackEventType.Unknown;
		public string? StreamFile { get; set; } = null;
		public string? Alpha { get; set; } = null;
		public string? Text { get; set; } = null;
		public string? Digits { get; set; } = null;
		public Engine Engine { get; set; } = Engine.Neural;
		public VoiceId Voice { get; set; } = VoiceId.Brian;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public Guid? RecordingId { get; set; } = null;
	}
}
