using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TreeObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private int _maxTreeObjects;

    private List<TreeObject> enabledTreeObjects;
    private List<TreeObject> disabledTreeObjects;

    public int MaxTreeObjects => _maxTreeObjects;

    private void Awake()
    {
        enabledTreeObjects = new List<TreeObject>();
        disabledTreeObjects = new List<TreeObject>();

        for (int i = 0; i < MaxTreeObjects; i++)
        {
            TreeObject treeObject = Instantiate(treePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<TreeObject>();
            treeObject.IsEnabled = false;
            disabledTreeObjects.Add(treeObject);
        }
    }

    public void DisableAllTrees()
    {
        for (int i = enabledTreeObjects.Count - 1; i >= 0; i--)
        {
            TreeObject treeObject = enabledTreeObjects[i];
            treeObject.IsEnabled = false;
            disabledTreeObjects.Add(treeObject);
            enabledTreeObjects.Remove(treeObject);
        }
    }

    public void PlaceTreeAt (Vector3 position)
    {
        if (disabledTreeObjects.Count == 0) {
            return;
        }

        TreeObject treeObject = disabledTreeObjects[0];
        treeObject.IsEnabled = true;
        treeObject.transform.localPosition = position;
        disabledTreeObjects.RemoveAt(0);
        enabledTreeObjects.Add(treeObject);
    }
}
