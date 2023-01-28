using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonWebSignaturePart
	{
		public virtual string Raw { get; }

		public string Hex => this.Raw.Base64UrlToBase64().Base64ToHexString();

		public string PlainText => this.Raw.Base64UrlToUtf8();

		protected JsonWebSignaturePart()
		{
		}
		public JsonWebSignaturePart(string base64Url)
		{
			this.Raw = base64Url;
		}

		public override string ToString() => this.PlainText;
	}
}
