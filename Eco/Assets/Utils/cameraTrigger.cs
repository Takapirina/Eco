using System;
using UnityEngine;

public class CameraAreaTrigger : MonoBehaviour
{
    [Header("Riferimento alla camera")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Impostazioni dentro l'area")]
    [SerializeField] private Vector3 areaOffset = new Vector3(0, 10, -8);
    [SerializeField] private Vector3 areaRotationEuler = new Vector3(45, 0, 0);
    [SerializeField] private float smoothTime = 0.025f;

    private Vector3 _originalOffset;
    private Quaternion _originalRotation;
    private bool _hasOriginalStored = false;

    private void OnTriggerEnter(Collider other)
    {
        Console.WriteLine("CameraAreaTrigger: OnTriggerEnter");
        if (!other.CompareTag("Player")) return;

        if (!_hasOriginalStored)
        {
            _originalOffset = cameraFollow.CurrentOffset;
            _originalRotation = cameraFollow.transform.rotation;
            _hasOriginalStored = true;
        }

        cameraFollow.SetOffset(areaOffset);

        cameraFollow.transform.rotation = Quaternion.Euler(areaRotationEuler);
    }

    private void OnTriggerExit(Collider other)
    {
        Console.WriteLine("CameraAreaTrigger: OnTriggerExit");
        if (!other.CompareTag("Player")) return;

        if (_hasOriginalStored)
        {
            cameraFollow.SetOffset(_originalOffset);
            cameraFollow.transform.rotation = _originalRotation;
        }
    }
}