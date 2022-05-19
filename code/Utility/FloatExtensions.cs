
using System;

namespace Strafe.Utility;

internal static class FloatExtensions
{

	public static string ToTime( this float seconds, bool includePlusSign = false )
	{
		var tsSeconds = TimeSpan.FromSeconds( seconds );
		var format = tsSeconds.TotalSeconds > 60
			? @"m\:ss\.fff\s"
			: @"s\.fff\s";

		var result = tsSeconds.ToString( format );
		if ( seconds < 0 ) result = '-' + result;
		else if ( includePlusSign ) result = '+' + result;

		return result;
	}

}
