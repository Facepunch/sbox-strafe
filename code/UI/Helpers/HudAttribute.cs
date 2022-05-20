
using Sandbox;
using System;

namespace Strafe.UI;

[AttributeUsage( AttributeTargets.Class )]
internal class HudAttribute : LibraryAttribute, ITypeAttribute
{
	public Type TargetType { get; set; }
}
