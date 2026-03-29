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
        if (spawnPoints.Length < keyPrefabs.Length)
        {
            Debug.LogError("No hay suficientes puntos de spawn.");
            return;
        }

        alreadySpawned = true;

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

        if (audioSource && spawnSound)
            audioSource.PlayOneShot(spawnSound);

        Debug.Log("Llaves spawnadas correctamente (multiplayer).");
    }
}