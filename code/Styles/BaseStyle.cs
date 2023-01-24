
namespace Strafe;

class BaseStyle
{

	public virtual bool UseStamina => false;

	public virtual void Simulate( StrafeController ctrl )
	{

	}

	public virtual void BuildInput( StrafeController ctrl )
	{

	}

}

class NormalStyle : BaseStyle
{

}
