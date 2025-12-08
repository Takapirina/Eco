using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; 
    [SerializeField] private Vector3 offset; 
    [SerializeField] private float smoothTime = 0.025f;

    public Vector3 CurrentOffset => offset;  
    public float currentTime => smoothTime;
    public void SetOffset(Vector3 newOffset) => offset = newOffset;
    public void SetSmoothTime(float newSmoothTime) => smoothTime = newSmoothTime;
    private Vector3 _velocity; 

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            smoothTime
        );

    }
}