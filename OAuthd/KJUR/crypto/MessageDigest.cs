using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.crypto
{
#pragma warning disable IDE1006 // Naming Styles
	class MessageDigest
	{
		public delegate void UpdateStringAction(object utf8StringOrByteArray);

		public UpdateStringAction updateString { get; private set; }
		public Action<string> updateHex { get; private set; }
		public Func<string> digest { get; private set; }
		public Func<string, string> digestString { get; private set; }
		public Func<string, string> digestHex { get; private set; }
		public System.Security.Cryptography.HashAlgorithm md { get; private set; }
		public string algName { get; private set; }
		public string provName { get; private set; }

		private MessageDigest()
		{
			this.updateString = (_) => { throw new NotSupportedException("updateString(str) not supported for this alg/prov: " + this.algName + "/" + this.provName); };
			this.updateHex = (_) => { throw new NotSupportedException("updateHex(hex) not supported for this alg/prov: " + this.algName + "/" + this.provName); };
			this.digest = () => { throw new NotSupportedException("digest() not supported for this alg/prov: " + this.algName + "/" + this.provName); };
			this.digestString = (_) => { throw new NotSupportedException("digestString(str) not supported for this alg/prov: " + this.algName + "/" + this.provName); };
			this.digestHex = (_) => { throw new NotSupportedException("digestHex(hex) not supported for this alg/prov: " + this.algName + "/" + this.provName); };
		}
		public MessageDigest(object n)
			: this()
		{
			if (n != null)
			{
				var nObj = new Newtonsoft.Json.Linq.JObject(n);
				var n_alg = nObj["alg"];
				if (n_alg != null)
				{
					this.algName = n_alg.ToString();
					var n_prov = nObj["prov"];
					if (n_prov == null)
					{
						this.provName = KJUR.crypto.Util.DEFAULTPROVIDER[this.algName];
					}
					this.setAlgAndProvider(this.algName, this.provName);
				}
			}
		}
		public MessageDigest(string alg, string prov = null)
			: this()
		{
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
			if (alg != null && prov == null)
				prov = KJUR.crypto.Util.DEFAULTPROVIDER[alg];
			
			if (":md5:sha1:sha224:sha256:sha384:sha512:ripemd160:".IndexOf(alg) != -1 && prov == "cryptojs")
			{
				try
				{
					var typeName = KJUR.crypto.Util.CRYPTOJSMESSAGEDIGESTNAME[alg];
					this.md = System.Security.Cryptography.HashAlgorithm.Create(alg);
					this.md.Initialize();
					//this.md = (object)Activator.CreateInstance(Type.GetType(typeName));
				}
				catch (Exception ex)
				{
					throw new Exception("setAlgAndProvider hash alg set fail alg=" + alg + "/" + ex, ex);
				}
				this.updateString = (str) =>
				{
					//this.md.update(str);
					if (!(str is byte[] bytes))
						bytes = Encoding.UTF8.GetBytes(str.ToString());
					var buffer = new byte[this.md.HashSize];
					this.md.TransformBlock(bytes, 0, bytes.Length, buffer, 0);
				};
				this.updateHex = (hex) =>
				{
					//var t = CryptoJS.enc.Hex.parse(hex); this.md.update(t);
					this.updateString(hex.HexStringToBase64().Base64ToByteArray());
				};
				this.digest = () =>
				{
					//var n = this.md.finalize(); return n.toString(CryptoJS.enc.Hex);
					var hash = this.md.TransformFinalBlock(new byte[0], 0, 0);
					return this.md.Hash.ToHexString();
				};
				this.digestString = (str) => { this.updateString(str); return this.digest(); };
				this.digestHex = (hex) => { this.updateHex(hex); return this.digest(); };
			}
			if (":sha256:".IndexOf(alg) != -1 && prov == "sjcl")
			{
				try
				{
					throw new NotSupportedException();
					//this.md = new sjcl.hash.sha256();
					//var md2 = System.Security.Cryptography.SHA256Cng.Create();
				}
				catch (Exception ex)
				{
					throw new Exception("setAlgAndProvider hash alg set fail alg=" + alg + "/" + ex, ex);
				}
				//this.updateString = (str) => { this.md.update(str); };
				//this.updateHex = (hex) => { var t = sjcl.codec.hex.toBits(hex); this.md.update(t); };
				//this.digest = () => { var n = this.md.finalize(); return sjcl.codec.hex.fromBits(n); };
				//this.digestString = (str) => { this.updateString(str); return this.digest(); };
				//this.digestHex = (hex) => { this.updateHex(hex); return this.digest(); };
			}
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
