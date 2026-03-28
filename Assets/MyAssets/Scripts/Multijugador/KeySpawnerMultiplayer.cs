using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KeySpawnerMultiplayer : NetworkBehaviour
{
    [Header("Llaves (prefabs con NetworkObject)")]
    public GameObject[] keyPrefabs;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spawnSound;

    private bool alreadySpawned = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Solo servidor spawnea

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

            netObj.Spawn(true); // true = con ownership del servidor inicialmente

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
            PlayAudioClientRpc();

        Debug.Log("Llaves spawnadas correctamente (multiplayer).");
    }

    [ClientRpc]
    private void PlayAudioClientRpc()
    {
        if (audioSource && spawnSound)
            audioSource.PlayOneShot(spawnSound);
    }
}