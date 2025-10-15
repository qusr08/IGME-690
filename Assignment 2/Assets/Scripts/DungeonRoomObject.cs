using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomObject : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> floors;
    [SerializeField] private List<MeshRenderer> walls;
    [SerializeField] private Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;

            Color.RGBToHSV(_color, out float h, out float s, out float v);
            foreach (MeshRenderer floor in floors)
            {
                Material floorMaterial = new Material(floor.material);
                floorMaterial.color = _color;
                floor.material = floorMaterial;
            }

            Color wallColor = Color.HSVToRGB(h, s, v * 0.65f);
            foreach (MeshRenderer wall in walls)
            {
                Material wallMaterial = new Material(wall.material);
                wallMaterial.color = wallColor;
                wall.material = wallMaterial;
            }
        }
    }
}
