using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

namespace SojaExiles
{
    public class OpenCloseDoorMultiplayer : NetworkBehaviour
    {
        [Header("Animation")]
        public Animator animator;
        public string openAnim = "Opening 1";
        public string closeAnim = "Closing 1";

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip openSound;
        public AudioClip closeSound;

        [Header("Settings")]
        public float animationDuration = 0.5f;

        // Estado sincronizado
        private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);

        private bool isMoving = false;

        public bool IsOpen => isOpen.Value;

        // =========================
        // SINCRONIZACIÓN INICIAL
        // =========================
        public override void OnNetworkSpawn()
        {
            ApplyVisualState(isOpen.Value);
        }

        void ApplyVisualState(bool open)
        {
            if (animator)
            {
                // Salta directamente al estado final (sin animar)
                animator.Play(open ? openAnim : closeAnim, 0, 1f);
            }
        }

        // =========================
        //  XR INTERACTION — GRAB
        // =========================

        // Llamado cuando el jugador agarra la puerta (botón Grab)
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            OpenDoor();
        }

        // Llamado cuando el jugador suelta la puerta
        public void OnSelectExited(SelectExitEventArgs args)
        {
            CloseDoor();
        }

        // Compatibilidad con Activate por si se usa desde otro sitio
        public void OnActivate(ActivateEventArgs args) { }

        // =========================
        //  TOGGLE GENERAL
        // =========================
        public void ToggleDoor()
        {
            if (IsServer)
                ApplyToggle();
            else
                ToggleDoorRpc();
        }

        [Rpc(SendTo.Server)]
        void ToggleDoorRpc(RpcParams rpcParams = default)
        {
            ApplyToggle();
        }

        void ApplyToggle()
        {
            isOpen.Value = !isOpen.Value;
            PlayDoorClientRpc(isOpen.Value);
        }

        // =========================
        // CONTROL EXTERNO (NPCs, etc.)
        // =========================
        public void OpenDoor()
        {
            if (IsServer)
                ApplyOpen();
            else
                OpenDoorRpc();
        }

        [Rpc(SendTo.Server)]
        void OpenDoorRpc(RpcParams rpcParams = default)
        {
            ApplyOpen();
        }

        void ApplyOpen()
        {
            if (isOpen.Value) return;

            isOpen.Value = true;
            PlayDoorClientRpc(true);
        }

        public void CloseDoor()
        {
            if (IsServer)
                ApplyClose();
            else
                CloseDoorRpc();
        }

        [Rpc(SendTo.Server)]
        void CloseDoorRpc(RpcParams rpcParams = default)
        {
            ApplyClose();
        }

        void ApplyClose()
        {
            if (!isOpen.Value) return;

            isOpen.Value = false;
            PlayDoorClientRpc(false);
        }

        // =========================
        // SINCRONIZACIÓN VISUAL
        // =========================
        [Rpc(SendTo.Everyone)]
        void PlayDoorClientRpc(bool openState)
        {
            if (openState)
                StartCoroutine(OpenRoutine());
            else
                StartCoroutine(CloseRoutine());
        }

        IEnumerator OpenRoutine()
        {
            if (isMoving) yield break;
            isMoving = true;

            Debug.Log("Opening door (Multiplayer)");

            if (animator)
                animator.Play(openAnim);

            if (audioSource && openSound)
                audioSource.PlayOneShot(openSound);

            yield return new WaitForSeconds(animationDuration);
            isMoving = false;
        }

        IEnumerator CloseRoutine()
        {
            if (isMoving) yield break;
            isMoving = true;

            Debug.Log("Closing door (Multiplayer)");

            if (animator)
                animator.Play(closeAnim);

            if (audioSource && closeSound)
                audioSource.PlayOneShot(closeSound);

            yield return new WaitForSeconds(animationDuration);
            isMoving = false;
        }
    }
}