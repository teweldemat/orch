using System.Collections;
using System.Reflection;

namespace orch.common
{
    public static class TypeParser
    {
        static public object? TryParseGuid(string str, object? defaultValue)
        {
            if (string.IsNullOrEmpty(str)) return defaultValue;
            if (Guid.TryParse(str, out Guid parsedGuid)) return parsedGuid;
            throw new FormatException($"Failed to parse Guid: {str}");
        }

        static public object? Convert(string str, Type targetType)
        {
            try
            {
                static string[] SplitValues(string input)
                {
                    return string.IsNullOrEmpty(input) ? Array.Empty<string>() : input.Split(',');
                }

                if (targetType == null) return null;
                if (targetType == typeof(Guid)) return TryParseGuid(str, Guid.Empty);
                if (targetType == typeof(Guid?)) return TryParseGuid(str, null);
                if (targetType == typeof(string)) return string.IsNullOrEmpty(str) ? string.Empty : str;

                if (targetType.IsEnum)
                {
                    if (Enum.TryParse(targetType, str, out object? parsedEnum))
                    {
                        return parsedEnum;
                    }
                    throw new FormatException($"Failed to parse Enum of type {targetType.FullName}: {str}");
                }

                if (targetType.IsArray)
                {
                    var elementType = targetType.GetElementType();
                    if (elementType == null)
                    {
                        throw new InvalidOperationException($"Failed to get the element type for the target array of type {targetType.FullName}.");
                    }
                    var values = SplitValues(str);
                    var array = Array.CreateInstance(elementType, values.Length);
                    for (int i = 0; i < values.Length; i++)
                    {
                        array.SetValue(Convert(values[i], elementType), i);
                    }
                    return array.Length == 0 ? Array.Empty<object>().GetType().MakeGenericType(elementType) : array;
                }

                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                    if (underlyingType == null)
                    {
                        throw new InvalidOperationException($"Failed to get the underlying type for the nullable type {targetType.FullName}.");
                    }
                    return string.IsNullOrEmpty(str) ? null : Convert(str, underlyingType);
                }

                if (targetType.GetInterfaces().Contains(typeof(IList)))
                {
                    var elementType = targetType.GetGenericArguments()[0];
                    var values = SplitValues(str);
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = Activator.CreateInstance(listType);
                    MethodInfo? addMethod = listType.GetMethod("Add");
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            addMethod?.Invoke(list, new[] { Convert(value, elementType) });
                        }
                    }
                    return list;
                }

                return System.Convert.ChangeType(str, targetType);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to convert {str} to {targetType.FullName}: {ex.Message}");
            }
        }

        public static object? GetParameterValueFromQuery(ParameterInfo paramInfo, Dictionary<string, string> queryParams)
        {
            object? paramValue;
            if (paramInfo.ParameterType.IsClass && paramInfo.ParameterType != typeof(string))
            {
                paramValue = Activator.CreateInstance(paramInfo.ParameterType);
                PropertyInfo[] properties = paramInfo.ParameterType.GetProperties();
                foreach (var (property, convertedValue) in from property in properties
                                                           where queryParams.ContainsKey(property.Name)
                                                           let convertedValue = Convert(queryParams[property.Name], property.PropertyType)
                                                           select (property, convertedValue))
                {
                    property.SetValue(paramValue, convertedValue);
                }
            }
            else if (queryParams.TryGetValue(paramInfo?.Name, out string? valueStr))
            {
                paramValue = Convert(valueStr, paramInfo.ParameterType);
            }
            else if (paramInfo.HasDefaultValue)
            {
                paramValue = paramInfo.DefaultValue;
            }
            else if (Nullable.GetUnderlyingType(paramInfo.ParameterType) != null)
            {
                paramValue = null;
            }
            else
            {
                throw new ArgumentException($"Missing parameter: {paramInfo.Name}");
            }

            return paramValue;
        }

    }
}