using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Offline
{
    public class RoundManager : MonoBehaviour
    {
        public const float buildTime = 30;
        public const float battleTime = 15;
        public static GameObject drawDisplay;
        public static GameObject startDisplay;
        public static GameObject timeDisplay;
        public static GameObject roundDisplay;
        public static GameObject buildTimeDisplay;
        public static GameObject battleTimeDisplay;
        private const int numRounds = 12;
        private float timeTillRoundChanged;
        public static float timeSinceBattleStart;

        private void Start()
        {
            timeSinceBattleStart = 0;
            timeTillRoundChanged = 0;
        }

        public void UILoaded()
        {
            SetupManager.moveableObjects = new HashSet<GameObject>();

            StartCoroutine(ShowDisplay(startDisplay, 2));
            StartCoroutine(StartGame());
        }

        IEnumerator ShowDisplay(GameObject display, float displayTime)
        {
            display.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            display.SetActive(false);
        }

        void PrepBuildMode(int round)
        {
            if (round != -1) StartCoroutine(ShowDisplay(buildTimeDisplay, 2));

            timeTillRoundChanged = buildTime;
            SetupManager.setupActive = true;
            foreach (GameObject item in CreationManager.objectHashSet)
            {
                Rigidbody2D rigidbody = item.GetComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0;
                rigidbody.freezeRotation = true;

                item.GetComponent<SetupManager>().numBattles++;
            }
            CreationManager.money += 1000;
            CreationManager.moneyDisplay.GetComponent<TextMeshProUGUI>().text = CreationManager.money.ToString();
            timeSinceBattleStart = 0;
        }

        void PrepBattleMode(int round)
        {
            roundDisplay.GetComponent<TextMeshProUGUI>().text = ((round - 1) / 2 + 1) + "";
            StartCoroutine(ShowDisplay(battleTimeDisplay, 2));
            timeTillRoundChanged = battleTime;
            SetupManager.moveableObjects = new HashSet<GameObject>();
            SetupManager.setupActive = false;
            BondsManager.binding = false;
            BondsManager.selected = null;
            foreach (GameObject item in CreationManager.objectHashSet)
            {
                SetupManager setupManager = item.GetComponent<SetupManager>();
                setupManager.selected = false;

                Rigidbody2D rigidbody = item.GetComponent<Rigidbody2D>();
                rigidbody.gravityScale = setupManager.gravity;
                rigidbody.freezeRotation = false;

                item.GetComponent<BondsManager>().selfBinding = false;
                item.GetComponent<Collider2D>().isTrigger = false;
            }
        }

        void FixedUpdate()
        {
            if (!SetupManager.setupActive) timeSinceBattleStart += Time.fixedDeltaTime;

            if (timeTillRoundChanged > 0)
            {
                timeTillRoundChanged -= Time.fixedDeltaTime;
                timeDisplay.GetComponent<TextMeshProUGUI>().text = (Mathf.Round(timeTillRoundChanged * 10) / 10) + "";
            }
        }

        void Draw()
        {
            drawDisplay.SetActive(true);
        }

        IEnumerator StartGame()
        {
            PrepBuildMode(-1);
            for (int i = 0; i < numRounds; i++)
            {
                if (SetupManager.setupActive)
                {
                    yield return new WaitForSeconds(buildTime);
                    PrepBattleMode(i);
                }
                else
                {
                    yield return new WaitForSeconds(battleTime);
                    PrepBuildMode(i);
                }
            }
            Draw();
        }
    }
}
