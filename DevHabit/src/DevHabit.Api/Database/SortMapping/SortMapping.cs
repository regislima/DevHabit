namespace DevHabit.Api.Database.SortMapping;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);
