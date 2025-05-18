using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace DevHabit.Api.Tools;

public sealed class DataShapingTool
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        var propertyInfos = GetPropertyInfos<T>(fields);
        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (var propertyinfo in propertyInfos)
            shapedObject[propertyinfo.Name] = propertyinfo.GetValue(entity);

        return (ExpandoObject)shapedObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields)
    {
        var shapedObjects = new List<ExpandoObject>();
        var propertyInfos = GetPropertyInfos<T>(fields);

        foreach (var entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (var propertyinfo in propertyInfos)
                shapedObject[propertyinfo.Name] = propertyinfo.GetValue(entity);

            shapedObjects.Add((ExpandoObject)shapedObject);
        }

        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return true;

        var fieldSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var propertyInfos = _propertiesCache.GetOrAdd(
            typeof(T),
            type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldSet.All(field =>
            propertyInfos.Any(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase)));
    }

    private static PropertyInfo[] GetPropertyInfos<T>(string? fields)
    {
        var fieldSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var propertyInfos = _propertiesCache.GetOrAdd(
            typeof(T),
            type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldSet.Any())
            propertyInfos = propertyInfos
            .Where(p => fieldSet.Contains(p.Name))
            .ToArray();
        
        return propertyInfos;
    }
}
