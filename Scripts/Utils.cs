using UnityEngine;

public class Utils
{
	public static float Remap(float value, float aLow, float aHigh, float bLow, float bHigh)
	{
		float normal = Mathf.InverseLerp(aLow, aHigh, value);
		float bValue = Mathf.Lerp(bLow, bHigh, normal);
		return bValue;
	}

	public static string GetGameObjectPath(Transform transform)
	{
		string path = transform.name;
		while (transform.parent != null)
		{
			transform = transform.parent;
			path = transform.name + "/" + path;
		}
		return path;
	}
}
