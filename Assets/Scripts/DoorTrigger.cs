using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private string keyTag = "key";

    [Header("Hinge")]
    [SerializeField] private Transform hingePoint;
    [SerializeField] private Vector3 hingeLocalOffset = Vector3.zero;

    [Header("Open settings")]
    [SerializeField] private float openSpeed = 90f;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private bool useYAxis = true;

    private bool isOpening = false;
    private GameObject key;

    private const String DOOR_NAME = "Door";
    private const String KEY_NAME = "Key";

    void Start()
    {
        if (door == null)
            door = GameObject.Find(DOOR_NAME);

        key = GameObject.Find(KEY_NAME);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isOpening) return;

        if (other.CompareTag(keyTag))
        {
            if (other.gameObject != null)
                Destroy(other.gameObject);
            if (key != null)
                Destroy(key);

            if (door != null)
            {
                StartCoroutine(OpenDoorCoroutine());
            }
        }
    }

    private IEnumerator OpenDoorCoroutine()
    {
        isOpening = true;
        Transform doorT = door.transform;

        Vector3 hingeWorldPos = (hingePoint != null) ? hingePoint.position : doorT.TransformPoint(hingeLocalOffset);
        Vector3 axis = useYAxis ? Vector3.up : Vector3.right;

        float totalRotated = 0f;
        float targetRotation = Mathf.Abs(openAngle);

        while (Mathf.Abs(totalRotated) < targetRotation)
        {
            float step = openSpeed * Time.deltaTime;
            if (Mathf.Abs(totalRotated + step) > targetRotation)
            {
                step = targetRotation - Mathf.Abs(totalRotated);
            }

            float rotationStep = Mathf.Sign(openAngle) * step;
            doorT.RotateAround(hingeWorldPos, axis, rotationStep);
            
            totalRotated += step;
            yield return null;
        }

        isOpening = false;

        Destroy(this.gameObject);
    }
}
