using DevHabit.Api.Database.SortMapping;
using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Extensions;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sort,
        SortMapping[] mappings,
        string defaulOrderBy = "Id")
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(defaulOrderBy);

        var sortFields = sort.Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var orderByParts = new List<string>();

        foreach (var field in sortFields)
        {
            (string sortField, bool isDescending) = ParseSortField(field);
            var mapping = mappings.First(m => m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));
            var direction = (isDescending, mapping.Reverse) switch
            {
                (true, true) => "ASC",
                (true, false) => "DESC",
                (false, true) => "DESC",
                (false, false) => "ASC"
            };

            orderByParts.Add($"{mapping.PropertyName} {direction}");
        }

        var orderBy = string.Join(",", orderByParts);

        return query.OrderBy(orderBy);
    }

    private static (string sortField, bool isDescending) ParseSortField(string field)
    {
        var parts = field.Split(' ');
        var sortField = parts[0];
        var isDescending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortField, isDescending);
    }
}
