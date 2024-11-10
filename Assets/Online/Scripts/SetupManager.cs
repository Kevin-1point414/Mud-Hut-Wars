using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SetupManager : NetworkBehaviour
{
    public static bool setupActive;
    public static HashSet<GameObject> moveableObjects;
    public bool selected;
    private Vector3 input;
    public int numBattles;
    private List<Vector3> offsets;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;
    public float gravity;
    private List<GameObject> connectedObjects;
    public NetworkVariable<float> currentGravity = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> rotationLocked = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isTrigger = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        if (IsOwner)
        {
            numBattles = -1;
            gravity = rigidbody.gravityScale;
            setupActive = true;
            selected = false;
        }
        collider.isTrigger = false;
        rigidbody.freezeRotation = true;
        rigidbody.gravityScale = currentGravity.Value;

        isTrigger.OnValueChanged += IsTriggerChanged;
        rotationLocked.OnValueChanged += RotationLockedChanged;
        currentGravity.OnValueChanged += CurrentGravityChanged;
    }

    void FixedUpdate()
    {
        if (!IsOwner || !Input.GetButton("Fire1") || !selected) return;

        input = Input.mousePosition;
        input.z = 10;
        input = Camera.main.ScreenToWorldPoint(input);

        transform.position = input;
        for (int i = 0; i < connectedObjects.Count; i++)
        {
            connectedObjects[i].transform.position = transform.position + offsets[i];
        }
    }

    public static List<GameObject> CountConections(GameObject objectToCheck, HashSet<GameObject> 
        connectedObjects = null)
    {
        if (connectedObjects == null) connectedObjects = new HashSet<GameObject>();

        connectedObjects.Add(objectToCheck);
        foreach (FixedJoint2D joint in objectToCheck.GetComponents<FixedJoint2D>())
        {
            if (connectedObjects.Add(joint.connectedBody.gameObject))
            {
                CountConections(joint.connectedBody.gameObject, connectedObjects);
            }
        }
        return new List<GameObject>(connectedObjects);
    }

    void OnMouseDown()
    {
        if (setupActive && IsOwner && moveableObjects.Contains(gameObject))
        {
            connectedObjects = CountConections(gameObject);
            offsets = new List<Vector3>();
            selected = true;
            foreach (GameObject connectedObject in connectedObjects)
            {
                connectedObject.GetComponent<SetupManager>().isTrigger.Value = true;
                offsets.Add(connectedObject.transform.position - transform.position);
            }
        }
    }

    void OnMouseUp()
    {
        if (setupActive && IsOwner && moveableObjects.Contains(gameObject))
        {
            selected = false;
            foreach (GameObject connectedObject in connectedObjects)
            {
                connectedObject.GetComponent<SetupManager>().isTrigger.Value = false;
            }
        }
    }

    public void CurrentGravityChanged(float previous, float current)
    {
        rigidbody.gravityScale = currentGravity.Value;
    }

    public void RotationLockedChanged(bool previous, bool current)
    {
        rigidbody.freezeRotation = current;
    }

    public void IsTriggerChanged(bool previous, bool current)
    {
        collider.isTrigger = isTrigger.Value;
    }
}
