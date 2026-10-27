using System;

public interface ITileType
{
    string Name { get; }
    int Id { get; }
}

[Serializable]
public readonly struct TileType : IEquatable<TileType>
{
    public readonly int Id;

    public TileType(int id)
    {
        Id = id;
    }

    public bool Equals(TileType other) => Id == other.Id;

    public override bool Equals(object obj) => obj is TileType other && Equals(other);

    public override int GetHashCode() => Id;

    public static bool operator ==(TileType a, TileType b) => a.Id == b.Id;

    public static bool operator !=(TileType a, TileType b) => a.Id != b.Id;
    
    public static bool operator ==(int a, TileType b) => a == b.Id;

    public static bool operator !=(int a, TileType b) => a != b.Id;
    
    public static bool operator ==(TileType a, int b) => a.Id == b;

    public static bool operator !=(TileType a, int b) => a.Id != b;
}
