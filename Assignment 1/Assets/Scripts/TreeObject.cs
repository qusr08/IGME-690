using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TreeObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer oakLeafRenderer;
    [SerializeField] private MeshRenderer oakLogRenderer;
    [SerializeField] private GameObject treeGameObject;
    [SerializeField] private List<Texture> leafTextureList;

    public bool IsEnabled { set => treeGameObject.SetActive(value); }

    private void Start()
    {
        Material leafMaterial = new Material(oakLeafRenderer.material);
        leafMaterial.mainTexture = leafTextureList[Random.Range(0, leafTextureList.Count)];
        oakLeafRenderer.material = leafMaterial;
    }
}
