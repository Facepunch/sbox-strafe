
using System;
using System.Text;

namespace Strafe.Utility;

internal static class StringExtensions
{

	public static string ToBase64( this string plaintext ) => Convert.ToBase64String( Encoding.UTF8.GetBytes( plaintext ) );
	public static string FromBase64( this string base64 ) => Encoding.UTF8.GetString( Convert.FromBase64String( base64 ) );

}
