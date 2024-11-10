using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MudHutManager : NetworkBehaviour
{
    public NetworkVariable<float> mudHut1Health = new NetworkVariable<float>();
    public NetworkVariable<float> mudHut2Health = new NetworkVariable<float>();
    public static GameObject winDisplay;
    public static GameObject loseDisplay;

    void Start()
    {
        mudHut2Health.OnValueChanged += mudHut2HealthChanged;
        mudHut1Health.OnValueChanged += mudHut1HealthChanged;
    }

    void mudHut1HealthChanged(float previous, float current)
    {
        if (mudHut1Health.Value <= -100)
        {
            if (IsHost)
            {
                winDisplay.SetActive(true);
            }
            else
            {
                loseDisplay.SetActive(true);
            }
        }
    }

    void mudHut2HealthChanged(float previous, float current)
    {
        if (mudHut2Health.Value <= -100)
        {
            if (IsHost)
            {
                loseDisplay.SetActive(true);
            }
            else
            {
                winDisplay.SetActive(true);
            }
        }
    }

    public void HomeScreenButtonClicked()
    {
        Destroy(GameObject.Find("RoundManager(Clone)"));
        Destroy(NetworkManager.Singleton.gameObject);       
        SceneManager.LoadSceneAsync("HomeScreen");
    }
}
