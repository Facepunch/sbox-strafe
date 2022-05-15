
using Strafe.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strafe.Api;

internal partial class Backend
{

	public static async Task<List<PersonalBestEntry>> FetchPersonalBests( string mapIdent, int stage, int amount, int skip )
	{
		return await Get<List<PersonalBestEntry>>( $"pb/fetch?map={mapIdent}&stage={stage}&amount={amount}&skip={skip}" );
	}

}
