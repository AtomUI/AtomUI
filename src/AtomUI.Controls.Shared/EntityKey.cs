using System.Globalization;
using AtomUI.Utils;

namespace AtomUI.Controls;

public readonly struct EntityKey : IEquatable<EntityKey>
{
    public EntityKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool Equals(EntityKey other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is EntityKey other)
        {
            return Equals(other);
        }

        if (obj is string str)
        {
            return Value == str;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(EntityKey left, EntityKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EntityKey left, EntityKey right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(EntityKey left, string right)
    {
        return left.Equals(new EntityKey(right));
    }

    public static bool operator !=(EntityKey left, string right)
    {
        return !left.Equals(new EntityKey(right));
    }

    public static implicit operator EntityKey(string value)
    {
        return new EntityKey(value);
    }
    
    public override string ToString()
    {
        return Value;
    }
    
    public static EntityKey Parse(string s)
    {
        using (var tokenizer = new SpanStringTokenizer(s, CultureInfo.InvariantCulture, exceptionMessage: "Invalid EntityKey."))
        {
            return new EntityKey(
                tokenizer.ReadString()
            );
        }
    }
}