using System.Globalization;

namespace JalaaliDate;

public class JalaaliDate
{
    private static int _jYear;
    private static int _jMonth;
    private static int _jDay;
    private static int _hour;
    private static int _minute;
    private static int _second;
    private static double _milliseconds;
    private static PersianCalendar _calendar = new();

    public JalaaliDate(DateTime dateTime)
    {
        _jYear = _calendar.GetYear(dateTime);
        _jMonth = _calendar.GetMonth(dateTime);
        _jDay = _calendar.GetDayOfMonth(dateTime);
        _hour = _calendar.GetHour(dateTime);
        _minute = _calendar.GetMinute(dateTime);
        _second = _calendar.GetSecond(dateTime);
        _milliseconds = _calendar.GetMilliseconds(dateTime);
    }

    public string ToString(string format = "yyyy/MM/dd HH:mm:ss")
    {
        return _calendar
            .ToDateTime(_jYear, _jMonth, _jDay, _hour, _minute, _second, (int)_milliseconds)
            .ToString(format, new CultureInfo("fa-IR"));
    }

    public JalaaliDate AddYears(int years)
    {
        _jYear += years;
        var daysInMonth = _calendar.GetDaysInMonth(_jYear, _jMonth);
        if (_jDay > daysInMonth)
            _jDay = daysInMonth;
        return this;
    }

    public JalaaliDate AddMonths(int months)
    {
        switch (months)
        {
            case > 0 and > 12:
            {
                var extraYears = months / 12;
                var finalMonth = months % 12;
                _jYear += extraYears;
                _jMonth += finalMonth;

                if (_jMonth <= 12) return this;
                _jYear += 1;
                _jMonth -= 12;
                break;
            }
            case > 0:
                _jMonth += months;
                break;
            case < -12:
            {
                var extraYears = months / 12;
                var finalMonth = months % 12;
                _jYear += extraYears;
                _jMonth += finalMonth;

                if (_jMonth < -12)
                {
                    _jYear -= 1;
                    _jMonth += 12;
                }

                break;
            }
            default:
                _jMonth += months;
                break;
        }

        var daysInMonth = _calendar.GetDaysInMonth(_jYear, _jMonth);
        if (daysInMonth < _jDay)
        {
            _jDay = daysInMonth;
        }

        return this;
    }

    public JalaaliDate AddDays(int days)
    {
        var dateTime = _calendar.ToDateTime(_jYear, _jMonth, _jDay, _hour, _minute, _second, (int)_milliseconds);
        dateTime = dateTime.AddDays(days);
        _jYear = _calendar.GetYear(dateTime);
        _jMonth = _calendar.GetMonth(dateTime);
        _jDay = _calendar.GetDayOfMonth(dateTime);
        _hour = _calendar.GetHour(dateTime);
        _minute = _calendar.GetMinute(dateTime);
        _second = _calendar.GetSecond(dateTime);
        _milliseconds = _calendar.GetMilliseconds(dateTime);
        return this;
    }
}