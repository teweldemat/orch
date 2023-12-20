using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Collections;
using System.Reflection;

namespace orch.core.swagger
{
    public class SwaggerTypeMapper
    {
        private readonly HashSet<Type> ProcessingTypes = new();

        private readonly Dictionary<Type, (string type, string? format)> PrimitiveTypeMap = new()
        {
            {typeof(string), ("string", null)},
            {typeof(int), ("integer", "int32")},
            {typeof(long), ("integer", "int64")},
            {typeof(short), ("integer", "int16")},
            {typeof(double), ("number", "double")},
            {typeof(float), ("number", "float")},
            {typeof(decimal), ("number", "decimal")},
            {typeof(bool), ("boolean", null)},
            {typeof(DateTime), ("string", "date-time")},
            {typeof(Guid), ("string", "uuid")},
            {typeof(byte), ("string", "byte")},
            {typeof(char), ("string", "char")},
            {typeof(uint), ("integer", "uint32")},
            {typeof(ulong), ("integer", "uint64")},
            {typeof(ushort), ("integer", "uint16")},
            {typeof(sbyte), ("integer", "int8")},
            {typeof(TimeSpan), ("string", "duration")},
        };

        private static bool HasOGeneratedDataAttribute(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute(typeof(OGeneratedDataAttribute)) != null;
        }
        private static bool HasOViewUserAttribute(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute(typeof(OViewUserAttribute)) != null;
        }

        public OpenApiSchema? MapTypeToSchema(Type type, OpenApiDocument openApiDocument)
        {

            if (type == null || HasOGeneratedDataAttribute(type) || HasOViewUserAttribute(type) || ProcessingTypes.Contains(type))
            {
                return null;
            }

            OpenApiSchema? schema;
            if (type == typeof(string) || type == typeof(Guid) || type.IsPrimitive)
            {
                schema = GetPrimitiveSchema(type);
            }
            else if (type.IsEnum)
            {
                schema = GetEnumSchema(type);
            }
            else
            {
                schema = InitializeSchema(type, openApiDocument);
            }

            if (schema == null)
            {
                return null;
            }


            ProcessingTypes.Add(type);

            var displayName = GetDisplayName(type);

            openApiDocument.Components.Schemas[displayName] = schema;

            if (schema.Type == "object")
            {
                PopulateSchemaProperties(type, schema, openApiDocument);
            }

            ProcessingTypes.Remove(type);

            return new OpenApiSchema { Reference = new OpenApiReference { Id = displayName, Type = ReferenceType.Schema } };
        }

        private OpenApiSchema? InitializeSchema(Type type, OpenApiDocument openApiDocument)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetPrimitiveSchema(type.GetGenericArguments()[0]);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var genericArguments = type.GetGenericArguments();
                var itemType = type.GetElementType() ?? (genericArguments.Length > 0 ? genericArguments[0] : null);
                if (itemType != null)
                {
                    return new OpenApiSchema
                    {
                        Type = "array",
                        Items = MapTypeToSchema(itemType, openApiDocument)
                    };
                }
            }
            else
            {
                return new OpenApiSchema { Type = "object", Properties = new Dictionary<string, OpenApiSchema>() };
            }

            return null;
        }

        private OpenApiSchema GetPrimitiveSchema(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (PrimitiveTypeMap.TryGetValue(type, out var schemaInfo))
            {
                return new OpenApiSchema
                {
                    Type = schemaInfo.type,
                    Format = schemaInfo.format
                };
            }

            return new OpenApiSchema { Description = $"Unknown Type: {type.Name}" };
        }

        private OpenApiSchema GetEnumSchema(Type enumType)
        {
            var enumValues = Enum.GetNames(enumType);
            return new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny>(enumValues.Select(e => new OpenApiString(e)))
            };
        }

        public string GetDisplayName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var index = type.Name.IndexOf('`');
            var typeName = index >= 0 ? type.Name.Substring(0, index) : type.Name;
            var genericArgs = string.Join(",", type.GetGenericArguments().Select(t => GetDisplayName(t)));
            return $"{typeName}<{genericArgs}>";
        }


        private void PopulateSchemaProperties(Type type, OpenApiSchema schema, OpenApiDocument openApiDocument)
        {
            if (type == null || schema == null) return;

            var flags = BindingFlags.Public | BindingFlags.Instance;

            foreach (var propertyInfo in type.GetProperties(flags))
            {
                if (HasOGeneratedDataAttribute(propertyInfo)) continue;
                if (HasOViewUserAttribute(propertyInfo)) continue;

                var propertySchema = propertyInfo.PropertyType != null ? MapTypeToSchema(propertyInfo.PropertyType, openApiDocument) : null;
                if (propertySchema != null)
                {
                    schema.Properties[propertyInfo.Name] = propertySchema;
                }
            }

            foreach (var fieldInfo in type.GetFields(flags))
            {
                if (HasOGeneratedDataAttribute(fieldInfo)) continue;
                if (HasOViewUserAttribute(fieldInfo)) continue;


                var fieldSchema = MapTypeToSchema(fieldInfo.FieldType, openApiDocument);
                if (fieldSchema != null)
                {
                    schema.Properties[fieldInfo.Name] = fieldSchema;
                }
            }
        }


    }
}
