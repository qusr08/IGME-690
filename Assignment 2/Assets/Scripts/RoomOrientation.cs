using System;

[Serializable]
public class RoomOrientation
{
    public ConnectionType PositiveX;
    public ConnectionType NegativeX;
    public ConnectionType PositiveY;
    public ConnectionType NegativeY;
    public ConnectionType PositiveZ;
    public ConnectionType NegativeZ;
    public int Rotation;

    private readonly ConnectionType defaultType;

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
            PositiveY = PositiveY,
            NegativeY = NegativeY,
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
        PositiveY = parent != null ? parent.PositiveY : defaultType;
        NegativeY = parent != null ? parent.NegativeY : defaultType;
        PositiveZ = parent != null ? parent.PositiveZ : defaultType;
        NegativeZ = parent != null ? parent.NegativeZ : defaultType;
        Rotation = parent != null ? parent.Rotation : 0;
    }

    public override string ToString()
    {
        return $"+X: {PositiveX} | -X: {NegativeX} | +Y: {PositiveY} | -Y: {NegativeY} | +Z: {PositiveZ} | -Z: {NegativeZ}";
    }
}
