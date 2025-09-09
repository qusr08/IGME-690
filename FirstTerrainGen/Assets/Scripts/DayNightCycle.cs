using UnityEngine;

public class DayNightCycle: MonoBehaviour
{
    public float CycleTime;

    private void Update()
    {
        transform.rotation *= Quaternion.Euler((360f / CycleTime) * Time.deltaTime, 0f, 0f);
    }
}
