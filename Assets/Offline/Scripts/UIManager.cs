using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Offline
{
    public class UIManager : MonoBehaviour
    {
        public static ushort modeSelected;
        private ushort input;
        public static GameObject UIopen;

        [SerializeField] private Color selectedColor;
        [SerializeField] private GameObject roundManagerPrefabs;
        private Color buttonColor;

        private Image ExplosiveCreationImage;
        private Image PowerSourceCreationImage;
        private Image BoxCreationImage;
        private Image BondCreationImage;
        private Image SensorCreationImage;

        void Start()
        {
            GetComponent<Canvas>().worldCamera = GameObject.Find("Player(Clone)").GetComponent<Camera>();
            modeSelected = 0;
            UIopen = null;

            PowerSourceManager.powerSourceUI = GameObject.Find("PowerSourceUI");
            PowerSourceManager.powerSourceUI.SetActive(false);

            ExplosiveManager.explosiveUI = GameObject.Find("ExplosiveUI");
            ExplosiveManager.explosiveUI.SetActive(false);

            SensorManager.sensorUI = GameObject.Find("SensorUI");
            SensorManager.sensorUI.SetActive(false);
            SensorManager.sensorList = new List<GameObject>();

            CreationManager.moneyDisplay = GameObject.Find("MoneyDisplay");
            CreationManager.moneyDisplay.GetComponent<TextMeshProUGUI>().text = "1000";

            MudHutManager.winDisplay = GameObject.Find("WinDisplay");
            MudHutManager.winDisplay.SetActive(false);
            MudHutManager.loseDisplay = GameObject.Find("LoseDisplay");
            MudHutManager.loseDisplay.SetActive(false);

            RoundManager.drawDisplay = GameObject.Find("DrawDisplay");
            RoundManager.drawDisplay.SetActive(false);
            RoundManager.startDisplay = GameObject.Find("StartDisplay");
            RoundManager.startDisplay.SetActive(false);
            RoundManager.buildTimeDisplay = GameObject.Find("BuildTimeDisplay");
            RoundManager.buildTimeDisplay.SetActive(false);
            RoundManager.battleTimeDisplay = GameObject.Find("BattleTimeDisplay");
            RoundManager.battleTimeDisplay.SetActive(false);
            RoundManager.timeDisplay = GameObject.Find("TimeDisplay");
            RoundManager.roundDisplay = GameObject.Find("RoundDisplay");

            ExplosiveCreationImage = transform.Find("ExplosiveCreationButton").GetComponent<Image>();
            PowerSourceCreationImage = transform.Find("PowerSourceCreationButton").GetComponent<Image>();
            BoxCreationImage = transform.Find("BoxCreationButton").GetComponent<Image>();
            BondCreationImage = transform.Find("BondCreationButton").GetComponent<Image>();
            SensorCreationImage = transform.Find("SensorCreationButton").GetComponent<Image>();

            buttonColor = ExplosiveCreationImage.color;

            transform.Find("ExplosiveCreationButton").GetComponent<Button>().onClick.AddListener(ExplosiveCreation);
            transform.Find("PowerSourceCreationButton").GetComponent<Button>().onClick.AddListener(PowerSourceCreation);
            transform.Find("BoxCreationButton").GetComponent<Button>().onClick.AddListener(BoxCreation);
            transform.Find("BondCreationButton").GetComponent<Button>().onClick.AddListener(BondCreation);
            transform.Find("SensorCreationButton").GetComponent<Button>().onClick.AddListener(SensorCreation);

            GameObject roundManagerGameObject = Instantiate(roundManagerPrefabs);

            roundManagerGameObject.GetComponent<RoundManager>().UILoaded();
        }

        void ExplosiveCreation()
        {
            modeSelected = 1;
            ExplosiveCreationImage.color = selectedColor;
            PowerSourceCreationImage.color = buttonColor;
            BoxCreationImage.color = buttonColor;
            BondCreationImage.color = buttonColor;
            SensorCreationImage.color = buttonColor;
        }
        void PowerSourceCreation()
        {
            modeSelected = 2;
            ExplosiveCreationImage.color = buttonColor;
            PowerSourceCreationImage.color = selectedColor;
            BoxCreationImage.color = buttonColor;
            BondCreationImage.color = buttonColor;
            SensorCreationImage.color = buttonColor;
        }
        void BoxCreation()
        {
            modeSelected = 3;
            ExplosiveCreationImage.color = buttonColor;
            PowerSourceCreationImage.color = buttonColor;
            BoxCreationImage.color = selectedColor;
            BondCreationImage.color = buttonColor;
            SensorCreationImage.color = buttonColor;
        }
        void BondCreation()
        {
            modeSelected = 4;
            ExplosiveCreationImage.color = buttonColor;
            PowerSourceCreationImage.color = buttonColor;
            BoxCreationImage.color = buttonColor;
            BondCreationImage.color = selectedColor;
            SensorCreationImage.color = buttonColor;
        }
        void SensorCreation()
        {
            modeSelected = 5;
            ExplosiveCreationImage.color = buttonColor;
            PowerSourceCreationImage.color = buttonColor;
            BoxCreationImage.color = buttonColor;
            BondCreationImage.color = buttonColor;
            SensorCreationImage.color = selectedColor;
        }

        public static void OpenPropertiesUI(GameObject UI, GameObject _gameObject, Vector3 offset, List<string> strings)
        {
            if (!Input.GetButton("Fire1")) return;

            for (int i = 0; i < UI.transform.childCount; i++)
            {
                try
                {
                    UI.transform.GetChild(i).GetComponent<TMP_InputField>().text = strings[i];
                }
                catch { }
            }
            UI.SetActive(true);
            UI.transform.position = _gameObject.transform.position + offset;
            UIopen = _gameObject;
        }

        public static void ClosePropertiesUI(Type type, GameObject UI)
        {
            if (!Input.GetButton("Fire1")) return;

            Vector3 _input = Input.mousePosition;
            _input.z = 10;
            _input = Camera.main.ScreenToWorldPoint(_input);

            RaycastHit2D[] hits = Physics2D.RaycastAll(_input, new Vector2(0, 0));
            bool close = true;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.GetComponent(type) != null || hit.collider.gameObject == UI) close = false;
            }
            UI.SetActive(!close);
        }

        void Update()
        {
            if (PowerSourceManager.powerSourceUI.transform.Find("InputPower").GetComponent<TMP_InputField>().isFocused
                || PowerSourceManager.powerSourceUI.transform.Find("InputDirection").GetComponent<TMP_InputField>()
                .isFocused || ExplosiveManager.explosiveUI.transform.Find("InputActive").GetComponent<TMP_InputField>()
                .isFocused) return;

            if (ushort.TryParse(Input.inputString, out input)) modeSelected = input;

            if (Input.mouseScrollDelta.y > 0.5f)
            {
                modeSelected++;
                if (modeSelected > 5) modeSelected = 1;
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                modeSelected--;
                if (modeSelected < 1) modeSelected = 5;
            }
            switch (modeSelected)
            {
                case 1:
                    ExplosiveCreation();
                    break;
                case 2:
                    PowerSourceCreation();
                    break;
                case 3:
                    BoxCreation();
                    break;
                case 4:
                    BondCreation();
                    break;
                case 5:
                    SensorCreation();
                    break;
            }
        }
    }
}
