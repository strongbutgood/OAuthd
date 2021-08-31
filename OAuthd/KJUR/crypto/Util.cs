using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.crypto
{
#pragma warning disable IDE1006 // Naming Styles
	static class Util
	{
		public static readonly IDictionary<string, string> DIGESTINFOHEAD = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
		{
			["sha1"] = "3021300906052b0e03021a05000414",
			["sha224"] = "302d300d06096086480165030402040500041c",
			["sha256"] = "3031300d060960864801650304020105000420",
			["sha384"] = "3041300d060960864801650304020205000430",
			["sha512"] = "3051300d060960864801650304020305000440",
			["md2"] = "3020300c06082a864886f70d020205000410",
			["md5"] = "3020300c06082a864886f70d020505000410",
			["ripemd160"] = "3021300906052b2403020105000414",
		});
		public static readonly IDictionary<string, string> DEFAULTPROVIDER = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
		{
			["md5"] = "cryptojs",
			["sha1"] = "cryptojs",
			["sha224"] = "cryptojs",
			["sha256"] = "cryptojs",
			["sha384"] = "cryptojs",
			["sha512"] = "cryptojs",
			["ripemd160"] = "cryptojs",
			["hmacmd5"] = "cryptojs",
			["hmacsha1"] = "cryptojs",
			["hmacsha224"] = "cryptojs",
			["hmacsha256"] = "cryptojs",
			["hmacsha384"] = "cryptojs",
			["hmacsha512"] = "cryptojs",
			["hmacripemd160"] = "cryptojs",
			["MD5withRSA"] = "cryptojs/jsrsa",
			["SHA1withRSA"] = "cryptojs/jsrsa",
			["SHA224withRSA"] = "cryptojs/jsrsa",
			["SHA256withRSA"] = "cryptojs/jsrsa",
			["SHA384withRSA"] = "cryptojs/jsrsa",
			["SHA512withRSA"] = "cryptojs/jsrsa",
			["RIPEMD160withRSA"] = "cryptojs/jsrsa",
			["MD5withECDSA"] = "cryptojs/jsrsa",
			["SHA1withECDSA"] = "cryptojs/jsrsa",
			["SHA224withECDSA"] = "cryptojs/jsrsa",
			["SHA256withECDSA"] = "cryptojs/jsrsa",
			["SHA384withECDSA"] = "cryptojs/jsrsa",
			["SHA512withECDSA"] = "cryptojs/jsrsa",
			["RIPEMD160withECDSA"] = "cryptojs/jsrsa",
			["SHA1withDSA"] = "cryptojs/jsrsa",
			["SHA224withDSA"] = "cryptojs/jsrsa",
			["SHA256withDSA"] = "cryptojs/jsrsa",
			["MD5withRSAandMGF1"] = "cryptojs/jsrsa",
			["SHA1withRSAandMGF1"] = "cryptojs/jsrsa",
			["SHA224withRSAandMGF1"] = "cryptojs/jsrsa",
			["SHA256withRSAandMGF1"] = "cryptojs/jsrsa",
			["SHA384withRSAandMGF1"] = "cryptojs/jsrsa",
			["SHA512withRSAandMGF1"] = "cryptojs/jsrsa",
			["RIPEMD160withRSAandMGF1"] = "cryptojs/jsrsa",
		});
		public static readonly IDictionary<string, string> CRYPTOJSMESSAGEDIGESTNAME = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
		{
			["md5"] = "CryptoJS.algo.MD5",
			["sha1"] = "CryptoJS.algo.SHA1",
			["sha224"] = "CryptoJS.algo.SHA224",
			["sha256"] = "CryptoJS.algo.SHA256",
			["sha384"] = "CryptoJS.algo.SHA384",
			["sha512"] = "CryptoJS.algo.SHA512",
			["ripemd160"] = "CryptoJS.algo.RIPEMD160",
		});
		public static string getDigestInfoHex(string n, string alg)
		{
			if (!Util.DIGESTINFOHEAD.ContainsKey(alg))
				throw new ArgumentException("alg not supported in Util.DIGESTINFOHEAD: " + alg, nameof(alg));
			return Util.DIGESTINFOHEAD[alg] + n;
		}
		public static string getPaddedDigestInfoHex(string n, string alg, int keyLen)
		{
			var diHex = Util.getDigestInfoHex(n, alg);
			var fullLength = keyLen / 4;
			if (diHex.Length + 22 > fullLength)
				throw new Exception("key is too short for SigAlg: keylen=" + keyLen + "," + alg);
			var startPad = "0001";
			var endPadAndDIHex = "00" + diHex;
			var padding = "";
			var padLength = fullLength - startPad.Length - endPadAndDIHex.Length;
			for (var padIdx = 0; padIdx < padLength; padIdx += 2)
				padding += "ff";
			return startPad + padding + endPadAndDIHex;
		}
		public static string hashString(string n, string t)
		{
			var i = new KJUR.crypto.MessageDigest(alg: t);
			return i.digestString(n);
		}
		public static string hashHex(string n, string t)
		{
			var i = new KJUR.crypto.MessageDigest(alg: t);
			return i.digestHex(n);
		}
		public static string sha1(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "sha1", prov: "cryptojs");
			return t.digestString(n);
		}
		public static string sha256(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "sha256", prov: "cryptojs");
			return t.digestString(n);
		}
		public static string sha256Hex(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "sha256", prov: "cryptojs");
			return t.digestHex(n);
		}
		public static string sha512(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "sha512", prov: "cryptojs");
			return t.digestString(n);
		}
		public static string sha512Hex(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "sha512", prov: "cryptojs");
			return t.digestHex(n);
		}
		public static string md5(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "md5", prov: "cryptojs");
			return t.digestString(n);
		}
		public static string ripemd160(string n)
		{
			var t = new KJUR.crypto.MessageDigest(alg: "ripemd160", prov: "cryptojs");
			return t.digestString(n);
		}
		public static void getCryptoJSMDByName()
		{
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
