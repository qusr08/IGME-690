using UnityEngine;

public class CellFeature : MonoBehaviour
{
	[SerializeField, Range(0f, 1f)] private float offsetRange;
	[SerializeField, Range(0, 180)] private int rotationIncrements;

	private void Start()
	{
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in meshRenderers)
		{
			Material mat = new Material(renderer.material);
			mat.color = Utils.GetOffsetColor(mat.color);
			renderer.material = mat;
		}
	}

	private void OnEnable()
	{
		transform.localPosition = new Vector3(Random.Range(-offsetRange, offsetRange), transform.localPosition.y, Random.Range(-offsetRange, offsetRange));
		transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360 / rotationIncrements) * rotationIncrements, 0f);
		transform.localScale = new Vector3(1, Random.Range(0.999f, 1.001f), 1);
	}
}
