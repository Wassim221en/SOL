using System.Linq.Expressions;
using Template.Domain.Enums;

namespace Template.Application.Common;

public static class HelperMethods
{
    public static List<T> ApplyPagination<T>(this List<T> list,int pageSize,int pageIndex) where T:class
    =>list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, int pageSize, int pageIndex)
        where T : class
        => queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);

    /// <summary>
    /// Apply dynamic sorting to IQueryable based on column name and sort type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to sort</param>
    /// <param name="columnName">Property name to sort by</param>
    /// <param name="sortType">Ascending or Descending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? columnName, SortType sortType) where T : class
    {
        if (!string.IsNullOrEmpty(columnName))
        {
            columnName = char.ToUpper(columnName[0]) + columnName.Substring(1);
        }
        if (string.IsNullOrWhiteSpace(columnName))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        // Get property info
        var property = typeof(T).GetProperty(columnName);
        if (property == null)
            return query; // Property not found, return unsorted query

        // Create property access expression: x => x.PropertyName
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);

        // Convert to object for OrderBy/OrderByDescending
        var convertedProperty = Expression.Convert(propertyAccess, typeof(object));

        // Create lambda expression: x => (object)x.PropertyName
        var lambda = Expression.Lambda<Func<T, object>>(convertedProperty, parameter);

        // Apply sorting
        return sortType == SortType.Ascending
            ? query.OrderBy(lambda)
            : query.OrderByDescending(lambda);
    }
    public static DateTime ToDamascusTime(DateTimeOffset utcTime)
    {
        // احصل على TimeZone لدمشق
        TimeZoneInfo damascusZone = TimeZoneInfo.FindSystemTimeZoneById("Syria Standard Time"); 
        // ملاحظة: على Windows الاسم هو "Syria Standard Time"
        // على Linux/macOS مع NodaTime أو TZDB يمكن استخدام "Asia/Damascus"

        // تحويل UTC إلى Damascus time
        DateTime damascusTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.DateTime, damascusZone);
        return damascusTime;
    }

}
public static class DbTimeZoneFunctions
{
    public static DateTime ToDamascusTime(this DateTimeOffset value)
        => HelperMethods.ToDamascusTime(value);
    
}
