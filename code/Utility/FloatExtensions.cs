
using Sandbox;
using System;

namespace Strafe.Utility;

internal static class FloatExtensions
{

	public static string ToTimeMs( this float seconds ) => TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss\.ff" );
	public static string ToTime( this float seconds ) => TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss" );
	public static string ToTimeMs( this RealTimeSince rt ) => ToTimeMs( (float)rt );
	public static string ToTime( this RealTimeSince rt ) => ToTime( (float)rt );

}
