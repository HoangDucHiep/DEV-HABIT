﻿namespace DevHabit.Api.Utils;

public static class IdGenerator
{
    public static string GenerateId(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..26];

}

public static class IdPrefix
{
    public const string HABIT = "h_";
    public const string TAG = "t_";
    public const string USER = "u_";
    public const string GITHUB_ACCESS_TOKEN = "gh_";
}
