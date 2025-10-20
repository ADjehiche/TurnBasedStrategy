using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;
    public float throwForce = 500f;
    public float pickUpRange = 5f;
    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private int LayerNumber;
    
    private PlayerInput playerInput;
    private InputAction pickUpAction;
    private InputAction fireAction;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found. Make sure it's on this GameObject or a parent.");
            return;
        }

        // Get references to the actions we need
        pickUpAction = playerInput.actions["PickUp"];
        fireAction = playerInput.actions["Fire"];
        
        // Setup callbacks for the actions
        pickUpAction.performed += ctx => OnPickUpPerformed();
        fireAction.performed += ctx => OnFirePerformed();
    }

    void OnEnable()
    {
        pickUpAction?.Enable();
        fireAction?.Enable();
    }

    void OnDisable()
    {
        pickUpAction?.Disable();
        fireAction?.Disable();
    }

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer("holdLayer");
    }
    void Update()
    {
        if (heldObj != null)
        {
            MoveObject();
        }
    }
    
    private void OnPickUpPerformed()
    {
        if (heldObj == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
            {
                if (hit.transform.gameObject.CompareTag("canPickUp"))
                {
                    PickUpObject(hit.transform.gameObject);
                }
            }
        }
        else
        {
            StopClipping();
            DropObject();
        }
    }

    private void OnFirePerformed()
    {
        if (heldObj != null)
        {
            StopClipping();
            ThrowObject();
        }
    }
    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform;
            heldObj.layer = LayerNumber;
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }
    void DropObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObj = null;
    }
    void MoveObject()
    {
        heldObj.transform.position = holdPos.transform.position;
    }
    
    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }
    void StopClipping()
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }
}