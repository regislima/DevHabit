namespace DevHabit.Api.Database.SortMapping;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMapptingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        var definition = sortMapptingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (definition is null)
            throw new InvalidOperationException($"No sort mapping definition found for {typeof(TSource).Name} to {typeof(TDestination).Name}.");
        
        return definition.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return true;

        var sortFields = sort
            .Split(',')
            .Select(x => x.Trim().Split(' ')[0])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var mapping = GetMappings<TSource, TDestination>();

        return sortFields.All(f =>
            mapping.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
