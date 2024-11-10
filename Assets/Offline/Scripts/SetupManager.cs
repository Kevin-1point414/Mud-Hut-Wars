using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Offline
{
    public class SetupManager : MonoBehaviour
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

        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            numBattles = -1;
            gravity = rigidbody.gravityScale;
            setupActive = true;
            selected = false;

            rigidbody.freezeRotation = true;
        }

        void FixedUpdate()
        {
            if (!Input.GetButton("Fire1") || !selected) return;

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
            if (setupActive && moveableObjects.Contains(gameObject))
            {
                connectedObjects = CountConections(gameObject);
                offsets = new List<Vector3>();
                selected = true;
                foreach (GameObject connectedObject in connectedObjects)
                {
                    connectedObject.GetComponent<Collider2D>().isTrigger = true;
                    offsets.Add(connectedObject.transform.position - transform.position);
                }
            }
        }

        void OnMouseUp()
        {
            if (setupActive && moveableObjects.Contains(gameObject))
            {
                selected = false;
                foreach (GameObject connectedObject in connectedObjects)
                {
                    connectedObject.GetComponent<Collider2D>().isTrigger = false;
                }
            }
        }
    }
}
