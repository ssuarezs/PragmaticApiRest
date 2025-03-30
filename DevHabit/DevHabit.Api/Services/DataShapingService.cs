﻿using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace DevHabit.Api.Services;

public class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }


        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)shapedObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }

        List<ExpandoObject> shapedObjects = new();
        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            shapedObjects.Add((ExpandoObject)shapedObject);
        }

        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f => propertyInfos.Any(p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
