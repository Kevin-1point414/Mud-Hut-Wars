using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Offline
{
    public class CameraMovementManager : MonoBehaviour
    {
        [SerializeField] private Vector2 speed;
        [SerializeField] private float mapSize;
        private bool changedActiveSceneCalled;

        void Start()
        {
            changedActiveSceneCalled = false;

            transform.position = new Vector3(0, 0, -10);

            gameObject.GetComponent<Camera>().enabled = true;

            SceneManager.activeSceneChanged += ChangedActiveScene;
            if (SceneManager.GetActiveScene().name == "GameScreen")
            {
                ChangedActiveScene(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
            }
            Cursor.lockState = CursorLockMode.Confined;
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name != "GameScreen") return;

            if (PowerSourceManager.powerSourceUI.transform.Find("InputPower").GetComponent<TMP_InputField>()
                .isFocused || PowerSourceManager.powerSourceUI.transform.Find("InputDirection").GetComponent
                <TMP_InputField>().isFocused || ExplosiveManager.explosiveUI.transform.Find("InputActive")
                .GetComponent<TMP_InputField>().isFocused) return;

            transform.position += new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * speed.x,
                Input.GetAxis("Vertical") * Time.deltaTime * speed.y, 0);

            Vector3 movement = new Vector3(0, 0, 0);
            if (Input.mousePosition.x >= Screen.width - 10)
            {
                movement.x = speed.x * Time.deltaTime;
            }
            else if (Input.mousePosition.x <= 10)
            {
                movement.x = -speed.x * Time.deltaTime;
            }
            if (Input.mousePosition.y >= Screen.height - 10)
            {
                movement.y = speed.y * Time.deltaTime;
            }
            else if (Input.mousePosition.y <= 10)
            {
                movement.y = -speed.y * Time.deltaTime;
            }
            transform.position += movement;

            if (transform.position.y < -1)
            {
                transform.position = new Vector3(transform.position.x, -1, transform.position.z);
            }
            if (transform.position.x < 0)
            {
                transform.position = new Vector3(0, transform.position.y, transform.position.z);
            }
            if (transform.position.x > mapSize)
            {
                transform.position = new Vector3(mapSize, transform.position.y, transform.position.z);
            }
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
            if (next.name != "GameScreen" || changedActiveSceneCalled) return;

            changedActiveSceneCalled = true;

            Transform hut = GameObject.Find("MudHut2").transform;
            hut.position = new Vector3(mapSize, hut.position.y, hut.position.z);

            Transform ground = GameObject.Find("Ground").transform;
            ground.position = new Vector3(mapSize / 2, ground.position.y, ground.position.z);
            ground.localScale = new Vector3(mapSize + 50, ground.localScale.y, ground.localScale.z);

            GameObject.Find("Canvas").GetComponent<UIManager>().enabled = true;
        }
    }
}