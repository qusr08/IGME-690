using System;

[Serializable]
public class RoomOrientation
{
    public ConnectionType PositiveX;
    public ConnectionType NegativeX;
    public ConnectionType PositiveZ;
    public ConnectionType NegativeZ;

    private readonly ConnectionType defaultType;

    public int Rotation { get; set; }

    public RoomOrientation(RoomOrientation parent, ConnectionType defaultType = ConnectionType.ANY)
    {
        this.defaultType = defaultType;
        SetValuesFromParent(parent);
    }

    public RoomOrientation(ConnectionType defaultType = ConnectionType.ANY) : this(null, defaultType) { }

    public RoomOrientation RotateClockwise()
    {
        RoomOrientation rotated = new RoomOrientation(this)
        {
            PositiveX = NegativeZ,
            NegativeX = PositiveZ,
            PositiveZ = PositiveX,
            NegativeZ = NegativeX,
            Rotation = (Rotation + 1) % 4
        };

        SetValuesFromParent(rotated);
        return rotated;
    }

    public void SetValuesFromParent(RoomOrientation parent)
    {
        PositiveX = parent != null ? parent.PositiveX : defaultType;
        NegativeX = parent != null ? parent.NegativeX : defaultType;
        PositiveZ = parent != null ? parent.PositiveZ : defaultType;
        NegativeZ = parent != null ? parent.NegativeZ : defaultType;
        Rotation = parent != null ? parent.Rotation : 0;
    }

    public override string ToString()
    {
        return $"+X: {PositiveX} | -X: {NegativeX} | +Z: {PositiveZ} | -Z: {NegativeZ}";
    }
}
