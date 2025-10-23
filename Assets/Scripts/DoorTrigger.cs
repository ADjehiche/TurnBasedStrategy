using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private GameObject door;
    private GameObject key;
    private const String KEY_TAG = "key";
    private const String DOOR_NAME = "Door";
    private const String KEY_NAME = "Key";

    void Start()
    {
        door = GameObject.Find(DOOR_NAME);
        key = GameObject.Find(KEY_NAME);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(KEY_TAG))
        {
            Destroy(door);
            Destroy(other.gameObject);
            Destroy(key);
        }
    }
}
