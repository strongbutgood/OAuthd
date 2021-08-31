using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR
{
#pragma warning disable IDE1006 // Naming Styles
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

		internal static string b64map = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		internal static string b64pad = "=";
		public static string HexStringToBase64(this string hex)
		{
			var sb = new StringBuilder();
			int i, hexIdx;
			for (hexIdx = 0; hexIdx + 3 <= hex.Length; hexIdx += 3)
			{
				i = int.Parse(hex.Substring(hexIdx, 3), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(b64map[i >> 6]).Append(b64map[i & 63]);
			}
			if (hexIdx + 1 == hex.Length)
			{
				i = int.Parse(hex.Substring(hexIdx, 1), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(b64map[i << 2]);
			}
			else if (hexIdx + 2 == hex.Length)
			{
				i = int.Parse(hex.Substring(hexIdx, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				sb.Append(b64map[i >> 2]).Append(b64map[(i & 3) << 4]);
			}
			if(b64pad != null)
				while((sb.Length & 3)>0)
					sb.Append(b64pad);
			return sb.ToString();
		}
		internal static string BI_RM = "0123456789abcdefghijklmnopqrstuvwxyz";
		private static char int2char(int idx)
		{
			return BI_RM[idx];
		}
		public static string Base64ToHexString(this string base64)
		{
			var i = new StringBuilder();
			int t, u = 0, r = 0;
			for (var f = 0; f < base64.Length; ++f)
			{
				if (base64[f] == b64pad?.FirstOrDefault())
					break;
				t = b64map.IndexOf(base64[f]);
				if (t < 0)
					continue;
				if (r == 0)
				{
					i.Append(int2char(t >> 2));
					u = t & 3;
					r = 1;
				}
				else if (r == 1)
				{
					i.Append(int2char(u << 2 | t >> 4));
					u = t & 15;
					r = 2;
				}
				else if (r == 2)
				{
					i.Append(int2char(u)).Append(int2char(t >> 2));
					u = t & 3;
					r = 3;
				}
				else
				{
					i.Append(int2char(u << 2 | t >> 4)).Append(int2char(t & 15));
					r = 0;
				}
			}
			if (r == 1)
				i.Append(int2char(u << 2));
			return i.ToString();
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
			return base64.Replace("=", "").Replace('+', '-').Replace('/', '_');
		}




		public static string _Base64ToUtf8_(this string base64)
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
		}
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
#pragma warning restore IDE1006 // Naming Styles
}
