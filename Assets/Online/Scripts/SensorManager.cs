using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SensorManager : NetworkBehaviour
{
    public static int biggestID = 0;
    public int ID;
    public float value;
    public static GameObject sensorUI;
    private List<string> strings = null;
    public static List<GameObject> sensorList;
    public GameObject sensorIDDisplay;
    private static GameObject canvas;
    private float lastValue;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        if (!IsOwner) return;

        lastValue = 0;
        strings = new List<string>{""};
        transform.eulerAngles = new Vector3(0, 0, -90);
        canvas = GameObject.Find("Canvas");
        sensorIDDisplay = Instantiate(sensorUI.transform.Find("SensorIDDisplay"), transform.position,
            Quaternion.identity).gameObject;
        sensorIDDisplay.SetActive(true);
        sensorIDDisplay.transform.SetParent(canvas.transform, false);
        ID = biggestID + 1;
        sensorIDDisplay.GetComponent<TextMeshProUGUI>().text = ID.ToString();
        sensorIDDisplay.transform.SetSiblingIndex(0);
        biggestID++;
        sensorList.Add(gameObject);
        PowerSourceManager.SensorCreated();
        ExplosiveManager.SensorCreated();
        SensorCreated();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (sensorUI.activeSelf)
        {
            UIManager.ClosePropertiesUI(typeof(SensorManager), sensorUI);

            if (gameObject == UIManager.UIopen && SetupManager.setupActive)
            {
                strings[0] = sensorUI.transform.Find("InputDirection").
                    GetComponent<TMP_InputField>().text;
            }
        }
        sensorIDDisplay.transform.position = transform.position;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        (float value, int index) number;
        number = Parser.MainParse(strings[0], gameObject);
        if (number.index != -1 && number.value != lastValue)
        {
            lastValue = number.value;
            transform.eulerAngles = new Vector3(0, 0, number.value - 90);
        }
        if (number.index == -1)
        {
            if (IsHost && lastValue != 180)
            {
                lastValue = number.value;
                transform.eulerAngles = new Vector3(0, 0, 90);
            }
            if (IsClient && lastValue != 0)
            {
                lastValue = number.value;
                transform.eulerAngles = new Vector3(0, 0, -90);
            }
        }
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(
            (transform.eulerAngles.z + 90) * Mathf.Deg2Rad),Mathf.Sin((transform.eulerAngles.z + 90) * 
            Mathf.Deg2Rad)), 100);
        if (hits.Length > 1)
        {
            value = hits[1].distance - 0.6f;
        }
        else
        {
            value = 100;
        }
    }

    public static void SensorCreated()
    {
        string text = "";
        foreach (GameObject sensor in SensorManager.sensorList)
        {
            text = text + " S" + sensor.GetComponent<SensorManager>().ID;
        }
        sensorUI.transform.Find("SensorListDisplay").GetComponent<TextMeshProUGUI>().text = text;
    }

    private void OnMouseOver()
    {
        if (IsOwner)
        {
            UIManager.OpenPropertiesUI(sensorUI, gameObject, offset, strings);
        }
    }
}
