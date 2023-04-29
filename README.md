# JalaaliDate

JalaaliDate is a .NET library for dealing with Persian date.

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)

## Usage

```csharp
var now = new DateTime(2020, 3, 19);

# create an instance of JalaaliDate
var jd = new JalaaliDate.JalaaliDate(now);

# returns persian date in a specific format
var dateString = jd.ToString("yyyy/MM/dd HH:mm:ss");

# add days to given date
var addDay = jd.AddDays(1);

# add months to given date
var addMonth = jd.AddMonths(1);

# add years to given date
var addYear = jd.AddYears(1);
```

## License

[MIT](https://choosealicense.com/licenses/mit/)