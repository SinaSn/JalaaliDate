DateTime now = new DateTime(2020, 3, 19);
var jd = new JalaaliDate.JalaaliDate(now);
var x = jd.ToString();
var y = jd.AddYears(1).ToString();
Console.WriteLine(x);
Console.WriteLine(y);