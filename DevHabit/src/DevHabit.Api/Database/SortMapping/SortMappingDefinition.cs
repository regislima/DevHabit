namespace DevHabit.Api.Database.SortMapping;

public sealed class SortMappingDefinition<TSource, TDestination> : ISortMapptingDefinition
{
    public required SortMapping[] Mappings { get; init; }
}
