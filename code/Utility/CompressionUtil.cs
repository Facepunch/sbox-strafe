using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Strafe.Utility;

public static class CompressionUtil
{

	public static string Compress( this string uncompressedString )
	{
		byte[] compressedBytes;

		using ( var uncompressedStream = new MemoryStream( Encoding.UTF8.GetBytes( uncompressedString ) ) )
		{
			using ( var compressedStream = new MemoryStream() )
			{
				using ( var compressorStream = new DeflateStream( compressedStream, CompressionLevel.Fastest, true ) )
				{
					uncompressedStream.CopyTo( compressorStream );
				}
				compressedBytes = compressedStream.ToArray();
			}
		}

		return Convert.ToBase64String( compressedBytes );
	}

	public static string Decompress( this string compressedString )
	{
		byte[] decompressedBytes;

		var compressedStream = new MemoryStream( Convert.FromBase64String( compressedString ) );

		using ( var decompressorStream = new DeflateStream( compressedStream, CompressionMode.Decompress ) )
		{
			using ( var decompressedStream = new MemoryStream() )
			{
				decompressorStream.CopyTo( decompressedStream );

				decompressedBytes = decompressedStream.ToArray();
			}
		}

		return Encoding.UTF8.GetString( decompressedBytes );
	}

	public static byte[] Compress( this byte[] data )
	{
		var output = new MemoryStream();
		using ( var dstream = new DeflateStream( output, CompressionLevel.Optimal ) )
		{
			dstream.Write( data, 0, data.Length );
		}
		return output.ToArray();
	}

	public static byte[] Decompress( this byte[] data )
	{
		var input = new MemoryStream( data );
		var output = new MemoryStream();
		using ( var dstream = new DeflateStream( input, CompressionMode.Decompress ) )
		{
			dstream.CopyTo( output );
		}
		return output.ToArray();
	}
}
