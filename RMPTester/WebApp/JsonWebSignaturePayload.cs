using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonWebSignaturePayload : JsonWebSignaturePart
	{
		public JsonWebSignaturePayload(string base64Url)
			: base(base64Url)
		{
		}

		public Newtonsoft.Json.Linq.JToken ToJson() => Newtonsoft.Json.Linq.JToken.Parse(this.PlainText);
	}
}
