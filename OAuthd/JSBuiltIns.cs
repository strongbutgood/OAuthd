using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
	static class JSBuiltIns
	{
		public static long Date_now()
		{
			var now = DateTime.UtcNow;
			/*if (TimeZoneInfo.Local.IsDaylightSavingTime(now))
			{
				var utc = TimeZoneInfo.ConvertTimeToUtc(now);
				var newNow = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"));
				return (long)(newNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
			}//*/
			return (long)(now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
		}
	}
}
