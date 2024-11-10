using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Offline
{
    public class ExplosiveManager : MonoBehaviour
    {
        private static float radius;
        private const float destructionRadius = 2;
        public bool exploding;
        private const float bondsDestructionRadius = 5;
        private const float falloff = 10;
        private const float power = 100;
        public static GameObject explosiveUI;
        private List<string> strings;
        private ContactFilter2D contactFilter;
        private Vector3 input;
        [SerializeField] private Vector3 offset;

        private void Start()
        {
            exploding = false;
            strings = new List<string>();
            strings.Add("");
            contactFilter = new ContactFilter2D();
            radius = power / falloff;
        }

        private void Update()
        {
            if (!explosiveUI.activeSelf) return;

            UIManager.ClosePropertiesUI(typeof(ExplosiveManager), explosiveUI);

            if (gameObject == UIManager.UIopen)
            {
                strings[0] = explosiveUI.transform.Find("InputActive").GetComponent<TMP_InputField>().text;
            }
        }

        private void FixedUpdate()
        {
            if (!SetupManager.setupActive)
            {
                (float value, int index) number = Parser.MainParse(strings[0], gameObject);
                if (number.index != -1 && number.value >= 100 && !exploding) Explode();
            }
        }

        private void OnMouseOver()
        {
            UIManager.OpenPropertiesUI(explosiveUI, gameObject, offset, strings);
        }

        public static void SensorCreated()
        {
            string text = "";
            foreach (GameObject sensor in SensorManager.sensorList)
            {
                text = text + " S" + sensor.GetComponent<SensorManager>().ID;
            }
            explosiveUI.transform.Find("SensorListDisplay").GetComponent<TextMeshProUGUI>().text = text;
        }

        public void Explode()
        {
            exploding = true;
            List<Collider2D> effectedObjects = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, radius, contactFilter.NoFilter(), effectedObjects);
            foreach (Collider2D collider in effectedObjects)
            {
                if (collider != null)
                {
                    if (collider.gameObject.GetComponent<Rigidbody2D>() != null)
                    {
                        float distancesqr = (transform.position - collider.transform.position).sqrMagnitude;
                        Rigidbody2D rigidbody = collider.attachedRigidbody;

                        if (distancesqr < bondsDestructionRadius * bondsDestructionRadius)
                        {
                            FixedJoint2D[] joints1 = collider.gameObject.GetComponents<FixedJoint2D>();
                            for (int i = 0; i < joints1.Length; i++)
                            {
                                try
                                {
                                    FixedJoint2D[] joints2 = joints1[i].connectedBody.GetComponents<FixedJoint2D>();
                                    for (int j = 0; j < joints2.Length; j++)
                                    {
                                        if (joints2[j] == joints1[i])
                                        {
                                            ExplosiveDestroy(joints2[j].gameObject, j);
                                        }
                                    }
                                    ExplosiveDestroy(collider.gameObject, i);
                                }
                                catch { }
                            }
                        }
                        if (distancesqr < destructionRadius * destructionRadius)
                        {
                            if (UIManager.UIopen == collider.gameObject)
                            {
                                UIManager.UIopen = null;
                                PowerSourceManager.powerSourceUI.SetActive(false);
                                SensorManager.sensorUI.SetActive(false);
                                explosiveUI.SetActive(false);
                            }
                            if (collider.gameObject.GetComponent<SensorManager>() != null)
                            {
                                ExplosiveDestroy(collider.gameObject, -1, true);
                                SensorManager.sensorList.Remove(collider.gameObject);
                                PowerSourceManager.SensorCreated();
                                SensorCreated();
                                SensorManager.SensorCreated();
                            }
                            else if (collider.gameObject.GetComponent<ExplosiveManager>() != null)
                            {
                                if (!collider.gameObject.GetComponent<ExplosiveManager>().exploding)
                                {
                                    collider.gameObject.GetComponent<ExplosiveManager>().Explode();
                                }
                            }
                            else
                            {
                                ExplosiveDestroy(collider.gameObject);
                            }
                        }
                    }
                    else
                    {
                        if (collider.gameObject.name == "MudHut1") GameObject.Find("RoundManager(Clone)").
                                GetComponent<MudHutManager>().mudHut1Health -= 10;

                        if (collider.gameObject.name == "MudHut2") GameObject.Find("RoundManager(Clone)").
                                GetComponent<MudHutManager>().mudHut2Health -= 10;
                    }
                }
            }
            ExplosiveDestroy(gameObject);
        }

        void ExplosiveDestroy(GameObject gameObjectToDestroy, int index = -1, bool sensor = false)
        {
            if (index == -1)
            {
                CreationManager.objectHashSet.Remove(gameObjectToDestroy);
                if (sensor)
                {
                    Destroy(gameObjectToDestroy.GetComponent<SensorManager>().sensorIDDisplay);
                }
            }
            if (index == -1)
            {
                Destroy(gameObjectToDestroy);
            }
            else
            {
                try
                {
                    Destroy(gameObjectToDestroy.GetComponents<FixedJoint2D>()[index]);
                }
                catch { }
            }
        }
    }
}