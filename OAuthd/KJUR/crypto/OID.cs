using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.crypto
{
#pragma warning disable IDE1006 // Naming Styles
	static class OID
	{
		public static readonly IDictionary<string, string> oidhex2name = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
		{
			["2a864886f70d010101"] = "rsaEncryption",
			["2a8648ce3d0201"] = "ecPublicKey",
			["2a8648ce380401"] = "dsa",
			["2a8648ce3d030107"] = "secp256r1",
			["2b8104001f"] = "secp192k1",
			["2b81040021"] = "secp224r1",
			["2b8104000a"] = "secp256k1",
			["2b81040023"] = "secp521r1",
			["2b81040022"] = "secp384r1",
			["2a8648ce380403"] = "SHA1withDSA",
			["608648016503040301"] = "SHA224withDSA",
			["608648016503040302"] = "SHA256withDSA",
		});
	}
#pragma warning restore IDE1006 // Naming Styles
}
