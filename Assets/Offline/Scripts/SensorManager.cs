using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Offline
{
    public class SensorManager : MonoBehaviour
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
        private Vector3 input;
        [SerializeField] private Vector3 offset;

        private void Start()
        {

            biggestID = 0;
            lastValue = 0;
            strings = new List<string>();
            strings.Add("");
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
            if (sensorUI.activeSelf)
            {
                UIManager.ClosePropertiesUI(typeof(SensorManager), sensorUI);

                if (gameObject == UIManager.UIopen)
                {
                    strings[0] = sensorUI.transform.Find("InputDirection").
                        GetComponent<TMP_InputField>().text;
                }
            }
            sensorIDDisplay.transform.position = transform.position;
        }

        private void FixedUpdate()
        {
            (float value, int index) number;
            number = Parser.MainParse(strings[0], gameObject);
            if (number.index != -1 && number.value != lastValue)
            {
                lastValue = number.value;
                transform.eulerAngles = new Vector3(0, 0, number.value - 90);
            }
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(
                (transform.eulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + 90) *
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
            UIManager.OpenPropertiesUI(sensorUI, gameObject, offset, strings);
        }
    }
}
