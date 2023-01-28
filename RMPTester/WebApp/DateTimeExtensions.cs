using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	public static class DateTimeExtensions
	{
		public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

		public static long ToEpoch(this DateTime utc) => (long)(utc - DateTimeExtensions.Epoch).TotalSeconds;
		public static long? ToEpoch(this DateTime? utc) => utc != null ? utc.Value.ToEpoch() : default;

		public static DateTime FromEpoch(this long seconds) => DateTimeExtensions.Epoch.AddSeconds(seconds);
		public static DateTime? FromEpoch(this long? seconds) => seconds != null ? seconds.Value.FromEpoch() : default;
	}
}
