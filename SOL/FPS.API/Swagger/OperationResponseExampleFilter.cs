using System.Collections;
using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Template.Application.Common.Core.Response;

namespace Template.API.Swagger;

public class OperationResponseExampleFilter : IOperationFilter
{
    private const int MaxDepth = 3;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var responseTypesByStatus = context.ApiDescription.SupportedResponseTypes
            .Where(responseType => responseType.Type != null)
            .GroupBy(responseType => responseType.StatusCode == 0 ? "default" : responseType.StatusCode.ToString())
            .ToDictionary(group => group.Key, group => group.First().Type!);

        var fallbackResponseType = GetResponseTypeFromHandler(context.MethodInfo);

        foreach (var response in operation.Responses)
        {
            var statusCode = response.Key;
            var openApiResponse = response.Value;

            Type? responseType = null;
            if (responseTypesByStatus.TryGetValue(statusCode, out var typedResponse) &&
                typedResponse != typeof(void))
            {
                responseType = typedResponse;
            }

            if (!TryGetOperationResponseType(responseType, out var operationResponseType, out var dataType) &&
                !TryGetOperationResponseType(fallbackResponseType, out operationResponseType, out dataType))
            {
                continue;
            }

            openApiResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
            openApiResponse.Content.Remove("text/plain");

            if (!openApiResponse.Content.TryGetValue("application/json", out var mediaType))
            {
                mediaType = new OpenApiMediaType();
                openApiResponse.Content["application/json"] = mediaType;
            }

            if (mediaType.Schema == null && operationResponseType != null)
            {
                mediaType.Schema = context.SchemaGenerator.GenerateSchema(operationResponseType, context.SchemaRepository);
            }

            if (mediaType.Example != null)
                continue;

            var isSuccess = IsSuccessStatusCode(statusCode);
            mediaType.Example = BuildExample(dataType, isSuccess, statusCode);
        }
    }

    private static Type? GetResponseTypeFromHandler(MethodInfo methodInfo)
    {
        foreach (var parameter in methodInfo.GetParameters())
        {
            var parameterType = parameter.ParameterType;
            if (!parameterType.IsGenericType)
                continue;

            if (parameterType.GetGenericTypeDefinition() != typeof(IRequestHandler<,>))
                continue;

            return parameterType.GetGenericArguments()[1];
        }

        return null;
    }

    private static bool TryGetOperationResponseType(Type? responseType, out Type? operationResponseType, out Type? dataType)
    {
        operationResponseType = null;
        dataType = null;

        if (responseType == null)
            return false;

        var unwrapped = UnwrapType(responseType);

        if (unwrapped == typeof(OperationResponse))
        {
            operationResponseType = unwrapped;
            return true;
        }

        if (unwrapped.IsGenericType && unwrapped.GetGenericTypeDefinition() == typeof(OperationResponse<>))
        {
            operationResponseType = unwrapped;
            dataType = unwrapped.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    private static Type UnwrapType(Type type)
    {
        var current = type;
        while (true)
        {
            if (current.IsGenericType)
            {
                var definition = current.GetGenericTypeDefinition();
                if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
                {
                    current = current.GetGenericArguments()[0];
                    continue;
                }

                if (definition == typeof(ActionResult<>))
                {
                    current = current.GetGenericArguments()[0];
                    continue;
                }
            }

            return current;
        }
    }

    private static bool IsSuccessStatusCode(string statusCode)
    {
        if (int.TryParse(statusCode, out var code))
            return code >= 200 && code < 300;

        return statusCode.StartsWith("2", StringComparison.Ordinal);
    }

    private static IOpenApiAny BuildExample(Type? dataType, bool isSuccess, string statusCode)
    {
        var example = new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(isSuccess),
            ["errorMessage"] = isSuccess ? new OpenApiNull() : BuildErrorMessage(statusCode)
        };

        if (dataType != null)
        {
            example["data"] = isSuccess
                ? CreateExampleForType(dataType, 0, new HashSet<Type>())
                : new OpenApiNull();
        }

        return example;
    }

    private static IOpenApiAny BuildErrorMessage(string statusCode)
    {
        var message = "Error message";

        if (int.TryParse(statusCode, out var code))
        {
            message = code switch
            {
                400 => "Bad request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not found",
                409 => "Conflict",
                500 => "Server error",
                _ => "Error message"
            };
        }

        return new OpenApiObject
        {
            ["message"] = new OpenApiString(message)
        };
    }

    private static IOpenApiAny CreateExampleForType(Type type, int depth, HashSet<Type> visited)
    {
        if (depth > MaxDepth)
            return new OpenApiObject();

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (TryCreateSimpleExample(underlyingType, out var example))
            return example;

        if (underlyingType == typeof(TimeSpan))
            return new OpenApiString("00:00:00");

        if (typeof(IDictionary).IsAssignableFrom(underlyingType))
        {
            var valueType = GetDictionaryValueType(underlyingType) ?? typeof(object);
            return new OpenApiObject
            {
                ["key"] = CreateExampleForType(valueType, depth + 1, visited)
            };
        }

        if (typeof(IEnumerable).IsAssignableFrom(underlyingType) && underlyingType != typeof(string))
        {
            var itemType = GetEnumerableItemType(underlyingType) ?? typeof(object);
            return new OpenApiArray
            {
                CreateExampleForType(itemType, depth + 1, visited)
            };
        }

        if (visited.Contains(underlyingType))
            return new OpenApiObject();

        visited.Add(underlyingType);

        var obj = new OpenApiObject();
        foreach (var property in underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetMethod == null || property.GetMethod.GetParameters().Length > 0)
                continue;

            var propertyName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            obj[propertyName] = CreateExampleForType(property.PropertyType, depth + 1, visited);
        }

        visited.Remove(underlyingType);
        return obj;
    }

    private static bool TryCreateSimpleExample(Type type, out IOpenApiAny example)
    {
        example = null!;

        if (type == typeof(string))
        {
            example = new OpenApiString("string");
            return true;
        }

        if (type == typeof(Guid))
        {
            example = new OpenApiString("00000000-0000-0000-0000-000000000000");
            return true;
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            example = new OpenApiString("2024-01-01T00:00:00Z");
            return true;
        }

        if (type == typeof(DateOnly))
        {
            example = new OpenApiString("2024-01-01");
            return true;
        }

        if (type == typeof(TimeOnly))
        {
            example = new OpenApiString("00:00:00");
            return true;
        }

        if (type == typeof(bool))
        {
            example = new OpenApiBoolean(true);
            return true;
        }

        if (type.IsEnum)
        {
            var value = Enum.GetNames(type).FirstOrDefault() ?? "value";
            example = new OpenApiString(value);
            return true;
        }

        if (type == typeof(byte[]))
        {
            example = new OpenApiString("base64string");
            return true;
        }

        if (TryCreateNumberExample(type, out var numberExample))
        {
            example = numberExample;
            return true;
        }

        return false;
    }

    private static bool TryCreateNumberExample(Type type, out IOpenApiAny example)
    {
        example = null!;

        if (type == typeof(byte) || type == typeof(sbyte) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(int) || type == typeof(uint))
        {
            example = new OpenApiInteger(1);
            return true;
        }

        if (type == typeof(long) || type == typeof(ulong))
        {
            example = new OpenApiLong(1);
            return true;
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            example = new OpenApiDouble(1.0);
            return true;
        }

        return false;
    }

    private static Type? GetEnumerableItemType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
            return type.GetGenericArguments().FirstOrDefault();

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments().FirstOrDefault();
    }

    private static Type? GetDictionaryValueType(Type type)
    {
        if (type.IsGenericType && type.GetGenericArguments().Length == 2)
            return type.GetGenericArguments()[1];

        var dictionaryInterface = type.GetInterfaces()
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        return dictionaryInterface?.GetGenericArguments()[1];
    }
}
