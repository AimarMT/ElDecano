using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SojaExiles
{
    public class lockedDoorScript: MonoBehaviour
    {
        public AudioSource audioSource;

        public AudioClip firstClip;
        public AudioClip secondClip;

        public float delayBetweenClips = 0.15f;

        //  Bloqueo para evitar que se repita
        private bool isPlaying = false;

        void Start()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void OnActivate(ActivateEventArgs args)
        {
            // Si ya está sonando algo, NO hacer nada
            if (isPlaying)
                return;

            StartCoroutine(PlayLockedSequence());
        }

        IEnumerator PlayLockedSequence()
        {
            isPlaying = true;

            if (audioSource == null || firstClip == null)
            {
                isPlaying = false;
                yield break;
            }

            // Primer sonido
            audioSource.PlayOneShot(firstClip);

            // Espera antes del segundo
            yield return new WaitForSeconds(delayBetweenClips);

            // Segundo sonido
            if (secondClip != null)
            {
                audioSource.PlayOneShot(secondClip);

                // Esperar a que termine el segundo clip
                yield return new WaitForSeconds(secondClip.length);
            }

            //  Desbloquear
            isPlaying = false;
        }
    }
}



