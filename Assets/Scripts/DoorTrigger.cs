using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private GameObject door;
    private GameObject key;

    void Start()
    {
        door = GameObject.Find("Door");
        key = GameObject.Find("Key");
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("key"))
        {
            Destroy(door);
            Destroy(other.gameObject);
            Destroy(key);
        }
    }
}
