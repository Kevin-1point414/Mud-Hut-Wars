using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class RoundManager : NetworkBehaviour
{
    public const float buildTime = 240;
    public const float battleTime = 120;
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

        if (!IsHost) 
        {
            StartGameServerRpc();
            StartCoroutine(ShowDisplay(startDisplay, 2));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartGameServerRpc()
    {
        StartCoroutine(ShowDisplay(startDisplay, 2));
        StartCoroutine(StartGame());
    }

    IEnumerator ShowDisplay(GameObject display, float displayTime)
    {
        display.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        display.SetActive(false);
    }

    [ClientRpc]
    void ModeChangeClientRpc(bool isBuildMode, int round)
    {
        if (isBuildMode)
        {
            if (round != -1) StartCoroutine(ShowDisplay(buildTimeDisplay, 2));

            timeTillRoundChanged = buildTime;
            SetupManager.setupActive = true;
            foreach (GameObject item in CreationManager.objectHashSet)
            {
                SetupManager setupManager = item.GetComponent<SetupManager>();
                setupManager.currentGravity.Value = 0;
                setupManager.rotationLocked.Value = true;
                setupManager.numBattles++;
            }
            CreationManager.money += 1000;
            CreationManager.moneyDisplay.GetComponent<TextMeshProUGUI>().text = "Money Left: " 
                + CreationManager.money;
            timeSinceBattleStart = 0;
        }
        else
        {
            roundDisplay.GetComponent<TextMeshProUGUI>().text = "Round: " + ((round - 1) / 2 + 1);
            StartCoroutine(ShowDisplay(battleTimeDisplay, 2));
            timeTillRoundChanged = battleTime;
            SetupManager.moveableObjects = new HashSet<GameObject>();
            SetupManager.setupActive = false;
            BondsManager.binding = false;
            BondsManager.selected = null;
            foreach (GameObject item in CreationManager.objectHashSet)
            {
                SetupManager setupManager = item.GetComponent<SetupManager>();
                BondsManager bondsManager = item.GetComponent<BondsManager>();
                setupManager.currentGravity.Value = setupManager.gravity;
                setupManager.rotationLocked.Value = false;
                setupManager.isTrigger.Value = false;
                setupManager.selected = false;
                bondsManager.selfBinding = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (!SetupManager.setupActive) timeSinceBattleStart += Time.fixedDeltaTime;

        if (timeTillRoundChanged > 0)
        {
            timeTillRoundChanged -= Time.fixedDeltaTime;
            timeDisplay.GetComponent<TextMeshProUGUI>().text
                = "Time Left: " + (Mathf.Round(timeTillRoundChanged * 10) / 10);
        }
    }

    [ClientRpc]
    void DrawClientRpc()
    {
        drawDisplay.SetActive(true);
    }

    IEnumerator StartGame()
    {
        ModeChangeClientRpc(true, -1);
        for (int i = 0; i < numRounds; i++)
        {
            if (SetupManager.setupActive)
            {
                yield return new WaitForSeconds(buildTime);
                ModeChangeClientRpc(false, i);
            }
            else
            {
                yield return new WaitForSeconds(battleTime);
                ModeChangeClientRpc(true, i);              
            }
        }
        DrawClientRpc();
    }
}
