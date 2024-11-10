using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class CreationManager : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkObjectReference> created = new();
    public static HashSet<GameObject> objectHashSet;
    public static GameObject moneyDisplay;
    public static float money;
    private Vector3 input;
    bool inputFire2 = false;
    bool inputDelay = false;
    [SerializeField] private GameObject box;
    [SerializeField] private GameObject powerSource;
    [SerializeField] private GameObject sensor;
    [SerializeField] private GameObject explosive;

    void Start()
    {
        if (IsOwner)
        {
            money = 0;
            objectHashSet = new HashSet<GameObject>();
            created.OnValueChanged += ObjectCreated;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        input = Input.mousePosition;
        input.z = 10;
        input = Camera.main.ScreenToWorldPoint(input);

        if (!inputDelay) inputFire2 = Input.GetButton("Fire2");

        if (Physics2D.Raycast(input, new Vector2(0, 0)).collider == null && inputFire2
            && SetupManager.setupActive)
        {
            inputFire2 = false;
            inputDelay = true;
            switch (UIManager.modeSelected) 
            {
                case 1:
                    if (money >= 50)
                    {
                        CreateObjectServerRpc("explosive", new Vector3(input.x, input.y, 0));
                        money -= 50;
                        moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                    }
                    break;
                case 2:
                    if (money >= 50)
                    {
                        CreateObjectServerRpc("powerSource", new Vector3(input.x, input.y, 0));
                        money -= 50;
                        moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                    }
                    break;
                case 3:
                    if (money >= 40)
                    {
                        CreateObjectServerRpc("box", new Vector3(input.x, input.y, 0));
                        money -= 40;
                        moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                    }
                    break;
                case 5:
                    if (money >= 20)
                    {
                        CreateObjectServerRpc("sensor", new Vector3(input.x, input.y, 0));
                        money -= 20;
                        moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                    }
                    break;
            }
        }
        if (!Input.GetButton("Fire2")) inputDelay = false;
    }

    void ObjectCreated(NetworkObjectReference previous, NetworkObjectReference current)
    {
        created.Value.TryGet(out NetworkObject networkObject);
        SetupManager.moveableObjects.Add(networkObject.gameObject);
        objectHashSet.Add(networkObject.gameObject);
    }

    [ServerRpc]
    void CreateObjectServerRpc(string name, Vector3 spawnLocation, ServerRpcParams serverRpcParams = default)
    {
        GameObject prefab = null;
        switch (name)
        {
            case "box":
                prefab = box;
                break;
            case "sensor":
                prefab = sensor;
                break;
            case "powerSource":
                prefab = powerSource;
                break;
            case "explosive":
                prefab = explosive;
                break;
        }
        GameObject newObject = Instantiate(prefab, spawnLocation, Quaternion.identity);
        newObject.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        created.Value = new NetworkObjectReference(newObject);
    }
}
