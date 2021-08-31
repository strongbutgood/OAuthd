using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.crypto
{
#pragma warning disable IDE1006 // Naming Styles
	class Mac
	{
		public Action<string> updateString { get; private set; }
		public Action<string> updateHex { get; private set; }
		public Func<string> doFinal { get; private set; }
		public Func<string, string> doFinalString { get; private set; }
		public Func<string, string> doFinalHex { get; private set; }
		public dynamic mac { get; private set; }
		public string algName { get; private set; }
		public string provName { get; private set; }
		public string algProv { get; private set; }
		public string pass { get; private set; }

		private Mac()
		{
			this.updateString = (_) => { throw new NotSupportedException("updateString(str) not supported for this alg/prov: " + this.algProv); };
			this.updateHex = (_) => { throw new NotSupportedException("updateHex(hex) not supported for this alg/prov: " + this.algProv); };
			this.doFinal = () => { throw new NotSupportedException("digest() not supported for this alg/prov: " + this.algProv); };
			this.doFinalString = (_) => { throw new NotSupportedException("digestString(str) not supported for this alg/prov: " + this.algProv); };
			this.doFinalHex = (_) => { throw new NotSupportedException("digestHex(hex) not supported for this alg/prov: " + this.algProv); };
		}
		public Mac(object n)
			: this()
		{
			if (n != null)
			{
				var nObj = new Newtonsoft.Json.Linq.JObject(n);
				var n_pass = nObj["pass"];
				if (n_pass != null)
					this.pass = n_pass.ToString();
				var n_alg = nObj["alg"];
				if (n_alg != null)
				{
					this.algName = n_alg.ToString();
					var n_prov = nObj["prov"];
					if (n_prov == null)
						this.provName = KJUR.crypto.Util.DEFAULTPROVIDER[this.algName];
					this.setAlgAndProvider(this.algName, this.provName);
				}
			}
		}
		public Mac(string alg, string prov = null, string pass = null)
			: this()
		{
			if (pass != null)
				this.pass = pass;
			if (alg != null)
			{
				this.algName = alg;
				if (prov == null)
					this.provName = KJUR.crypto.Util.DEFAULTPROVIDER[this.algName];
				this.setAlgAndProvider(this.algName, this.provName);
			}
		}

		public void setAlgAndProvider(string alg, string prov)
		{
			string hashAlg;
			//var mdObj;
			if (alg == null)
				alg = "hmacsha1";
			alg = alg.ToLower();
			if (alg.Substring(0, 4) != "hmac")
				throw new ArgumentException("setAlgAndProvider unsupported HMAC alg: " + alg);
			if (prov == null)
				prov = KJUR.crypto.Util.DEFAULTPROVIDER[alg];
			this.algProv = alg + "/" + prov;
			hashAlg = alg.Substring(4);
			if (":md5:sha1:sha224:sha256:sha384:sha512:ripemd160:".IndexOf(hashAlg) != -1 && prov == "cryptojs")
			{
				try
				{
					var typeName = KJUR.crypto.Util.CRYPTOJSMESSAGEDIGESTNAME[hashAlg];
					//mdObj = eval(typeName);
					//this.mac = CryptoJS.algo.HMAC.create(mdObj, this.pass);
				}
				catch (Exception ex)
				{
					throw new Exception("setAlgAndProvider hash alg set fail hashAlg=" + hashAlg + "/" + ex, ex);
				}
				this.updateString = (str) => { this.mac.update(str); };
				//this.updateHex = (hex) => { var t = CryptoJS.enc.Hex.parse(hex); this.mac.update(t); };
				//this.doFinal = () => { var n = this.mac.finalize(); return n.toString(CryptoJS.enc.Hex); };
				this.doFinalString = (str) => { this.updateString(str); return this.doFinal(); };
				this.doFinalHex = (hex) => { this.updateHex(hex); return this.doFinal(); };
			}
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
