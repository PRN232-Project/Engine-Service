using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Services.QueryParams;
using PRN232.LMS.Services.Shared;

namespace PRN232.LMS.Services.Helpers;

public static class QueryHelper
{
    public static IQueryable<T> ApplySort<T>(IQueryable<T> source, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return source;

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<T>? ordered = null;

        foreach (var raw in fields)
        {
            var desc = raw.StartsWith('-') || raw.EndsWith("_desc", StringComparison.OrdinalIgnoreCase);
            var name = raw
                .TrimStart('-')
                .Replace("_desc", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("_asc", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();
            var propertyName = NormalizePropertyName(name);
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = typeof(T).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                continue;

            var access = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(access, parameter);
            var methodName = ordered == null
                ? (desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                : (desc ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType);

            ordered = (IOrderedQueryable<T>?)method.Invoke(null, new object[] { ordered ?? source, lambda });
        }

        return ordered ?? source;
    }

    private static string NormalizePropertyName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1) return parts[0];

        return string.Concat(parts.Select((part, index) =>
            index == 0
                ? char.ToLowerInvariant(part[0]) + part[1..]
                : char.ToUpperInvariant(part[0]) + part[1..]));
    }

    public static async Task<PagedResult<object>> PaginateAsync<T>(
        IQueryable<T> source,
        BaseQueryParams query,
        Func<T, object> mapItem)
    {
        var totalItems = await source.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.Size);
        var entities = await source.Skip((query.Page - 1) * query.Size).Take(query.Size).ToListAsync();

        var items = string.IsNullOrWhiteSpace(query.Fields)
            ? entities.Select(e => mapItem(e))
            : entities.Select(e => SelectFields(mapItem(e), query.Fields));

        return new PagedResult<object>
        {
            Items = items,
            Pagination = new PaginationMeta
            {
                Page       = query.Page,
                PageSize   = query.Size,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };
    }

    /// <summary>Dynamic field selection using ExpandoObject.</summary>
    public static object SelectFields(object obj, string fields)
    {
        var props  = fields.Split(',').Select(f => f.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var expando = (IDictionary<string, object?>)new ExpandoObject();
        foreach (var prop in obj.GetType().GetProperties())
            if (props.Contains(prop.Name))
                expando[prop.Name] = prop.GetValue(obj);
        return expando;
    }
}
