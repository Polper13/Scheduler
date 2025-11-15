using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

public static class TimeSpanUtils
{
    public static bool tryParseDuration(string input, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        TimeSpan result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim().ToLowerInvariant();  // Clean it up, genius.

        // Regex: Greedily match all "number [unit]" chunks (spaces optional around unit, no split numbers)
        var match = Regex.Match(input, @"^(\d+\s*h)?\s*(\d+\s*m)?\s*(\d+\s*s?)?$");
        if (!match.Success) { return false; }

        for (int i = 1; i < match.Groups.Count; i++)
        {
            Group group = match.Groups[i];
            if (!group.Success) { continue; }

            string subInput = Regex.Replace(group.Value, @"[a-z\s]+", "");
            if (!int.TryParse(subInput, out int value)) { return false; } 

            switch (i)
            {
                case 1:
                    result += TimeSpan.FromHours(value);
                    break;
                case 2:
                    result += TimeSpan.FromMinutes(value);
                    break;
                case 3:
                    result += TimeSpan.FromSeconds(value);
                    break;
                default:
                    return false;
            }
        }

        duration = result;
        return true;
    }

    public static string ToDetailedString(TimeSpan duration)
    {
        if (duration == TimeSpan.Zero) return "0 s";

        int hours = (int)duration.TotalHours;
        int minutes = duration.Minutes;
        int seconds = duration.Seconds;

        var parts = new List<string>();
        if (hours > 0) parts.Add($"{hours} h");
        if (minutes > 0) parts.Add($"{minutes} m");
        if (seconds > 0) parts.Add($"{seconds} s");

        return parts.Count > 0 ? string.Join(" ", parts) : "0 s";  // Fallback, genius.
    }
}
