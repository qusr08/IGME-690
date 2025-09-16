using UnityEditor;
using UnityEngine;

public class CreateTextureAtlas : MonoBehaviour { }

[CustomEditor(typeof(CreateTextureAtlas))]
public class CreateTextureAtlasEditor : Editor
{
    private string directoryName = "Assets/Textures/Atlas Textures";
    private string outputFileName = "Assets/Atlas.png";

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Update Atlas Texture"))
        {
            TextureAtlas.Instance.CreateAtlasComponentData(directoryName, outputFileName);
            Debug.Log("Updated atlas texture.");
        }
    }
}