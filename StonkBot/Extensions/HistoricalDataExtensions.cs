﻿using StonkBot.Data.Entities;

namespace StonkBot.Extensions;

public static class HistoricalDataExtensions
{
    public static bool Intersect(this HistoricalData h1, HistoricalData h2)
    {
        var a1 = new[] { h1.Open, h1.Close };
        var a2 = new[] { h2.Open, h2.Close };

        var intersect = a1.Min() <= a2.Max() && a1.Max() >= a2.Min();
        return intersect;
    }

    public static bool ChangeIsSmall(this HistoricalData h1, HistoricalData h2)
    {
        var a1 = new[] { h1.Open, h1.Close };
        var a2 = new[] { h2.Open, h2.Close };

        var checkDiff = a1.Max() < a2.Min()
            ? a1.Max() / a2.Min()
            : a1.Min() / a2.Max();

        var changeIsSmall = checkDiff <= (decimal)1.03;
        return changeIsSmall;
    }

    public static bool PositiveChange(this HistoricalData h1, HistoricalData h2)
    {
        var a1 = new[] { h1.Open, h1.Close };
        var a2 = new[] { h2.Open, h2.Close };

        return a1.Max() < a2.Min();
    }

    public static bool IsAlwaysDown(this HistoricalData h1, List<HistoricalData> range)
    {
        return range.All(x => x.Close <= h1.High);
    }

    public static bool IsAlwaysUp(this HistoricalData h1, List<HistoricalData> range)
    {
        return range.All(x => x.Close >= h1.Low);
    }
}