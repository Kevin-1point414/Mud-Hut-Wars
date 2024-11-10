using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Offline
{
    public class MudHutManager : MonoBehaviour
    {
        public float mudHut1Health;
        public float mudHut2Health;
        public static GameObject winDisplay;
        public static GameObject loseDisplay;

        public void HomeScreenButtonClicked()
        {
            Destroy(GameObject.Find("RoundManager(Clone)"));
            SceneManager.LoadSceneAsync("HomeScreen");
        }
    }
}