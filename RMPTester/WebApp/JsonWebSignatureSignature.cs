using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonWebSignatureSignature : JsonWebSignaturePart
	{
		public byte[] Bytes => this.Raw.Base64UrlToBase64().Base64ToByteArray();

		public JsonWebSignatureSignature(string base64Url)
			: base(base64Url)
		{
		}
	}
}
