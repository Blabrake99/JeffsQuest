using UnityEngine;

public class GravityPlane : GravitySource
{
	[SerializeField]
	public float gravity = 9.81f;
	[SerializeField, Min(0f)]
	float range = 20f;
	public override Vector3 GetGravity(Vector3 position)
	{
		Vector3 up = transform.up;
		float distance = Vector3.Dot(up, position - transform.position);
		if (distance > range)
		{
			return Vector3.zero;
		}
		float g = -gravity;
		if (distance > 0f)
		{
			g *= 1f - distance / range;
		}
		return g * up;
	}
}