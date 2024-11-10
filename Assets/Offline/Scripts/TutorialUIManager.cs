using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Offline
{
    public class TutorialUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        public void TutorialButtonClicked()
        {
            Destroy(NetworkManager.Singleton.gameObject);
            DontDestroyOnLoad(Instantiate(playerPrefab));
            SceneManager.LoadSceneAsync("GameScreen");
        }
    }
}
