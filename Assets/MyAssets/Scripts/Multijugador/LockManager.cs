using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.Netcode;

public class LockManager : NetworkBehaviour
{
    [System.Serializable]
    public class LockEntry
    {
        public XRSocketInteractor socket;
        public string requiredTag;
        [HideInInspector] public bool isPlaced;
    }

    [Header("Locks y Tags")]
    public LockEntry[] locks;

    [Header("Puertas")]
    public Animator[] doorAnimators;
    [SerializeField] private string doorAnimationName = "Opening 1";

    private NetworkVariable<int> keysPlacedCount = new NetworkVariable<int>(0);
    private bool doorOpened;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        for (int i = 0; i < locks.Length; i++)
        {
            int index = i;
            locks[i].socket.selectEntered.AddListener((args) => OnKeyPlaced(index, args));
            locks[i].socket.selectExited.AddListener((args) => OnKeyRemoved(index, args));
        }

        keysPlacedCount.OnValueChanged += OnKeysCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        keysPlacedCount.OnValueChanged -= OnKeysCountChanged;
        base.OnNetworkDespawn();
    }

    private void OnKeyPlaced(int lockIndex, SelectEnterEventArgs args)
    {
        GameObject key = args.interactableObject.transform.gameObject;
        LockEntry entry = locks[lockIndex];

        NetworkObject keyNet = key.GetComponent<NetworkObject>();
        if (keyNet != null && !keyNet.IsOwner) return;

        if (!key.CompareTag(entry.requiredTag))
        {
            StartCoroutine(ForceRelease(entry.socket));
            return;
        }

        entry.isPlaced = true;
        NotifyKeyPlacedRpc(lockIndex);
    }

    private void OnKeyRemoved(int lockIndex, SelectExitEventArgs args)
    {
        LockEntry entry = locks[lockIndex];
        if (!entry.isPlaced) return;

        GameObject key = args.interactableObject.transform.gameObject;
        NetworkObject keyNet = key.GetComponent<NetworkObject>();
        if (keyNet != null && !keyNet.IsOwner) return;

        entry.isPlaced = false;
        NotifyKeyRemovedRpc(lockIndex);
    }

    private void OnKeysCountChanged(int oldValue, int newValue)
    {
        if (newValue >= locks.Length && !doorOpened)
        {
            doorOpened = true;
            foreach (Animator door in doorAnimators)
            {
                door.Play(doorAnimationName);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void NotifyKeyPlacedRpc(int lockIndex)
    {
        if (lockIndex < 0 || lockIndex >= locks.Length) return;
        if (locks[lockIndex].isPlaced) return;
        locks[lockIndex].isPlaced = true;
        keysPlacedCount.Value++;
    }

    [Rpc(SendTo.Server)]
    private void NotifyKeyRemovedRpc(int lockIndex)
    {
        if (lockIndex < 0 || lockIndex >= locks.Length) return;
        if (!locks[lockIndex].isPlaced) return;
        locks[lockIndex].isPlaced = false;
        keysPlacedCount.Value = Mathf.Max(0, keysPlacedCount.Value - 1);
    }

    private IEnumerator ForceRelease(XRSocketInteractor socket)
    {
        yield return null;
        socket.enabled = false;
        yield return null;
        socket.enabled = true;
    }
}