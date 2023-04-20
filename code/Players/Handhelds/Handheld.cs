
namespace Strafe.Players;

internal class Handheld : AnimatedEntity
{

	protected virtual string ViewModelPath => string.Empty;
	public HandheldViewModel ViewModel { get; private set; }

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if( ViewModel == null && cl.IsOwnedByLocalClient )
		{
			ViewModel = new HandheldViewModel();
			ViewModel.Owner = Owner;
			ViewModel.SetModel( ViewModelPath );
			ViewModel.EnableViewmodelRendering = true;
		}

		if ( Input.Pressed( "attack1" ) )
		{
			PrimaryAttack();
		}

		if ( Input.Pressed( "attack2" ) )
		{
			SecondaryAttack();
		}
	}

	protected virtual void PrimaryAttack()
	{

	}

	protected virtual void SecondaryAttack()
	{

	}

}
