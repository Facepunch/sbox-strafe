﻿
using Strafe.Players;

namespace Strafe.Api.Messages;

internal class CompletionSubmitResult
{

	public long CompletionId { get; set; }
	public int NewRank { get; set; }
	public int OldRank { get; set; }
	public float NewTime { get; set; }
	public float OldTime { get; set; }
	public TimerFrame Comparison { get; set; }
	public int Credits { get; set; }

	public bool IsPersonalBest => OldTime == 0 || NewTime < OldTime;

}
