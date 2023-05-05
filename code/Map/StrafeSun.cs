
using Sandbox;
using Editor;
using System.Linq;

namespace Strafe.Map;

[Library( "strafe_sun_controller" )]
[HammerEntity]
[Title( "Sun Controller" )]
[Description("Gives the sun a day/night cycle")]
internal partial class StrafeSun : Entity
{

	[Net]
	public bool Paused { get; set; }
	[Net]
	public float TimeOfDay { get; set; }
	[Net]
	public EnvironmentLightEntity SunEntity { get; set; }
	[Net]
	public GradientFogEntity FogEntity { get; set; }
	//[Net]
	public Color NightSkyColor => new Color( .256f, .272f, .436f );
	//[Net]
	public Color DaySkyColor => new Color( .85f, 0.8f, 0.8f );

	/// <summary>
	/// Length from midnight to midnight, in seconds
	/// </summary>
	public int DayLength => 240;

	/// <summary>
	/// Night time will go by this much faster
	/// </summary>
	private float NightSpeedMultiplier => 1f;

	private const float DayBegin = .57f;
	private const float SunsetBegin = .75f;
	private const float SunsetEnd = .9f;
	private const float SunriseBegin = .45f;
	private const float SunriseEnd = .52f;
	private const float Night = 1;

	private const float RealSecondsPerDay = 86400f;

	public override void Spawn()
	{
		base.Spawn();

		TimeOfDay = RealSecondsPerDay * SunriseEnd;

		SunEntity = All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
		FogEntity = All.FirstOrDefault( x => x is GradientFogEntity ) as GradientFogEntity;
	}

	[GameEvent.Tick.Server]
	private void OnTick()
	{
		if ( !SunEntity.IsValid() ) return;

		if ( !Paused )
		{
			var increment = Time.Delta * (RealSecondsPerDay / DayLength);
			if ( IsNight() ) increment *= NightSpeedMultiplier;

			TimeOfDay += increment;

			if ( TimeOfDay >= RealSecondsPerDay )
			{
				TimeOfDay = 0;
			}
		}

		var alpha = TimeOfDay / RealSecondsPerDay;
		var pitch = 0f.LerpTo( 360, alpha ) - 180f;
		SunEntity.Rotation = Rotation.From( new Angles( pitch, 90, 0 ) ) * Rotation.FromYaw( -43 );
		SunEntity.SkyColor = GetSkyColor();
		SunEntity.Color = GetSunColor();

		if( FogEntity.IsValid() )
		{
			FogEntity.FogFadeTime = 0f;
			FogEntity.SetFogColor( GetFogColor() );
			FogEntity.SetFogMaxOpacity( 1f );
		}
	}

	private bool IsNight()
	{
		var a = TimeOfDay / RealSecondsPerDay;
		return a < SunriseBegin || a > SunsetEnd;
	}

	private Color GetSunColor()
	{
		var sunsetColor = new Color( .4f, 0.2f, 0.15f );
		var sunriseColor = new Color( .25f, 0.18f, 0.18f );
		var nightColor = new Color( .02f, .02f, .02f );
		var dayColor = new Color( 0.69f, 0.69f, 0.69f );
		var a = TimeOfDay / RealSecondsPerDay;

		if ( a > SunriseBegin && a <= SunriseEnd )
		{
			return Color.Lerp( nightColor, sunriseColor, a.LerpInverse( SunriseBegin, SunriseEnd ) );
		}
		else if ( a > SunriseEnd && a <= DayBegin )
		{
			return Color.Lerp( sunriseColor, dayColor, a.LerpInverse( SunriseEnd, DayBegin ) );
		}
		else if ( a > SunsetBegin && a <= SunsetEnd )
		{
			return Color.Lerp( dayColor, sunsetColor, a.LerpInverse( SunsetBegin, SunsetEnd ) );
		}
		else if ( a > SunsetEnd && a <= Night )
		{
			return Color.Lerp( sunsetColor, nightColor, a.LerpInverse( SunsetEnd, Night ) );
		}
		else if ( a > DayBegin && a <= SunsetBegin )
		{
			return dayColor;
		}

		return nightColor;
	}

	private Color GetSkyColor()
	{
		var a = TimeOfDay / RealSecondsPerDay;

		if ( a > SunriseBegin && a <= DayBegin )
		{
			return Color.Lerp( NightSkyColor, DaySkyColor, a.LerpInverse( SunriseBegin, DayBegin ) );
		}
		else if ( a > SunsetBegin && a <= Night )
		{
			return Color.Lerp( DaySkyColor, NightSkyColor, a.LerpInverse( SunsetBegin, Night ) );
		}
		else if ( a > DayBegin && a <= SunsetBegin )
		{
			return DaySkyColor;
		}

		return NightSkyColor;
	}

	private Color GetFogColor()
	{
		var nightfogcolor = Color.Black;
		var a = TimeOfDay / RealSecondsPerDay;

		if ( a > SunriseBegin && a <= DayBegin )
		{
			return Color.Lerp( nightfogcolor, GetSunColor(), a.LerpInverse( SunriseBegin, DayBegin ) );
		}
		else if ( a > SunsetBegin && a <= Night )
		{
			return Color.Lerp( GetSunColor(), nightfogcolor, a.LerpInverse( SunsetBegin, Night ) );
		}
		else if ( a > DayBegin && a <= SunsetBegin )
		{
			return DaySkyColor;
		}

		return nightfogcolor;
	}

}
