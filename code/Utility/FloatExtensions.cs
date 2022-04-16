
using System;

namespace Strafe.Utility;

internal static class FloatExtensions
{

	public static string HumanReadable( this float seconds ) => TimeSpan.FromSeconds( seconds ).ToString( @"mm\:ss\.fff" );

}
