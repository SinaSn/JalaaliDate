using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace JalaaliDate;

/// <summary>
/// Created By SinaSn, sinasn1996@gmail.com
/// </summary>
[Serializable]
public struct JalaaliDateTime :
    ISerializable, IFormattable, IConvertible,
    IComparable<JalaaliDateTime>, IComparable<DateTime>,
    IEquatable<JalaaliDateTime>, IEquatable<DateTime>
{
    #region properties and fields

    private static PersianCalendar? _persianCalendar;

    private static PersianCalendar PersianCalendar
    {
        get
        {
            if (_persianCalendar != null) return _persianCalendar;
            _persianCalendar = new PersianCalendar();
            return _persianCalendar;
        }
    }

    private readonly DateTime _dateTime;

    /// <summary>
    /// سال شمسی
    /// </summary>
    public int Year => _dateTime <= DateTime.MinValue ? DateTime.MinValue.Year : PersianCalendar.GetYear(_dateTime);

    /// <summary>
    /// ماه شمسی
    /// </summary>
    public int Month => _dateTime <= DateTime.MinValue ? DateTime.MinValue.Month : PersianCalendar.GetMonth(_dateTime);

    /// <summary>
    /// نام فارسی ماه
    /// </summary>
    public string MonthName => ((JalaaliDateTimeMonthEnum)Month).ToString();

    /// <summary>
    /// روز ماه
    /// </summary>
    public int Day => _dateTime <= DateTime.MinValue ? DateTime.MinValue.Day : PersianCalendar.GetDayOfMonth(_dateTime);

    /// <summary>
    /// روز هفته
    /// </summary>
    public DayOfWeek DayOfWeek => _dateTime <= DateTime.MinValue ? DateTime.MinValue.DayOfWeek : PersianCalendar.GetDayOfWeek(_dateTime);

    /// <summary>
    /// روز هفته یا ایندکس شمسی
    /// <para />
    /// شنبه دارای ایندکس صفر است
    /// </summary>
    public JalaaliDayOfWeek PersianDayOfWeek
    {
        get
        {
            var dayOfWeek = PersianCalendar.GetDayOfWeek(_dateTime);
            JalaaliDayOfWeek persianDayOfWeek = dayOfWeek switch
            {
                DayOfWeek.Sunday => JalaaliDayOfWeek.Sunday,
                DayOfWeek.Monday => JalaaliDayOfWeek.Monday,
                DayOfWeek.Tuesday => JalaaliDayOfWeek.Tuesday,
                DayOfWeek.Wednesday => JalaaliDayOfWeek.Wednesday,
                DayOfWeek.Thursday => JalaaliDayOfWeek.Thursday,
                DayOfWeek.Friday => JalaaliDayOfWeek.Friday,
                DayOfWeek.Saturday => JalaaliDayOfWeek.Saturday,
                _ => throw new ArgumentOutOfRangeException()
            };

            return persianDayOfWeek;
        }
    }

    /// <summary>
    /// ساعت
    /// </summary>
    public int Hour => _dateTime <= DateTime.MinValue ? 12 : PersianCalendar.GetHour(_dateTime);

    /// <summary>
    /// ساعت دو رقمی
    /// </summary>
    public int ShortHour => Hour > 12 ? Hour - 12 : Hour;

    /// <summary>
    /// دقیقه
    /// </summary>
    public int Minute => _dateTime <= DateTime.MinValue ? 0 : PersianCalendar.GetMinute(_dateTime);

    /// <summary>
    /// ثانیه
    /// </summary>
    public int Second => _dateTime <= DateTime.MinValue ? 0 : PersianCalendar.GetSecond(_dateTime);

    /// <summary>
    /// میلی ثانیه
    /// </summary>
    public int Millisecond => _dateTime <= DateTime.MinValue ? 0 : (int)PersianCalendar.GetMilliseconds(_dateTime);

    /// <summary>
    /// تعداد روز در ماه
    /// </summary>
    public int GetMonthDays
    {
        get
        {
            var days = Month switch
            {
                1 => 31,
                2 => 31,
                3 => 31,
                4 => 31,
                5 => 31,
                6 => 31,
                7 => 30,
                8 => 30,
                9 => 30,
                10 => 30,
                11 => 30,
                12 => IsLeapYear ? 30 : 29,
                _ => throw new ArgumentOutOfRangeException()
            };

            return days;
        }
    }

    /// <summary>
    /// هفته چندم سال
    /// </summary>
    public int GetWeekOfYear => _dateTime <= DateTime.MinValue ? 0 : PersianCalendar.GetWeekOfYear(_dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);

    /// <summary>
    /// هفته چندم ماه
    /// </summary>
    public int GetWeekOfMonth
    {
        get
        {
            if (_dateTime <= DateTime.MinValue) return 0;
            var persianDateTime = AddDays(1 - Day);
            return GetWeekOfYear - persianDateTime.GetWeekOfYear + 1;
        }
    }

    /// <summary>
    /// روز چندم سال
    /// </summary>
    public int GetDayOfYear => _dateTime <= DateTime.MinValue ? 0 : PersianCalendar.GetDayOfYear(_dateTime);

    /// <summary>
    /// آیا سال کبیسه است؟
    /// </summary>
    public bool IsLeapYear => _dateTime > DateTime.MinValue && PersianCalendar.IsLeapYear(Year);

    /// <summary>
    /// قبل از ظهر، بعد از ظهر
    /// </summary>
    private AmPmEnum PersianAmPm => _dateTime.ToString("tt") == "PM" ? AmPmEnum.PM : AmPmEnum.AM;

    /// <summary>
    /// قبل از ظهر، بعد از ظهر به شکل مخفف . کوتاه
    /// </summary>
    public string GetPersianAmPm
    {
        get
        {
            var result = PersianAmPm switch
            {
                AmPmEnum.AM => "ق.ظ",
                AmPmEnum.PM => "ب.ظ",
                _ => string.Empty
            };

            return result;
        }
    }

    /// <summary>
    /// نام کامل ماه
    /// </summary>
    public string GetLongMonthName => GetPersianMonthNamePrivate(Month);

    /// <summary>
    /// سال دو رقمی
    /// </summary>
    public int GetShortYear => Year % 100;

    /// <summary>
    /// نام کامل روز
    /// </summary>
    public string? GetLongDayOfWeekName
    {
        get
        {
            string? weekDayName = null;
            switch (DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    weekDayName = PersianWeekDaysStruct.یکشنبه.Value;
                    break;

                case DayOfWeek.Monday:
                    weekDayName = PersianWeekDaysStruct.دوشنبه.Value;
                    break;

                case DayOfWeek.Tuesday:
                    weekDayName = PersianWeekDaysStruct.سه_شنبه.Value;
                    break;

                case DayOfWeek.Wednesday:
                    weekDayName = PersianWeekDaysStruct.چهارشنبه.Value;
                    break;

                case DayOfWeek.Thursday:
                    weekDayName = PersianWeekDaysStruct.پنجشنبه.Value;
                    break;

                case DayOfWeek.Friday:
                    weekDayName = PersianWeekDaysStruct.جمعه.Value;
                    break;

                case DayOfWeek.Saturday:
                    weekDayName = PersianWeekDaysStruct.شنبه.Value;
                    break;
            }

            return weekDayName;
        }
    }

    /// <summary>
    /// نام یک حرفی روز، حرف اول روز
    /// </summary>
    public string? GetShortDayOfWeekName
    {
        get
        {
            string? weekDayName = null;
            switch (DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    weekDayName = "ی";
                    break;

                case DayOfWeek.Monday:
                    weekDayName = "د";
                    break;

                case DayOfWeek.Tuesday:
                    weekDayName = "س";
                    break;

                case DayOfWeek.Wednesday:
                    weekDayName = "چ";
                    break;

                case DayOfWeek.Thursday:
                    weekDayName = "پ";
                    break;

                case DayOfWeek.Friday:
                    weekDayName = "ج";
                    break;

                case DayOfWeek.Saturday:
                    weekDayName = "ش";
                    break;
            }

            return weekDayName;
        }
    }

    /// <summary>
    /// تاریخ و زمان همین الان
    /// </summary>
    public static JalaaliDateTime Now => new(DateTime.Now);

    /// <summary>
    /// تاریخ امروز
    /// </summary>
    public static JalaaliDateTime Today => new(DateTime.Today);

    /// <summary>
    /// زمان به فرمتی مشابه 
    /// <para />
    /// 13:47:40:530
    /// </summary>
    public string TimeOfDay
    {
        get
        {
            //if (_dateTime <= DateTime.MinValue) return null;
            var result = $"{Hour:00}:{Minute:00}:{Second:00}:{Millisecond:000}";
            return result;
        }
    }

    /// <summary>
    /// زمان به فرمتی مشابه زیر 
    /// <para />
    /// ساعت 01:47:40:530 ب.ظ
    /// </summary>
    public string LongTimeOfDay
    {
        get
        {
            //if (_dateTime <= DateTime.MinValue) return null;
            var result = $"ساعت {ShortHour:00}:{Minute:00}:{Second:00}:{Millisecond:000} {GetPersianAmPm}";
            return result;
        }
    }

    /// <summary>
    /// زمان به فرمتی مشابه زیر
    /// <para />
    /// 01:47:40 ب.ظ
    /// </summary>
    public string ShortTimeOfDay
    {
        get
        {
            //if (_dateTime <= DateTime.MinValue) return null;
            var result = $"{ShortHour:00}:{Minute:00}:{Second:00} {GetPersianAmPm}";
            return result;
        }
    }

    /// <summary>
    /// تاریخ بدون احتساب زمان
    /// </summary>
    public JalaaliDateTime Date
    {
        get
        {
            var persianDateTime = new JalaaliDateTime(Year, Month, Day);
            return persianDateTime;
        }
    }

    /// <summary>
    /// حداقل مقدار
    /// </summary>
    public static JalaaliDateTime MinValue => new(DateTime.MinValue);

    /// <summary>
    /// حداکثر مقدار
    /// </summary>
    public static JalaaliDateTime MaxValue => new(DateTime.MaxValue);

    #endregion

    #region ctor

    /// <summary>
    /// متد سازنده برای دی سریالایز شدن
    /// </summary>
    private JalaaliDateTime(SerializationInfo info, StreamingContext context)
        : this()
    {
        _dateTime = info.GetDateTime("DateTime");
    }

    /// <summary>
    /// مقدار دهی اولیه با استفاده از دیت تایم میلادی
    /// </summary>
    public JalaaliDateTime(DateTime dateTime)
        : this()
    {
        _dateTime = dateTime;
    }

    /// <summary>
    /// مقدار دهی اولیه با استفاده از دیت تایم قابل نال میلادی
    /// </summary>
    public JalaaliDateTime(DateTime? nullableDateTime)
        : this()
    {
        if (!nullableDateTime.HasValue)
        {
            _dateTime = DateTime.MinValue;
            return;
        }

        _dateTime = nullableDateTime.Value;
    }

    /// <summary>
    /// مقدار دهی اولیه
    /// </summary>
    /// <param name="persianYear">سال شمسی</param>
    /// <param name="persianMonth">ماه شمسی</param>
    /// <param name="persianDay">روز شمسی</param>
    public JalaaliDateTime(int persianYear, int persianMonth, int persianDay)
        : this()
    {
        _dateTime = PersianCalendar.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);
    }

    /// <summary>
    /// مقدار دهی اولیه
    /// </summary>
    /// <param name="persianYear">سال شمسی</param>
    /// <param name="persianMonth">ماه شمسی</param>
    /// <param name="persianDay">روز شمسی</param>
    /// <param name="hour">ساعت</param>
    /// <param name="minute">دقیقه</param>
    /// <param name="second">ثانیه</param>
    public JalaaliDateTime(int persianYear, int persianMonth, int persianDay, int hour, int minute, int second)
        : this()
    {
        _dateTime = PersianCalendar.ToDateTime(persianYear, persianMonth, persianDay, hour, minute, second, 0);
    }

    /// <summary>
    /// مقدار دهی اولیه
    /// </summary>
    /// <param name="persianYear">سال شمسی</param>
    /// <param name="persianMonth">ماه شمسی</param>
    /// <param name="persianDay">روز شمسی</param>
    /// <param name="hour">سال</param>
    /// <param name="minute">دقیقه</param>
    /// <param name="second">ثانیه</param>
    /// <param name="millisecond">میلی ثانیه</param>
    public JalaaliDateTime(int persianYear, int persianMonth, int persianDay, int hour, int minute, int second, int millisecond)
        : this()
    {
        _dateTime = PersianCalendar.ToDateTime(persianYear, persianMonth, persianDay, hour, minute, second, millisecond);
    }

    #endregion

    #region Types

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private enum AmPmEnum
    {
        AM = 0,
        PM = 1,
        None = 2
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    private enum JalaaliDateTimeMonthEnum
    {
        فروردین = 1,
        اردیبهشت = 2,
        خرداد = 3,
        تیر = 4,
        مرداد = 5,
        شهریور = 6,
        مهر = 7,
        آبان = 8,
        آذر = 9,
        دی = 10,
        بهمن = 11,
        اسفند = 12
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private struct PersianWeekDaysStruct
    {
        public static KeyValuePair<int, string> شنبه => new KeyValuePair<int, string>((int)DayOfWeek.Saturday, "شنبه");
        public static KeyValuePair<int, string> یکشنبه => new KeyValuePair<int, string>((int)DayOfWeek.Sunday, "یکشنبه");
        public static KeyValuePair<int, string> دوشنبه => new KeyValuePair<int, string>((int)DayOfWeek.Monday, "دوشنبه");
        public static KeyValuePair<int, string> سه_شنبه => new KeyValuePair<int, string>((int)DayOfWeek.Tuesday, "سه شنبه");
        public static KeyValuePair<int, string> چهارشنبه => new KeyValuePair<int, string>((int)DayOfWeek.Thursday, "چهارشنبه");
        public static KeyValuePair<int, string> پنجشنبه => new KeyValuePair<int, string>((int)DayOfWeek.Wednesday, "پنج شنبه");
        public static KeyValuePair<int, string> جمعه => new KeyValuePair<int, string>((int)DayOfWeek.Friday, "جمعه");
    }

    #endregion

    #region override

    /// <summary>
    /// تبدیل تاریخ به رشته با فرمت مشابه زیر
    /// <para />
    /// 1393/09/14 13:49:40
    /// </summary>
    public override string ToString()
    {
        return ToString("");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (!(obj is JalaaliDateTime persianDateTime)) return false;
        return _dateTime == persianDateTime.ToDateTime();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _dateTime.GetHashCode();
    }

    /// <summary>
    /// مقایسه با تاریخ دیگر
    /// </summary>
    /// <returns>مقدار بازگشتی همانند مقدار بازگشتی متد کامپیر در دیت تایم دات نت است</returns>
    public int CompareTo(JalaaliDateTime otherPersianDateTime)
    {
        return _dateTime.CompareTo(otherPersianDateTime.ToDateTime());
    }

    /// <summary>
    /// مقایسه با تاریخ دیگر
    /// </summary>
    /// <returns>مقدار بازگشتی همانند مقدار بازگشتی متد کامپیر در دیت تایم دات نت است</returns>
    public int CompareTo(DateTime otherDateTime)
    {
        return _dateTime.CompareTo(otherDateTime);
    }

    #region operators

    /// <summary>
    /// تبدیل خودکار به دیت تایم میلادی
    /// </summary>
    public static implicit operator DateTime(JalaaliDateTime persianDateTime)
    {
        return persianDateTime.ToDateTime();
    }

    /// <summary>
    /// اپراتور برابر
    /// </summary>
    public static bool operator ==(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return persianDateTime1.Equals(persianDateTime2);
    }

    /// <summary>
    /// اپراتور نامساوی
    /// </summary>
    public static bool operator !=(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return !persianDateTime1.Equals(persianDateTime2);
    }

    /// <summary>
    /// اپراتور بزرگتری
    /// </summary>
    public static bool operator >(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return persianDateTime1.ToDateTime() > persianDateTime2.ToDateTime();
    }

    /// <summary>
    /// اپراتور کوچکتری
    /// </summary>
    public static bool operator <(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return persianDateTime1.ToDateTime() < persianDateTime2.ToDateTime();
    }

    /// <summary>
    /// اپراتور بزرگتر مساوی
    /// </summary>
    public static bool operator >=(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return persianDateTime1.ToDateTime() >= persianDateTime2.ToDateTime();
    }

    /// <summary>
    /// اپراتور کوچکتر مساوی
    /// </summary>
    public static bool operator <=(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        return persianDateTime1.ToDateTime() <= persianDateTime2.ToDateTime();
    }

    /// <summary>
    /// اپراتور جمع تو زمان
    /// </summary>
    public static JalaaliDateTime operator +(JalaaliDateTime persianDateTime1, TimeSpan timeSpanToAdd)
    {
        DateTime dateTime1 = persianDateTime1;
        return new JalaaliDateTime(dateTime1.Add(timeSpanToAdd));
    }

    /// <summary>
    /// اپراتور کم کردن دو زمان از هم
    /// </summary>
    public static TimeSpan operator -(JalaaliDateTime persianDateTime1, JalaaliDateTime persianDateTime2)
    {
        DateTime dateTime1 = persianDateTime1;
        DateTime dateTime2 = persianDateTime2;
        return dateTime1 - dateTime2;
    }

    #endregion

    #endregion

    #region ISerializable

    /// <inheritdoc />
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("DateTime", ToDateTime());
    }

    #endregion

    #region IComparable

    /// <inheritdoc />
    public bool Equals(JalaaliDateTime other)
    {
        return Year == other.Year && Month == other.Month && Day == other.Day &&
               Hour == other.Hour && Minute == other.Minute && Second == other.Second && Millisecond == other.Millisecond;
    }

    /// <inheritdoc />
    public bool Equals(DateTime other)
    {
        return _dateTime == other;
    }

    #endregion

    #region Methods

    /// <summary>
    /// تاریخ شروع ماه رمضان 
    /// <para />
    /// چون ممکن است در یک سال شمسی دو شروع ماه رمضان داشته باشیم 
    /// <para />
    /// مقدار بازگشتی آرایه است که حداکثر دو آیتم دارد 
    /// </summary>
    public JalaaliDateTime[] GetStartDayOfRamadan(int hijriAdjustment)
    {
        var result = new List<JalaaliDateTime>();
        var hijriCalendar = new HijriCalendar { HijriAdjustment = hijriAdjustment };

        var currentHijriYear = hijriCalendar.GetYear(_dateTime);

        var startDayOfRamadan1 = new JalaaliDateTime(hijriCalendar.ToDateTime(currentHijriYear, 9, 1, 0, 0, 0, 0));
        result.Add(startDayOfRamadan1);

        var startDayOfRamadan2 = new JalaaliDateTime(hijriCalendar.ToDateTime(++currentHijriYear, 9, 1, 0, 0, 0, 0));
        if (startDayOfRamadan1.Year == startDayOfRamadan2.Year)
            result.Add(startDayOfRamadan2);

        return result.ToArray();
    }

    /// <summary>
    /// تاریخ آخرین روز ماه شمسی
    /// </summary>
    public JalaaliDateTime GetJalaaliDateOfLastDayOfMonth()
    {
        return new JalaaliDateTime(Year, Month, GetMonthDays);
    }

    /// <summary>
    /// تاریخ آخرین روز سال شمسی
    /// </summary>
    public JalaaliDateTime GetJalaaliDateOfLastDayOfYear()
    {
        return new JalaaliDateTime(Year, 12, IsLeapYear ? 30 : 29);
    }

    /// <summary>
    /// پارس کردن رشته و تبدیل به نوع PersianDateTime
    /// </summary>
    /// <param name="persianDateTimeInString">متنی که باید پارس شود</param>
    /// <param name="dateSeparatorPattern">کاراکتری که جدا کننده تاریخ ها است</param>
    public static JalaaliDateTime Parse(string persianDateTimeInString, string dateSeparatorPattern = @"\/|-")
    {
        //Convert persian and arabic digit to english to avoid throwing exception in Parse method
        persianDateTimeInString = Helpers.ConvertDigitsToLatin(persianDateTimeInString);

        string month = "",
            year,
            day,
            hour = "0",
            minute = "0",
            second = "0",
            milliseconds = "0";
        var amPmEnum = AmPmEnum.None;
        var containMonthSeparator = Regex.IsMatch(persianDateTimeInString, dateSeparatorPattern);

        persianDateTimeInString = ToEnglishNumber(persianDateTimeInString.Replace("&nbsp;", " ").Replace(" ", "-").Replace("\\", "-"))!;
        persianDateTimeInString = Regex.Replace(persianDateTimeInString, dateSeparatorPattern, "-");
        persianDateTimeInString = persianDateTimeInString.Replace("ك", "ک").Replace("ي", "ی");

        persianDateTimeInString = $"-{persianDateTimeInString}-";

        // بدست آوردن ب.ظ یا ق.ظ
        if (persianDateTimeInString.IndexOf("ق.ظ", StringComparison.InvariantCultureIgnoreCase) > -1)
            amPmEnum = AmPmEnum.AM;
        else if (persianDateTimeInString.IndexOf("ب.ظ", StringComparison.InvariantCultureIgnoreCase) > -1)
            amPmEnum = AmPmEnum.PM;

        if (persianDateTimeInString.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) > -1) // رشته ورودی شامل ساعت نیز هست
        {
            persianDateTimeInString = Regex.Replace(persianDateTimeInString, @"-*:-*", ":");
            hour = Regex.Match(persianDateTimeInString, @"(?<=-)\d{1,2}(?=:)", RegexOptions.IgnoreCase).Value;
            minute = Regex.Match(persianDateTimeInString, @"(?<=-\d{1,2}:)\d{1,2}(?=:?)", RegexOptions.IgnoreCase).Value;
            if (persianDateTimeInString.IndexOf(':') != persianDateTimeInString.LastIndexOf(':'))
            {
                second = Regex.Match(persianDateTimeInString, @"(?<=-\d{1,2}:\d{1,2}:)\d{1,2}(?=(\d{1,2})?)", RegexOptions.IgnoreCase).Value;
                milliseconds = Regex.Match(persianDateTimeInString, @"(?<=-\d{1,2}:\d{1,2}:\d{1,2}:)\d{1,4}(?=(\d{1,2})?)", RegexOptions.IgnoreCase).Value;
                if (string.IsNullOrEmpty(milliseconds)) milliseconds = "0";
            }
        }

        if (containMonthSeparator)
        {
            // بدست آوردن ماه
            month = Regex.Match(persianDateTimeInString, @"(?<=\d{2,4}-)\d{1,2}(?=-\d{1,2}-\d{1,2}(?!-\d{1,2}:))", RegexOptions.IgnoreCase).Value;
            if (string.IsNullOrEmpty(month))
                month = Regex.Match(persianDateTimeInString, @"(?<=\d{2,4}-)\d{1,2}(?=-\d{1,2}[^:])", RegexOptions.IgnoreCase).Value;

            // بدست آوردن روز
            day = Regex.Match(persianDateTimeInString, @"(?<=\d{2,4}-\d{1,2}-)\d{1,2}(?=-)", RegexOptions.IgnoreCase).Value;

            // بدست آوردن سال
            year = Regex.Match(persianDateTimeInString, @"(?<=-)\d{2,4}(?=-\d{1,2}-\d{1,2})", RegexOptions.IgnoreCase).Value;
        }
        else
        {
            foreach (JalaaliDateTimeMonthEnum item in Enum.GetValues(typeof(JalaaliDateTimeMonthEnum)))
            {
                var itemValueInString = item.ToString();
                if (!persianDateTimeInString.Contains(itemValueInString)) continue;
                month = ((int)item).ToString();
                break;
            }

            if (string.IsNullOrEmpty(month))
                throw new Exception("عدد یا حرف ماه در رشته ورودی وجود ندارد");

            // بدست آوردن روز
            var dayMatch = Regex.Match(persianDateTimeInString, @"(?<=-)\d{1,2}(?=-)", RegexOptions.IgnoreCase);
            if (dayMatch.Success)
            {
                day = dayMatch.Value;
                persianDateTimeInString = Regex.Replace(persianDateTimeInString, $"(?<=-){day}(?=-)", "");
            }
            else
                throw new Exception("عدد روز در رشته ورودی وجود ندارد");

            // بدست آوردن سال
            var yearMatch = Regex.Match(persianDateTimeInString, @"(?<=-)\d{4}(?=-)", RegexOptions.IgnoreCase);
            if (yearMatch.Success)
                year = yearMatch.Value;
            else
            {
                yearMatch = Regex.Match(persianDateTimeInString, @"(?<=-)\d{2,4}(?=-)", RegexOptions.IgnoreCase);
                if (yearMatch.Success)
                    year = yearMatch.Value;
                else
                    throw new Exception("عدد سال در رشته ورودی وجود ندارد");
            }
        }

        //if (year.Length <= 2 && year[0] == '9') year = string.Format("13{0}", year);
        //else if (year.Length <= 2) year = string.Format("14{0}", year);

        var numericYear = int.Parse(year);
        var numericMonth = int.Parse(month);
        var numericDay = int.Parse(day);
        var numericHour = int.Parse(hour);
        var numericMinute = int.Parse(minute);
        var numericSecond = int.Parse(second);
        var numericMilliseconds = int.Parse(milliseconds);

        if (numericYear < 100)
            numericYear += 1300;

        switch (amPmEnum)
        {
            case AmPmEnum.PM:
                if (numericHour < 12)
                    numericHour = numericHour + 12;
                break;
            case AmPmEnum.AM:
            case AmPmEnum.None:
                break;
        }

        return new JalaaliDateTime(numericYear, numericMonth, numericDay, numericHour, numericMinute, numericSecond, numericMilliseconds);
    }

    /// <summary>
    /// پارس کردن یک رشته برای یافتن تاریخ شمسی
    /// </summary>
    public static bool TryParse(string persianDateTimeInString, out JalaaliDateTime result, string dateSeparatorPattern = @"\/|-")
    {
        if (string.IsNullOrEmpty(persianDateTimeInString))
        {
            result = MinValue;
            return false;
        }

        try
        {
            result = Parse(persianDateTimeInString, dateSeparatorPattern);
            return true;
        }
        catch
        {
            result = MinValue;
            return false;
        }
    }

    /// <summary>
    /// پارس کردن عددی در فرمت تاریخ شمسی
    /// <para />
    /// همانند 13920305
    /// </summary>
    public static JalaaliDateTime Parse(int numericPersianDate)
    {
        if (numericPersianDate.ToString().Length != 8)
            throw new InvalidCastException("Numeric persian date must have a format like 13920101.");
        var year = numericPersianDate / 10000;
        var day = numericPersianDate % 100;
        var month = numericPersianDate / 100 % 100;
        return new JalaaliDateTime(year, month, day);
    }

    /// <summary>
    /// پارس کردن عددی در فرمت تاریخ شمسی
    /// <para />
    /// همانند 13920305
    /// </summary>
    public static bool TryParse(int numericPersianDate, out JalaaliDateTime result)
    {
        try
        {
            result = Parse(numericPersianDate);
            return true;
        }
        catch
        {
            result = MinValue;
            return false;
        }
    }

    /// <summary>
    /// پارس کردن عددی در فرمت تاریخ و زمان شمسی
    /// <para />
    /// همانند 13961223072132004
    /// </summary>
    public static JalaaliDateTime Parse(long numericPersianDateTime)
    {
        if (numericPersianDateTime.ToString().Length != 17)
            throw new InvalidCastException("Numeric persian date time must have a format like 1396122310223246.");
        var year = numericPersianDateTime / 10000000000000;
        var month = numericPersianDateTime / 100000000000 % 100;
        var day = numericPersianDateTime / 1000000000 % 100;
        var hour = numericPersianDateTime / 10000000 % 100;
        var minute = numericPersianDateTime / 100000 % 100;
        var second = numericPersianDateTime / 1000 % 100;
        var millisecond = numericPersianDateTime % 1000;
        return new JalaaliDateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second, (int)millisecond);
    }

    /// <summary>
    /// پارس کردن عددی در فرمت تاریخ و زمان شمسی
    /// <para />
    /// همانند 13961223102232461
    /// </summary>
    public static bool TryParse(long numericPersianDateTime, out JalaaliDateTime result)
    {
        try
        {
            result = Parse(numericPersianDateTime);
            return true;
        }
        catch
        {
            result = MinValue;
            return false;
        }
    }

    private static readonly List<string> GregorianWeekDayNames = new() { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };

    private static readonly List<string> GregorianMonthNames = new() { "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" };

    private static readonly List<string> PmAm = new() { "pm", "am" };

    /// <summary>
    /// فرمت های که پشتیبانی می شوند
    /// <para />
    /// yyyy: سال چهار رقمی
    /// <para />
    /// yy: سال دو رقمی
    /// <para />
    /// MMMM: نام فارسی ماه
    /// <para />
    /// MM: عدد دو رقمی ماه
    /// <para />
    /// M: عدد یک رقمی ماه
    /// <para />
    /// dddd: نام فارسی روز هفته
    /// <para />
    /// dd: عدد دو رقمی روز ماه
    /// <para />
    /// d: عدد یک رقمی روز ماه
    /// <para />
    /// HH: ساعت دو رقمی با فرمت 00 تا 24
    /// <para />
    /// H: ساعت یک رقمی با فرمت 0 تا 24
    /// <para />
    /// hh: ساعت دو رقمی با فرمت 00 تا 12
    /// <para />
    /// h: ساعت یک رقمی با فرمت 0 تا 12
    /// <para />
    /// mm: عدد دو رقمی دقیقه
    /// <para />
    /// m: عدد یک رقمی دقیقه
    /// <para />
    /// ss: ثانیه دو رقمی
    /// <para />
    /// s: ثانیه یک رقمی
    /// <para />
    /// fff: میلی ثانیه 3 رقمی
    /// <para />
    /// ff: میلی ثانیه 2 رقمی
    /// <para />
    /// f: میلی ثانیه یک رقمی
    /// <para />
    /// tt: ب.ظ یا ق.ظ
    /// <para />
    /// t: حرف اول از ب.ظ یا ق.ظ
    /// </summary>
    public string ToString(string? format, IFormatProvider? fp = null)
    {
        if (string.IsNullOrEmpty(format)) format = "yyyy/MM/dd HH:mm:ss";
        var dateTimeString = format.Trim();

        dateTimeString = dateTimeString.Replace("yyyy", Year.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("yy", GetShortYear.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("MMMM", MonthName);
        dateTimeString = dateTimeString.Replace("MM", Month.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("M", Month.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("dddd", GetLongDayOfWeekName);
        dateTimeString = dateTimeString.Replace("dd", Day.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("d", Day.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("HH", Hour.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("H", Hour.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("hh", ShortHour.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("h", ShortHour.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("mm", Minute.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("m", Minute.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("ss", Second.ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("s", Second.ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("fff", Millisecond.ToString("000", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("ff", (Millisecond / 10).ToString("00", CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("f", (Millisecond / 100).ToString(CultureInfo.InvariantCulture));
        dateTimeString = dateTimeString.Replace("tt", GetPersianAmPm);
        dateTimeString = dateTimeString.Replace("t", GetPersianAmPm[0].ToString(CultureInfo.InvariantCulture));

        return dateTimeString;
    }

    /// <summary>
    /// بررسی میکند آیا تاریخ ورودی تاریخ میلادی است یا نه
    /// </summary>
    public static bool IsChristianDate(string inputString)
    {
        inputString = inputString.ToLower();
        bool result;

        foreach (var gregorianWeekDayName in GregorianWeekDayNames)
        {
            result = inputString.Contains(gregorianWeekDayName);
            if (result) return true;
        }

        foreach (var gregorianMonthName in GregorianMonthNames)
        {
            result = inputString.Contains(gregorianMonthName);
            if (result) return true;
        }

        foreach (var item in PmAm)
        {
            result = inputString.Contains(item);
            if (result) return true;
        }

        result = Regex.IsMatch(inputString, @"(1[8-9]|[2-9][0-9])\d{2}", RegexOptions.IgnoreCase);

        return result;
    }

    /// <summary>
    /// بررسی میکند آیا تاریخ ورودی مطابق  تاریخ اس کسو ال سرور می باشد یا نه
    /// </summary>
    public static bool IsSqlDateTime(DateTime dateTime)
    {
        var minSqlDateTimeValue = new DateTime(1753, 1, 1);
        return dateTime >= minSqlDateTimeValue;
    }

    /// <summary>
    /// تبدیل نام ماه شمسی به عدد معادل آن
    /// <para />
    /// به طور مثال آذر را به 9 تبدیل می کند
    /// </summary>
    public int GetMonthEnum(string longMonthName)
    {
        var monthEnum = (JalaaliDateTimeMonthEnum)Enum.Parse(typeof(JalaaliDateTimeMonthEnum), longMonthName);
        return (int)monthEnum;
    }

    /// <summary>
    /// نمایش تاریخ به فرمتی مشابه زیر
    /// <para />
    /// 1393/09/14
    /// </summary>
    public string ToShortDateString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result = $"{Year:0000}/{Month:00}/{Day:00}";
        return result;
    }

    /// <summary>
    /// نمایش تاریخ به صورت عدد و در فرمتی مشابه زیر
    /// <para />
    /// 13930914
    /// </summary>
    public int ToShortDateInt()
    {
        var result = $"{Year:0000}{Month:00}{Day:00}";
        return int.Parse(result);
    }

    /// <summary>
    /// نمایش تاریخ و ساعت تا دقت میلی ثانیه به صورت عدد
    /// <para />
    /// 1396122310324655
    /// </summary>
    public long ToLongDateTimeInt()
    {
        var result = $"{Year:0000}{Month:00}{Day:00}{Hour:00}{Minute:00}{Second:00}{Millisecond:000}";
        return long.Parse(result);
    }

    /// <summary>
    /// در این فرمت نمایش ساعت و دقیقه و ثانیه در کنار هم با حذف علامت : تبدیل به عدد می شوند و نمایش داده می شود
    /// <para />
    /// مثال: 123452 
    /// <para />
    /// که به معنای ساعت 12 و 34 دقیقه و 52 ثانیه می باشد
    /// </summary>
    public int ToTimeInt()
    {
        var result = $"{Hour:00}{Minute:00}{Second:00}";
        return int.Parse(result);
    }

    /// <summary>
    /// نمایش تاریخ به فرمتی مشابه زیر
    /// <para />
    /// جمعه، 14 آذر 1393
    /// </summary>
    public string ToLongDateString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result = $"{GetLongDayOfWeekName}، {Day:00} {GetLongMonthName} {Year:0000}";
        return result;
    }

    /// <summary>
    /// نمایش تاریخ و زمان به فرمتی مشابه زیر
    /// <para />
    /// جمعه، 14 آذر 1393 ساعت 13:50:27
    /// </summary>
    public string ToLongDateTimeString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result =
            $"{GetLongDayOfWeekName}، {Day:00} {GetLongMonthName} {Year:0000} ساعت {Hour:00}:{Minute:00}:{Second:00}";
        return result;
    }

    /// <summary>
    /// نمایش تاریخ و زمان به فرمتی مشابه زیر
    /// <para />
    /// جمعه، 14 آذر 1393 13:50
    /// </summary>
    public string ToShortDateTimeString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result = $"{GetLongDayOfWeekName}، {Day:00} {GetLongMonthName} {Year:0000} {Hour:00}:{Minute:00}";
        return result;
    }

    /// <summary>
    /// نمایش زمان به فرمتی مشابه زیر
    /// <para />
    /// 01:50 ب.ظ
    /// </summary>
    public string ToShortTimeString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result = $"{ShortHour:00}:{Minute:00} {GetPersianAmPm}";
        return result;
    }

    /// <summary>
    /// نمایش زمان به فرمتی مشابه زیر
    /// <para />
    /// 13:50:20
    /// </summary>
    public string ToLongTimeString()
    {
        //if (_dateTime <= DateTime.MinValue) return null;
        var result = $"{Hour:00}:{Minute:00}:{Second:00}";
        return result;
    }

    /// <summary>
    /// نمایش زمان به فرمتی مشابه زیر
    /// <para />
    /// 1 دقیقه قبل
    /// </summary>
    public string ElapsedTime()
    {
        //if (_dateTime <= DateTime.MinValue) return null;

        var persianDateTimeNow = new JalaaliDateTime(DateTime.Now);
        var timeSpan = persianDateTimeNow - _dateTime;
        if (timeSpan.TotalDays > 90)
            return ToShortDateTimeString();

        var result = string.Empty;
        if (timeSpan.TotalDays > 30)
        {
            var month = timeSpan.TotalDays / 30;
            if (month > 0)
                result = $"{month:0} ماه قبل";
        }
        else if (timeSpan.TotalDays >= 1)
        {
            result = $"{timeSpan.TotalDays:0} روز قبل";
        }
        else if (timeSpan.TotalHours >= 1)
        {
            result = $"{timeSpan.TotalHours:0} ساعت قبل";
        }
        else
        {
            var minute = timeSpan.TotalMinutes;
            if (minute <= 1) minute = 1;
            result = $"{minute:0} دقیقه قبل";
        }

        return result;
    }

    /// <summary>
    /// گرفتن فقط زمان 
    /// </summary>
    public TimeSpan GetTime()
    {
        return new TimeSpan(0, _dateTime.Hour, _dateTime.Minute, _dateTime.Second, _dateTime.Millisecond);
    }

    /// <summary>
    /// تنظیم کردن زمان
    /// </summary>
    public JalaaliDateTime SetTime(int hour, int minute, int second = 0, int milliseconds = 0)
    {
        return new JalaaliDateTime(Year, Month, Day, hour, minute, second, milliseconds);
    }

    /// <summary>
    /// تبدیل به تاریخ میلادی
    /// </summary>
    public DateTime ToDateTime()
    {
        return _dateTime;
    }

    /// <summary>
    /// کم کردن دو تاریخ از هم
    /// </summary>
    public TimeSpan Subtract(JalaaliDateTime persianDateTime)
    {
        return _dateTime - persianDateTime.ToDateTime();
    }

    /// <summary>
    /// تعداد ماه اختلافی با تاریخ دیگری را بر میگرداند
    /// </summary>
    /// <returns>تعداد ماه</returns>
    public int MonthDifference(DateTime dateTime)
    {
        return Math.Abs(dateTime.Month - _dateTime.Month + 12 * (dateTime.Year - _dateTime.Year));
    }

    /// <summary>
    /// اضافه کردن مدت زمانی به تاریخ
    /// </summary>
    public JalaaliDateTime Add(TimeSpan timeSpan)
    {
        return new JalaaliDateTime(_dateTime.Add(timeSpan));
    }

    /// <summary>
    /// اضافه کردن سال به تاریخ
    /// </summary>
    public JalaaliDateTime AddYears(int years)
    {
        return new JalaaliDateTime(PersianCalendar.AddYears(_dateTime, years));
    }

    /// <summary>
    /// اضافه کردن روز به تاریخ
    /// </summary>
    public JalaaliDateTime AddDays(int days)
    {
        return new JalaaliDateTime(PersianCalendar.AddDays(_dateTime, days));
    }

    /// <summary>
    /// اضافه کردن ماه به تاریخ
    /// </summary>
    public JalaaliDateTime AddMonths(int months)
    {
        return new JalaaliDateTime(PersianCalendar.AddMonths(_dateTime, months));
    }

    /// <summary>
    /// اضافه کردن ساعت به تاریخ
    /// </summary>
    public JalaaliDateTime AddHours(int hours)
    {
        return new JalaaliDateTime(_dateTime.AddHours(hours));
    }

    /// <summary>
    /// اضافه کردن دقیقه به تاریخ
    /// </summary>
    public JalaaliDateTime AddMinutes(int minutes)
    {
        return new JalaaliDateTime(_dateTime.AddMinutes(minutes));
    }

    /// <summary>
    /// اضافه کردن ثانیه به تاریخ
    /// </summary>
    public JalaaliDateTime AddSeconds(int seconds)
    {
        return new JalaaliDateTime(_dateTime.AddSeconds(seconds));
    }

    /// <summary>
    /// اضافه کردن میلی ثانیه به تاریخ
    /// </summary>
    public JalaaliDateTime AddMilliseconds(int milliseconds)
    {
        return new JalaaliDateTime(_dateTime.AddMilliseconds(milliseconds));
    }

    /// <summary>
    /// بدست آوردن تاریخ شمسی اولین روز هفته
    /// </summary>
    public JalaaliDateTime GetFirstDayOfWeek()
    {
        var persianDateTime = new JalaaliDateTime(_dateTime).Date;
        return persianDateTime.AddDays(JalaaliDayOfWeek.Saturday - persianDateTime.PersianDayOfWeek);
    }

    /// <summary>
    /// بدست آوردن تاریخ شمسی آخرین روز هفته
    /// </summary>
    public JalaaliDateTime GetPersianWeekend()
    {
        var persianDateTime = new JalaaliDateTime(_dateTime).Date;
        return persianDateTime.AddDays(JalaaliDayOfWeek.Friday - persianDateTime.PersianDayOfWeek);
    }

    /// <summary>
    /// نام فارسی ماه بر اساس شماره ماه
    /// </summary>
    /// <returns>نام فارسی ماه</returns>
    public static string GetPersianMonthName(int monthNumber)
    {
        return GetPersianMonthNamePrivate(monthNumber);
    }

    private static string? ToEnglishNumber(string? input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        input = input.Replace("ي", "ی").Replace("ك", "ک");

        //۰ ۱ ۲ ۳ ۴ ۵ ۶ ۷ ۸ ۹
        return input
            .Replace(",", "")
            .Replace("۰", "0")
            .Replace("۱", "1")
            .Replace("۲", "2")
            .Replace("۳", "3")
            .Replace("۴", "4")
            .Replace("۵", "5")
            .Replace("۶", "6")
            .Replace("۷", "7")
            .Replace("۸", "8")
            .Replace("۹", "9");
    }

    private static string GetPersianMonthNamePrivate(int monthNumber)
    {
        var monthName = "";
        switch (monthNumber)
        {
            case 1:
                monthName = "فروردین";
                break;

            case 2:
                monthName = "اردیبهشت";
                break;

            case 3:
                monthName = "خرداد";
                break;

            case 4:
                monthName = "تیر";
                break;

            case 5:
                monthName = "مرداد";
                break;

            case 6:
                monthName = "شهریور";
                break;

            case 7:
                monthName = "مهر";
                break;

            case 8:
                monthName = "آبان";
                break;

            case 9:
                monthName = "آذر";
                break;

            case 10:
                monthName = "دی";
                break;

            case 11:
                monthName = "بهمن";
                break;

            case 12:
                monthName = "اسفند";
                break;
        }

        return monthName;
    }

    #endregion

    #region IConvertible

    /// <inheritdoc />
    public TypeCode GetTypeCode()
    {
        return TypeCode.DateTime;
    }

    /// <inheritdoc />
    public bool ToBoolean(IFormatProvider? provider)
    {
        return _dateTime > DateTime.MinValue;
    }

    /// <inheritdoc />
    public byte ToByte(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public char ToChar(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return _dateTime;
    }

    /// <inheritdoc />
    public decimal ToDecimal(IFormatProvider? provider)
    {
        return ToLongDateTimeInt();
    }

    /// <inheritdoc />
    public double ToDouble(IFormatProvider? provider)
    {
        return ToLongDateTimeInt();
    }

    /// <inheritdoc />
    public short ToInt16(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public int ToInt32(IFormatProvider? provider)
    {
        return ToShortDateInt();
    }

    /// <inheritdoc />
    public long ToInt64(IFormatProvider? provider)
    {
        return ToLongDateTimeInt();
    }

    /// <inheritdoc />
    public sbyte ToSByte(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public float ToSingle(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public string ToString(IFormatProvider? provider)
    {
        return ToString("", provider);
    }

    /// <inheritdoc />
    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        switch (Type.GetTypeCode(conversionType))
        {
            case TypeCode.Boolean:
                return ToBoolean(provider);
            case TypeCode.Byte:
                return ToByte(provider);
            case TypeCode.Char:
                return ToChar(provider);
            case TypeCode.DateTime:
                return ToDateTime(provider);
            case TypeCode.Decimal:
                return ToDecimal(provider);
            case TypeCode.Double:
                return ToDouble(provider);
            case TypeCode.Int16:
                return ToInt16(provider);
            case TypeCode.Int32:
                return ToInt32(provider);
            case TypeCode.Int64:
                return ToInt64(provider);
            case TypeCode.Object:
                if (typeof(JalaaliDateTime) == conversionType)
                    return this;
                if (typeof(DateTime) == conversionType)
                    return ToDateTime();
                throw new InvalidCastException($"Conversion to a {conversionType.Name} is not supported.");
            case TypeCode.SByte:
                return ToSByte(provider);
            case TypeCode.Single:
                return ToSingle(provider);
            case TypeCode.String:
                return ToString(provider);
            case TypeCode.UInt16:
                if (provider != null) return ToUInt16(provider);
                break;
            case TypeCode.UInt32:
                if (provider != null) return ToUInt32(provider);
                break;
            case TypeCode.UInt64:
                if (provider != null) return ToUInt64(provider);
                break;
            case TypeCode.DBNull:
                break;
            case TypeCode.Empty:
                break;
            default:
                throw new InvalidCastException($"Conversion to {conversionType.Name} is not supported.");
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public ushort ToUInt16(IFormatProvider? provider)
    {
        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public uint ToUInt32(IFormatProvider? provider)
    {
        return (uint)ToShortDateInt();
    }

    /// <inheritdoc />
    public ulong ToUInt64(IFormatProvider? provider)
    {
        return (ulong)ToLongDateTimeInt();
    }

    #endregion
}