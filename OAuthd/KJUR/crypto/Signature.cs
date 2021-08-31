using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.crypto
{
#pragma warning disable IDE1006 // Naming Styles
	/*
	class Signature
	{
		public Action<object, string> init { get; private set; }
		public Action<object> initSign { get; private set; }
		public Action<object> initVerifyByPublicKey { get; private set; }
		public Action<object> initVerifyByCertificatePEM { get; private set; }
		public Action<object> updateString { get; private set; }
		public Action<object> updateHex { get; private set; }
		public Func<object> sign { get; private set; }
		public Func<object, object> signString { get; private set; }
		public Func<object, object> signHex { get; private set; }
		public Func<object, object> verify { get; private set; }
		public MessageDigest md { get; private set; }
		public string algName { get; private set; }
		public string provName { get; private set; }
		public string algProvName { get; private set; }
		public string mdAlgName { get; private set; }
		public string pubkeyAlgName { get; private set; }
		public object prvKey { get; private set; }
		public object pubKey { get; private set; }
		public string state { get; private set; }
		public object ecpubhex { get; private set; }
		public object ecprvhex { get; private set; }
		public string eccurvename { get; private set; }
		public int pssSaltLen { get; private set; }

		private Signature()
		{
			this.init = (_, _) => { throw new Exception("init(key, pass) not supported for this alg:prov=" + this.algProvName); };
			this.initVerifyByPublicKey = (_) => { throw new Exception("initVerifyByPublicKey(rsaPubKeyy) not supported for this alg:prov=" + this.algProvName); };
			this.initVerifyByCertificatePEM = (_) => { throw new Exception("initVerifyByCertificatePEM(certPEM) not supported for this alg:prov=" + this.algProvName); };
			this.initSign = (_) => { throw new Exception("initSign(prvKey) not supported for this alg:prov=" + this.algProvName); };
			this.updateString = (_) => { throw new Exception("updateString(str) not supported for this alg:prov=" + this.algProvName); };
			this.updateHex = (_) => { throw new Exception("updateHex(hex) not supported for this alg:prov=" + this.algProvName); };
			this.sign = () => { throw new Exception("sign() not supported for this alg:prov=" + this.algProvName); };
			this.signString = (_) => { throw new Exception("digestString(str) not supported for this alg:prov=" + this.algProvName); };
			this.signHex = (_) => { throw new Exception("digestHex(hex) not supported for this alg:prov=" + this.algProvName); };
			this.verify = (_) => { throw new Exception("verify(hSigVal) not supported for this alg:prov=" + this.algProvName); };
		}
		public Signature(object n)
			: this()
		{
			object t = null;
			this.initParams = n;
			if (n != null)
			{
				if (n.alg != null)
				{
					this.algName = n.alg;
					this.provName = n.prov == null ? KJUR.crypto.Util.DEFAULTPROVIDER[this.algName] : n.prov;
					this.algProvName = this.algName + ":" + this.provName;
					this.setAlgAndProvider(this.algName, this.provName);
					this._setAlgNames();
				}
				if (n.psssaltlen != null)
				{
					this.pssSaltLen = n.psssaltlen;
				}
				if (n.prvkeypem != null)
				{
					if (n.prvkeypas != null)
						throw new ArgumentException("both prvkeypem and prvkeypas parameters not supported");
					else
						try
						{
							t = new RSAKey();
							t.readPrivateKeyFromPEMString(n.prvkeypem);
							this.initSign(t);
						}
						catch (Exception ex)
						{
							throw new Exception("fatal error to load pem private key: " + ex, ex);
						}
				}
			}
		}

		private void _setAlgNames()
		{
			var match = System.Text.RegularExpressions.Regex.Match(this.algName, "^(.+)with(.+)$");
			if (match.Success)
			{
				this.mdAlgName = match.Groups[1].Value.ToLower();
				this.pubkeyAlgName = match.Groups[2].Value.ToLower();
			}
		}
		private string _zeroPaddingOfSignature(string sig, int length)
		{
			string padding = "";
			int padLength = length / 4 - sig.Length;
			for (var padIdx = 0; padIdx < padLength; padIdx++)
				padding = padding + "0";
			return padding + sig;
		}
		public void setAlgAndProvider(string alg, string prov)
		{
			this._setAlgNames();
			if (prov != "cryptojs/jsrsa")
				throw new ArgumentException("provider not supported: " + prov, nameof(prov));
			if (":md5:sha1:sha224:sha256:sha384:sha512:ripemd160:".IndexOf(this.mdAlgName) != -1)
			{
				try
				{
					this.md = new KJUR.crypto.MessageDigest(alg: this.mdAlgName);
				}
				catch (Exception ex)
				{
					throw new Exception("setAlgAndProvider hash alg set fail alg=" + this.mdAlgName + "/" + ex, ex);
				}
				this.init = (key, pass) =>
				{
					object keyObj = null;
					try
					{
						keyObj = pass == null ? KEYUTIL.getKey(key) : KEYUTIL.getKey(key, pass);
					}
					catch (Exception r)
					{
						throw new Exception("init failed:" + r, r);
					}
					if (keyObj.isPrivate == true)
					{
						this.prvKey = keyObj;
						this.state = "SIGN";
					}
					else if (keyObj.isPublic == true)
					{
						this.pubKey = keyObj;
						this.state = "VERIFY";
					}
					else
						throw new Exception("init failed.:" + keyObj);
				};
				this.initSign = (prvKey) =>
				{
					if (typeof(string) == prvKey.ecprvhex.GetType() && typeof(string) == prvKey.eccurvename.GetType())
					{
						this.ecprvhex = prvKey.ecprvhex;
						this.eccurvename = prvKey.eccurvename;
					}
					else
						this.prvKey = prvKey;
					this.state = "SIGN";
				};
				this.initVerifyByPublicKey = (rsaPubKeyy) =>
				{
					if (typeof(string) == rsaPubKeyy.ecpubhex.GetType() && typeof(string) == rsaPubKeyy.eccurvename.GetType())
					{
						this.ecpubhex = rsaPubKeyy.ecpubhex;
						this.eccurvename = rsaPubKeyy.eccurvename;
					}
					else
					{
						if (rsaPubKeyy is KJUR.crypto.ECDSA)
							this.pubKey = rsaPubKeyy;
						else if (rsaPubKeyy is RSAKey)
							this.pubKey = rsaPubKeyy;
					}
					this.state = "VERIFY";
				};
				this.initVerifyByCertificatePEM = (certPEM) =>
				{
					var x509 = new X509();
					x509.readCertPEM(certPEM);
					this.pubKey = x509.subjectPublicKeyRSA;
					this.state = "VERIFY";
				};
				this.updateString = (str) => { this.md.updateString(str); };
				this.updateHex = (hex) => { this.md.updateHex(hex); };
				this.sign = () =>
				{
					this.sHashHex = this.md.digest();
					if (this.ecprvhex != null && this.eccurvename != null)
					{
						var n = new KJUR.crypto.ECDSA(new { curve = this.eccurvename});
						this.hSign = n.signHex(this.sHashHex, this.ecprvhex);
					}
					else if (this.pubkeyAlgName == "rsaandmgf1")
						this.hSign = this.prvKey.signWithMessageHashPSS(this.sHashHex, this.mdAlgName, this.pssSaltLen);
					else if (this.pubkeyAlgName == "rsa")
						this.hSign = this.prvKey.signWithMessageHash(this.sHashHex, this.mdAlgName);
					else if (this.prvKey is KJUR.crypto.ECDSA)
						this.hSign = this.prvKey.signWithMessageHash(this.sHashHex);
					else if (this.prvKey is KJUR.crypto.DSA)
						this.hSign = this.prvKey.signWithMessageHash(this.sHashHex);
					else
						throw new Exception("Signature: unsupported public key alg: " + this.pubkeyAlgName);
					return this.hSign;
				};
				this.signString = (str) => { this.updateString(str); return this.sign(); };
				this.signHex = (hex) => { this.updateHex(hex); return this.sign(); };
				this.verify = (hSigVal) =>
				{
					this.sHashHex = this.md.digest();
					if (this.ecpubhex != null && this.eccurvename != null)
					{
						var t2 = new System.Security.Cryptography.ECDsaCng(System.Security.Cryptography.ECCurve.CreateFromFriendlyName(this.eccurvename));
						var t = new KJUR.crypto.ECDSA(new { curve = this.eccurvename });
						return t.verifyHex(this.sHashHex, hSigVal, this.ecpubhex);
					}
					if (this.pubkeyAlgName == "rsaandmgf1")
						return this.pubKey.verifyWithMessageHashPSS(this.sHashHex, hSigVal, this.mdAlgName, this.pssSaltLen);
					if (this.pubkeyAlgName == "rsa" || this.pubKey is KJUR.crypto.ECDSA || this.pubKey is KJUR.crypto.DSA)
						return this.pubKey.verifyWithMessageHash(this.sHashHex, hSigVal);
					throw new Exception("Signature: unsupported public key alg: " + this.pubkeyAlgName);
				};
			}
		}
	}
	//*/
#pragma warning restore IDE1006 // Naming Styles
}
