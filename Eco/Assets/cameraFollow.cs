using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Default")]
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    [SerializeField] private Vector3 rotationEuler = new Vector3(45, 0, 0);

    [Header("Follow Smoothing (micro jitter)")]
    [Tooltip("SmoothDamp per seguire il target una volta arrivati alla posizione desiderata.")]
    [SerializeField] private float followSmoothTime = 0.025f;

    private Vector3 _velocity;

    // stato corrente (quello usato per calcolare desiredPosition)
    private Vector3 _currentOffset;
    private Quaternion _currentRotation;

    // transizione
    private bool _isTransitioning;
    private float _t;
    private float _duration = 0.25f;
    private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 _fromOffset, _toOffset;
    private Quaternion _fromRot, _toRot;

    public Vector3 CurrentOffset => _currentOffset;
    public Quaternion CurrentRotation => _currentRotation;

    private void Awake()
    {
        _currentOffset = offset;
        _currentRotation = Quaternion.Euler(rotationEuler);
        transform.rotation = _currentRotation;
    }

    private void LateUpdate()
    {
        if (target == null) return;


        if (_isTransitioning)
        {
            _t += Time.deltaTime / Mathf.Max(0.0001f, _duration);
            float u = Mathf.Clamp01(_t);
            float eased = _curve != null ? _curve.Evaluate(u) : u;

            _currentOffset = Vector3.LerpUnclamped(_fromOffset, _toOffset, eased);
            _currentRotation = Quaternion.SlerpUnclamped(_fromRot, _toRot, eased);

            if (u >= 1f)
            {
                _isTransitioning = false;
                _currentOffset = _toOffset;
                _currentRotation = _toRot;
            }
        }

        Vector3 desiredPosition = target.position + _currentOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            followSmoothTime
        );

        transform.rotation = _currentRotation;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
    public void TransitionTo(Vector3 newOffset, Vector3 newRotationEuler, float duration, AnimationCurve curve = null)
    {
        _fromOffset = _currentOffset;
        _toOffset = newOffset;

        _fromRot = _currentRotation;
        _toRot = Quaternion.Euler(newRotationEuler);

        _duration = Mathf.Max(0.0001f, duration);
        _curve = curve != null ? curve : AnimationCurve.EaseInOut(0, 0, 1, 1);

        _t = 0f;
        _isTransitioning = true;
    }


    public void SnapTo(Vector3 newOffset, Vector3 newRotationEuler)
    {
        _isTransitioning = false;
        _currentOffset = newOffset;
        _currentRotation = Quaternion.Euler(newRotationEuler);
        transform.rotation = _currentRotation;
    }
}