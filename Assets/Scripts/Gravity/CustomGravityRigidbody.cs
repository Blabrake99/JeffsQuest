using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    float floatDelay;
    Rigidbody body;
    [SerializeField]
    bool floatToSleep = false;
    Vector3 gravity;
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }
    void FixedUpdate()
    {
        if (floatToSleep)
        {
            floatDelay = 0f;
            return;
        }
        if (body.velocity.sqrMagnitude < 0.0001f)
        {
            floatDelay += Time.deltaTime;
            if (floatDelay >= 1f)
            {
                return;
            }
        }
        else
        {
            floatDelay = 0f;
        }
        gravity = CustomGravity.GetGravity(body.position);

        body.AddForce(gravity, ForceMode.Acceleration);
    }
}
public static class CustomGravity
{
    static List<GravitySource> sources = new List<GravitySource>();
    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        return -g.normalized;
    }
    public static Vector3 GetGravity(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        return g;
    }
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        upAxis = -g.normalized;
        return g;
    }
    public static void Register(GravitySource source)
    {
        Debug.Assert(
            !sources.Contains(source),
            "Duplicate registration of gravity source!", source
        );
        sources.Add(source);
    }
    public static void Unregister(GravitySource source)
    {
        Debug.Assert(
            sources.Contains(source),
            "Unregistration of unknown gravity source!", source
        );
        sources.Remove(source);
    }
}