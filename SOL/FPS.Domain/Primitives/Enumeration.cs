using System.Reflection;

namespace Template.Domain.Primitives;

public abstract record Enumeration<TEnumeration> : IComparable<Enumeration<TEnumeration>>
    where TEnumeration : Enumeration<TEnumeration>
{
    private static readonly Lazy<Dictionary<int, TEnumeration>> EnumerationsValueDictionary =
        new Lazy<Dictionary<int, TEnumeration>>(() => GetAllEnumerationOptions().ToDictionary(item => item.Value));


    protected Enumeration(int value)
    {
        Value = value;
    }

    protected Enumeration()
    {
        Value = default;
    }


    public static IReadOnlyCollection<TEnumeration> List => EnumerationsValueDictionary.Value.Values.ToList();


    public int Value { get; private set; }


    public static TEnumeration From(int value) => EnumerationsValueDictionary.Value.TryGetValue(value, out var enumeration)
        ? enumeration!
        : default!;

    //public static TEnumeration From<TEnum>(TEnum @enum) where TEnum : Enum => From((int)Enum.ToObject(typeof(TEnum), @enum));
    public static TEnumeration From<TEnum>(TEnum @enum) where TEnum : Enum => From(From(Convert.ToInt32(@enum)));

    public static bool ContainsValue(int value) => EnumerationsValueDictionary.Value.ContainsKey(value);

    public TEnum ToEnum<TEnum>() where TEnum : Enum => (TEnum)Enum.ToObject(typeof(TEnum), Value);
    public static implicit operator int(Enumeration<TEnumeration> enumeration) => enumeration.Value;

    private static IEnumerable<TEnumeration> GetAllEnumerationOptions()
    {
        Type enumType = typeof(TEnumeration);

        IEnumerable<Type> enumerationTypes = Assembly
            .GetAssembly(enumType)!
            .GetTypes()
            .Where(type => enumType.IsAssignableFrom(type));

        var enumerations = new List<TEnumeration>();

        foreach (Type enumerationType in enumerationTypes)
        {
            List<TEnumeration> enumerationTypeOptions = GetFieldsOfType<TEnumeration>(enumerationType);

            enumerations.AddRange(enumerationTypeOptions);
        }

        return enumerations;
    }


    private static List<TFieldType> GetFieldsOfType<TFieldType>(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fieldInfo => type.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TFieldType)fieldInfo.GetValue(null)!)
            .ToList();

    /// <inheritdoc />
    public int CompareTo(Enumeration<TEnumeration> other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Value.CompareTo(other.Value);
    }
}