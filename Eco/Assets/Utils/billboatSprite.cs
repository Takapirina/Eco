using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 forward = cam.transform.rotation * Vector3.forward;
        Vector3 up = cam.transform.rotation * Vector3.up;

        transform.rotation = Quaternion.LookRotation(forward, up);
    }
}