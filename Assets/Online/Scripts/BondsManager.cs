using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BondsManager : NetworkBehaviour
{
    public static GameObject selected;
    public static bool binding;
    public bool selfBinding;
    private Vector3 oldVectorFromTarget;
    private static Color selectedOldColor;
    private Vector3 input;
    private new Rigidbody2D rigidbody;
    private List<Vector3> offsets;
    private int counter;
    private SpriteRenderer spriteRenderer;
    private List<GameObject> connectedObjects;
    public NetworkVariable<Vector2> velocity = new(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        if (IsOwner)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            oldVectorFromTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        }
    }

    void Update()
    {
        if (!IsOwner || !SetupManager.setupActive) return;

        input = Input.mousePosition;
        input.z = 10;
        input = Camera.main.ScreenToWorldPoint(input);

        if (Input.GetButton("Fire2") && selected != null && !binding)
        {
            RaycastHit2D hit = Physics2D.Raycast(input, new Vector2(0, 0));
            if (!hit)
            {
                selected.GetComponent<SpriteRenderer>().color = selectedOldColor;
                selected = null;
                oldVectorFromTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
            else if (hit.collider.gameObject.GetComponent("BondsManager") == null)
            {
                selected.GetComponent<SpriteRenderer>().color = selectedOldColor;
                selected = null;
                oldVectorFromTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
        }
    }

    void FixedUpdate()
    {
        if (!SetupManager.setupActive) return;

        rigidbody.velocity = velocity.Value;

        if (!IsOwner) return; 

        if (selfBinding)
        {
            selected.transform.position = Vector3.MoveTowards(selected.transform.position, transform.position, 
                5 * Time.fixedDeltaTime);
            for (int i = 0; i < connectedObjects.Count; i++)
            {
                connectedObjects[i].transform.position = selected.transform.position + offsets[i];
            }
            if (((oldVectorFromTarget - (transform.position - selected.transform.position))
                / Time.fixedDeltaTime).sqrMagnitude < 2.5 * 2.5) counter++;

            oldVectorFromTarget = transform.position - selected.transform.position;
            if (counter > 10)
            {
                binding = false;
                selfBinding = false;
                selected.GetComponent<SpriteRenderer>().color = selectedOldColor;
                oldVectorFromTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
                selected.GetComponent<BondsManager>().velocity.Value = Vector2.zero;
                velocity.Value = Vector2.zero;
                selected = null;
                counter = 0;
            }
        }
        else
        {
            velocity.Value = Vector2.zero;
        }
    }

    void OnMouseOver()
    {
        if (!Input.GetButton("Fire2") || UIManager.modeSelected != 4 || selected == gameObject || binding ||
            !IsOwner || !SetupManager.setupActive || !SetupManager.moveableObjects.Contains(gameObject))
            return;

        if (!SetupManager.moveableObjects.Contains(selected) && selected != null) return;

        if (selected != null && Physics2D.RaycastAll(selected.transform.position, 
            transform.position - selected.transform.position)[1].transform == transform)
        {
            if (selected.GetComponent<SensorManager>() == null || 
                gameObject.GetComponent<SensorManager>() == null)
            {
                binding = true;
                selfBinding = true;

                offsets = new List<Vector3>();
                connectedObjects = SetupManager.CountConections(selected);
                foreach (GameObject connectedObject in connectedObjects)
                {
                    offsets.Add(connectedObject.transform.position - selected.transform.position);
                }
            }
        }
        else
        {
            if (selected != null) selected.GetComponent<SpriteRenderer>().color = selectedOldColor;

            selected = gameObject;
            selectedOldColor = spriteRenderer.color;
            spriteRenderer.color = new Color(255, 255, 255);
        }
    }

    void OnCollisionEnter2D(Collision2D collider) 
    {
        FinishBinding(collider);
    }

    void OnCollisionStay2D(Collision2D collider)
    {
         FinishBinding(collider);
    }

    void FinishBinding(Collision2D collider)
    {
        if (!selfBinding || selected != collider.gameObject || !IsOwner || !SetupManager.setupActive) return;

        selected.GetComponent<BondsManager>().velocity.Value = Vector2.zero;
        velocity.Value = Vector2.zero;
        binding = false;
        selfBinding = false;

        gameObject.AddComponent<FixedJoint2D>();
        selected.AddComponent<FixedJoint2D>();

        foreach (FixedJoint2D joint in GetComponents<FixedJoint2D>())
        {
            if (joint.connectedBody == null)
            {
                joint.connectedBody = selected.GetComponent<Rigidbody2D>();
                joint.enableCollision = true;
            }
        }
        foreach (FixedJoint2D joint in selected.GetComponents<FixedJoint2D>())
        {
            if (joint.connectedBody == null)
            {
                joint.connectedBody = rigidbody;
                joint.enableCollision = true;
            }
        }
        oldVectorFromTarget = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        selected.GetComponent<SpriteRenderer>().color = selectedOldColor;
        selected = null;
        counter = 0;
    }
}
