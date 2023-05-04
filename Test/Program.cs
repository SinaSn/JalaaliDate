using JalaaliDate;

DateTime now = new DateTime(2021, 7, 5);
var jd = new JalaaliDateTime(now);
var a = jd.GetMonthDays;
var x = jd.ToString("yyyy/MM/dd HH:mm:ss");
var y = jd.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
Console.WriteLine(x);
Console.WriteLine(y);