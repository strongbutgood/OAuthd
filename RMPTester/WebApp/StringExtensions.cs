using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	static class StringExtensions
	{
		public static string ToPlainString(this byte[] bytes)
		{
			var sb = new StringBuilder(bytes.Length * 2);
			for (var r = 0; r < bytes.Length; r++)
				sb.Append((char)bytes[r]);
			return sb.ToString();
		}

		public static string ToHexString(this byte[] bytes)
		{
			var sb = new StringBuilder(bytes.Length * 2);
			for (var r = 0; r < bytes.Length; r++)
				sb.Append(bytes[r].ToString("X2"));
			return sb.ToString();
		}

		public static byte[] StringToByteArray(this string text)
		{
			var bytes = new byte[text.Length];
			for (int i = 0; i < text.Length; i++)
			{
				bytes[i] = (byte)text[i];
			}
			return bytes;
		}

		public static byte[] Base64ToByteArray(this string base64)
		{
			var i = base64.Base64ToHexString();
			var r = new byte[i.Length / 2];
			for (var t = 0; 2 * t < i.Length; ++t)
				r[t] = byte.Parse(i.Substring(2 * t, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			return r;
		}

		private const string _b64map = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		private const string _b64pad = "=";

		public static string HexStringToBase64(this string hex)
		{
			var sb = new StringBuilder();
			int i, hexIdx;
			for (hexIdx = 0; hexIdx + 3 <= hex.Length; hexIdx += 3)
			{
				i = int.Parse(hex.Substring(hexIdx, 3), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(_b64map[i >> 6]).Append(_b64map[i & 63]);
			}
			if (hexIdx + 1 == hex.Length)
			{
				i = int.Parse(hex.Substring(hexIdx, 1), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(_b64map[i << 2]);
			}
			else if (hexIdx + 2 == hex.Length)
			{
				i = int.Parse(hex.Substring(hexIdx, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(_b64map[i >> 2]).Append(_b64map[(i & 3) << 4]);
			}
			if (_b64pad != null)
				while ((sb.Length & 3) > 0)
					sb.Append(_b64pad);
			return sb.ToString();
		}
		public static string Base64ToHexString(this string base64)
		{
			const string BI_RM = "0123456789abcdefghijklmnopqrstuvwxyz";
			char int2char(int idx)
			{
				return BI_RM[idx];
			}

			var sb = new StringBuilder();
			int bits1, bits2 = 0, mod4 = 0;
			for (var idx = 0; idx < base64.Length; ++idx)
			{
				if (base64[idx] == _b64pad?.FirstOrDefault())
					break;
				bits1 = _b64map.IndexOf(base64[idx]);
				if (bits1 < 0)
					continue;
				if (mod4 == 0)
				{
					sb.Append(int2char(bits1 >> 2));
					bits2 = bits1 & 0x3;
					mod4 = 1;
				}
				else if (mod4 == 1)
				{
					sb.Append(int2char(bits2 << 2 | bits1 >> 4));
					bits2 = bits1 & 0xF;
					mod4 = 2;
				}
				else if (mod4 == 2)
				{
					sb.Append(int2char(bits2))
						.Append(int2char(bits1 >> 2));
					bits2 = bits1 & 0x3;
					mod4 = 3;
				}
				else
				{
					sb.Append(int2char(bits2 << 2 | bits1 >> 4))
						.Append(int2char(bits1 & 0xF));
					mod4 = 0;
				}
			}
			if (mod4 == 1)
				sb.Append(int2char(bits2 << 2));
			return sb.ToString();
		}

		public static string Base64UrlToBase64(this string base64url)
		{
			if (base64url.Length % 4 == 2)
				base64url += "==";
			else if (base64url.Length % 4 == 3)
				base64url += "=";
			return base64url.Replace('-', '+').Replace('_', '/');
		}
		public static string Base64ToBase64Url(this string base64)
		{
			return base64.Replace('/', '_').Replace('+', '-').Replace("=", "");
		}




		/*public static string _Base64ToUtf8_(this string base64)
		{
			var buffer = Convert.FromBase64String(base64);
			var utf8 = Encoding.UTF8.GetString(buffer);
			return utf8;
		}
		public static string _Utf8ToBase64_(this string utf8)
		{
			var buffer = Encoding.UTF8.GetBytes(utf8);
			var base64 = Convert.ToBase64String(buffer);
			return base64;
		}//*/
		public static string Base64UrlToUtf8(this string base64url)
		{
			var hex = base64url.Base64UrlToBase64().Base64ToHexString();
			var encodedHex = System.Text.RegularExpressions.Regex.Replace(hex, "(..)", "%$1");
			return Uri.UnescapeDataString(encodedHex);
		}
		public static string Utf8ToBase64Url(this string utf8)
		{
			var sb = new StringBuilder();
			var escapedUtf8 = Uri.EscapeDataString(utf8);
			for (var t = 0; t < escapedUtf8.Length; t++)
			{
				if (escapedUtf8[t] == '%')
				{
					sb.Append(escapedUtf8.Substring(t, 3));
					t += 2;
				}
				else
				{
					sb.Append("%").Append(escapedUtf8[t].ToString().StringToByteArray().ToHexString());
				}
			}
			var encodedHex = sb.ToString();
			return encodedHex.Replace("%", "").HexStringToBase64().Base64ToBase64Url();
		}
	}
}
