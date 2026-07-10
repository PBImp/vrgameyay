using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    public Transform source;
    public Vector3 rotationOffset;

    private void LateUpdate()
    {
        transform.rotation = source.rotation * Quaternion.Euler(rotationOffset);
    }
}
