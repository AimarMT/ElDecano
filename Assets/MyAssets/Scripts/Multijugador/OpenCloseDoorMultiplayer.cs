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

        // 👇 Getter público
        public bool IsOpen => isOpen.Value;

        // =========================
        // 🎮 VR INTERACTION
        // =========================
        public void OnActivate(ActivateEventArgs args)
        {
            ToggleDoor();
        }

        // =========================
        // 🔄 TOGGLE GENERAL
        // =========================
        public void ToggleDoor()
        {
            if (IsServer)
                ApplyToggle();
            else
                ToggleDoorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void ToggleDoorServerRpc()
        {
            ApplyToggle();
        }

        void ApplyToggle()
        {
            isOpen.Value = !isOpen.Value;
            PlayDoorClientRpc(isOpen.Value);
        }

        // =========================
        // 🤖 NPC / EXTERNAL CONTROL
        // =========================
        public void OpenDoor()
        {
            if (IsServer)
                ApplyOpen();
            else
                OpenDoorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void OpenDoorServerRpc()
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
                CloseDoorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void CloseDoorServerRpc()
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
        //  SYNC VISUALS
        // =========================
        [ClientRpc]
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
