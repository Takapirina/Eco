using System.Collections;
using UnityEngine;

public class CameraAreaTrigger : MonoBehaviour
{
    [Header("Riferimento alla camera")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Impostazioni di questa area")]
    [SerializeField] private Vector3 areaOffset = new Vector3(0, 10, -8);
    [SerializeField] private Vector3 areaRotationEuler = new Vector3(45, 0, 0);

    [Header("Transizione")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private AnimationCurve transitionCurve = null;

    [Header("Opzioni")]
    [SerializeField] private bool triggerOnce = false;

    private bool _used;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (cameraFollow == null) return;

        if (triggerOnce && _used) return;
        _used = true;

        cameraFollow.TransitionTo(areaOffset, areaRotationEuler, transitionDuration, transitionCurve);
    }
    private void OnDrawGizmos()
    {
        if (!TryGetComponent<BoxCollider>(out BoxCollider boxCollider)) return;
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCollider.transform.position, boxCollider.transform.rotation, boxCollider.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}