using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
	[SerializeField, Range(0f, 90f)] private float cameraTiltAngle;
	[SerializeField, Range(0f, 360f)] private float orbitSpeed;
	[SerializeField, Range(0f, 2f)] private float padding;

	private CellGrid cellGrid;
	private float height;
	private float angle;
	private float distance;
	private Vector3 origin;

	private void Awake()
	{
		cellGrid = FindFirstObjectByType<CellGrid>();
		distance = Mathf.Cos(cameraTiltAngle * Mathf.Deg2Rad) * cellGrid.Size * padding;
		height = Mathf.Sin(cameraTiltAngle * Mathf.Deg2Rad) * cellGrid.Size * padding;
		origin = new Vector3(0f, -cellGrid.Size / 5f, 0f);
		angle = 0f;
	}

	private void Update()
	{
		angle += orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;
		if (angle >= Mathf.PI * 2f)
		{
			angle -= Mathf.PI * 2f;
		}

		transform.position = origin + new Vector3(Mathf.Cos(angle) * distance, height, Mathf.Sin(angle) * distance);
		transform.LookAt(origin);
	}
}
