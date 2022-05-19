
namespace Strafe.Api;

internal class UploadReplay
{

	public long CompletionId { get; set; }
	public string ReplayBase64 { get; set; }

}

internal class UploadReplayResult
{

	public bool Success { get; set; }
	public string Url { get; set; }

}
