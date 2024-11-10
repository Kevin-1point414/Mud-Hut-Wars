using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ExplosiveManager : NetworkBehaviour
{
    private static float radius;
    private const float destructionRadius = 2;
    public bool exploding;
    private const float bondsDestructionRadius = 5;
    private const float falloff = 10;
    private const float power = 100;
    public static GameObject explosiveUI;
    private List<string> strings;
    private ContactFilter2D contactFilter;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        if (!IsOwner) return;

        exploding = false;
        strings = new List<string>{""};
        contactFilter = new ContactFilter2D();
        radius = power / falloff; 
    }

    private void Update()
    {
        if (!IsOwner || !explosiveUI.activeSelf) return;

        UIManager.ClosePropertiesUI(typeof(ExplosiveManager), explosiveUI);

        if (gameObject == UIManager.UIopen)
        {
            strings[0] = explosiveUI.transform.Find("InputActive").GetComponent<TMP_InputField>().text;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner && !SetupManager.setupActive)
        {
            (float value, int index) = Parser.MainParse(strings[0], gameObject);
            if (index != -1 && value >= 100 && !exploding) Explode();
        }
    }

    private void OnMouseOver()
    {
        if (IsOwner) UIManager.OpenPropertiesUI(explosiveUI, gameObject, offset, strings);
    }

    public static void SensorCreated()
    {
        string text = "";
        foreach (GameObject sensor in SensorManager.sensorList)
        {
            text = text + " S" + sensor.GetComponent<SensorManager>().ID;
        }
        explosiveUI.transform.Find("SensorListDisplay").GetComponent<TextMeshProUGUI>().text = text;
    }

    public void Explode()
    {
        exploding = true;
        List<Collider2D> effectedObjects = new List<Collider2D>();
        Physics2D.OverlapCircle(transform.position, radius, contactFilter.NoFilter(), effectedObjects);
        foreach (Collider2D collider in effectedObjects)
        {
            if (collider != null)
            {
                if (collider.gameObject.GetComponent<Rigidbody2D>() != null)
                {
                    float distancesqr = (transform.position - collider.transform.position).sqrMagnitude;
                    Rigidbody2D rigidbody = collider.attachedRigidbody;

                    if (distancesqr < bondsDestructionRadius * bondsDestructionRadius)
                    {
                        FixedJoint2D[] joints1 = collider.gameObject.GetComponents<FixedJoint2D>();
                        for (int i = 0; i < joints1.Length; i++)
                        {
                            try
                            {
                                FixedJoint2D[] joints2 = joints1[i].connectedBody.GetComponents<FixedJoint2D>();
                                for (int j = 0; j < joints2.Length; j++)
                                {
                                    if (joints2[j] == joints1[i])
                                    {
                                        DestroyServerRpc(new NetworkObjectReference(joints2[j].gameObject), j);
                                    }
                                }
                                DestroyServerRpc(new NetworkObjectReference(collider.gameObject), i);
                            }
                            catch { }
                        }
                    }
                    if (distancesqr < destructionRadius * destructionRadius)
                    {
                        if (UIManager.UIopen == collider.gameObject)
                        {
                            UIManager.UIopen = null;
                            PowerSourceManager.powerSourceUI.SetActive(false);
                            SensorManager.sensorUI.SetActive(false);
                            explosiveUI.SetActive(false);
                        }
                        if (collider.gameObject.GetComponent<SensorManager>() != null)
                        {
                            DestroyServerRpc(new NetworkObjectReference(collider.gameObject), -1, true);
                            SensorManager.sensorList.Remove(collider.gameObject);
                            PowerSourceManager.SensorCreated();
                            SensorCreated();
                            SensorManager.SensorCreated();
                        }
                        else if (collider.gameObject.GetComponent<ExplosiveManager>() != null)
                        {
                            if (!collider.gameObject.GetComponent<ExplosiveManager>().exploding)
                            {
                                collider.gameObject.GetComponent<ExplosiveManager>().Explode();
                            }
                        }
                        else
                        {
                            DestroyServerRpc(new NetworkObjectReference(collider.gameObject));
                        }
                    }
                }
                else
                {
                    if (collider.gameObject.name == "MudHut1") DamageMudHutServerRpc(true, 10);

                    if (collider.gameObject.name == "MudHut2") DamageMudHutServerRpc(false, 10);
                }
            }
        }
        DestroyServerRpc(new NetworkObjectReference(gameObject));
    }

    [ClientRpc]
    void DestroyClientRpc(NetworkObjectReference gameObjectReference, int index, bool sensor)
    {
        Debug.Log("DestroyClientRpc");
        NetworkObject networkObject;
        gameObjectReference.TryGet(out networkObject);
        if (index == -1)
        {
            CreationManager.objectHashSet.Remove(networkObject.gameObject);
            if (sensor)
            {
                if (networkObject.gameObject.GetComponent<SensorManager>().IsOwner)
                {
                    Destroy(networkObject.gameObject.GetComponent<SensorManager>().sensorIDDisplay);
                }
            }
        }
        if (IsHost)
        {
            Debug.Log("is host");
            if (index == -1)
            {
                Destroy(networkObject.gameObject);
                Debug.Log("Destroyed");
            }
            else
            {
                try
                {
                    Destroy(networkObject.gameObject.GetComponents<FixedJoint2D>()[index]);
                }
                catch { }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyServerRpc(NetworkObjectReference gameObjectReference, int index = -1, bool sensor = false)
    {
        DestroyClientRpc(gameObjectReference, index, sensor);
    }

    [ServerRpc(RequireOwnership = false)]
    void DamageMudHutServerRpc(bool hut, float damage)
    {
        if (hut)
        {
            GameObject.Find("RoundManager(Clone)").GetComponent<MudHutManager>().mudHut1Health.Value -= damage;
        }
        else
        {
            GameObject.Find("RoundManager(Clone)").GetComponent<MudHutManager>().mudHut2Health.Value -= damage;
        }
    }
}
