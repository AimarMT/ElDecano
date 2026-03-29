using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KeySpawnerMultiplayer : MonoBehaviour
{
    [Header("Llaves (prefabs con NetworkObject)")]
    public GameObject[] keyPrefabs;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spawnSound;

    [Header("Test Mode")]
    public bool testMode = false;
    public Vector3 testStartPosition = new Vector3(-10f, 1f, 1f);

    private bool alreadySpawned = false;

    IEnumerator Start()
    {
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            yield return null;

        if (!NetworkManager.Singleton.IsServer) yield break;

        SpawnKeys();
    }

    private void SpawnKeys()
    {
        if (alreadySpawned) return;

        if (!testMode && spawnPoints.Length < keyPrefabs.Length)
        {
            Debug.LogError("No hay suficientes puntos de spawn.");
            return;
        }

        alreadySpawned = true;

        if (testMode)
        {
            for (int i = 0; i < keyPrefabs.Length; i++)
            {
                Vector3 pos = testStartPosition + new Vector3(i, 0f, 0f);
                GameObject keyInstance = Instantiate(keyPrefabs[i], pos, Quaternion.identity);
                NetworkObject netObj = keyInstance.GetComponent<NetworkObject>();
                if (netObj == null)
                {
                    Debug.LogError("El prefab de llave necesita un NetworkObject.");
                    Destroy(keyInstance);
                    continue;
                }
                netObj.Spawn(true);
            }
        }
        else
        {
            List<Transform> availablePoints = new List<Transform>(spawnPoints);

            foreach (GameObject keyPrefab in keyPrefabs)
            {
                int index = Random.Range(0, availablePoints.Count);
                Transform chosenPoint = availablePoints[index];

                GameObject keyInstance = Instantiate(keyPrefab, chosenPoint.position, chosenPoint.rotation);
                NetworkObject netObj = keyInstance.GetComponent<NetworkObject>();
                if (netObj == null)
                {
                    Debug.LogError("El prefab de llave necesita un NetworkObject.");
                    Destroy(keyInstance);
                    continue;
                }

                netObj.Spawn(true);

                Rigidbody rb = keyInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.ResetInertiaTensor();
                    rb.isKinematic = false;
                }

                availablePoints.RemoveAt(index);
            }
        }

        if (audioSource && spawnSound)
            audioSource.PlayOneShot(spawnSound);

        Debug.Log(testMode ? "Llaves spawnadas en modo TEST." : "Llaves spawnadas correctamente (multiplayer).");
    }
}