namespace Art.OnShift.Scheduler.Middlewares
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUserLocalTime(this DateTime utcDateTime, TimeZoneInfo userTimeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);
        }

        public static DateTime ToUtcFromUserLocal(this DateTime localDateTime, TimeZoneInfo userTimeZone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);
        }
    }
}
