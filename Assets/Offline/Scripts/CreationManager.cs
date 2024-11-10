using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Offline
{
    public class CreationManager : MonoBehaviour
    {
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
            money = 0;
            objectHashSet = new HashSet<GameObject>();
        }

        void Update()
        {
            input = Input.mousePosition;
            input.z = 10;
            input = Camera.main.ScreenToWorldPoint(input);

            if (!inputDelay) inputFire2 = Input.GetButton("Fire2");

            if (Physics2D.Raycast(input, new Vector2(0, 0)).collider == null && inputFire2 && 
                SetupManager.setupActive)
            {
                inputFire2 = false;
                inputDelay = true;
                switch (UIManager.modeSelected)
                {
                    case 1:
                        if (money >= 60)
                        {
                            GameObject newObject = Instantiate(explosive, new Vector3(input.x, input.y, 0),
                                Quaternion.identity);
                            objectHashSet.Add(newObject);
                            money -= 60;
                            moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                        }
                        break;
                    case 2:
                        if (money >= 80)
                        {
                            GameObject newObject = Instantiate(powerSource, new Vector3(input.x, input.y, 0),
                                Quaternion.identity);
                            objectHashSet.Add(newObject);
                            money -= 80;
                            moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                        }
                        break;
                    case 3:
                        if (money >= 20)
                        {
                            GameObject newObject = Instantiate(box, new Vector3(input.x, input.y, 0),
                                Quaternion.identity);
                            objectHashSet.Add(newObject);
                            money -= 20;
                            moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                        }
                        break;
                    case 5:
                        if (money >= 1)
                        {
                            GameObject newObject = Instantiate(sensor, new Vector3(input.x, input.y, 0),
                                Quaternion.identity);
                            objectHashSet.Add(newObject);
                            money -= 1;
                            moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " + money;
                        }
                        break;
                }
            }
            if (!Input.GetButton("Fire2")) inputDelay = false;
        }
    }
}
