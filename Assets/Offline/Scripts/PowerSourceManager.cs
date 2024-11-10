using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Offline
{
    public class PowerSourceManager : MonoBehaviour
    {
        private float direction;
        private float power;
        private List<string> strings;
        private const float powerMultiplyer = 10;
        public static GameObject powerSourceUI;
        private new Rigidbody2D rigidbody;
        private Vector3 input;
        [SerializeField] private Vector3 offset;

        private void Start()
        {
            strings = new List<string>();
            strings.Add("");
            strings.Add("");
            rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            rigidbody.AddForce(new Vector2(Time.deltaTime * power * Mathf.Cos(direction * Mathf.Deg2Rad), Time.deltaTime *
                power * Mathf.Sin(direction * Mathf.Deg2Rad)));

            if (powerSourceUI.activeSelf)
            {
                UIManager.ClosePropertiesUI(typeof(PowerSourceManager), powerSourceUI);

                if (gameObject == UIManager.UIopen)
                {
                    strings[0] = powerSourceUI.transform.Find("InputDirection").
                        GetComponent<TMP_InputField>().text;
                    strings[1] = powerSourceUI.transform.Find("InputPower").
                        GetComponent<TMP_InputField>().text;
                }
            }
        }

        private void FixedUpdate()
        {
            if (SetupManager.setupActive) return;

            (float value, int index) number;
            number = Parser.MainParse(strings[0], gameObject);
            if (number.index != -1) direction = number.value;

            number = Parser.MainParse(strings[1], gameObject);
            if (number.index != -1) power = Mathf.Clamp(number.value, -100, 100) * powerMultiplyer;
        }

        private void OnMouseOver()
        {
            UIManager.OpenPropertiesUI(powerSourceUI, gameObject, offset, strings);
        }

        public static void SensorCreated()
        {
            string text = "";
            foreach (GameObject sensor in SensorManager.sensorList)
            {
                text = text + " S" + sensor.GetComponent<SensorManager>().ID;
            }
            powerSourceUI.transform.Find("SensorListDisplay").GetComponent<TextMeshProUGUI>().text = text;
        }
    }
}
