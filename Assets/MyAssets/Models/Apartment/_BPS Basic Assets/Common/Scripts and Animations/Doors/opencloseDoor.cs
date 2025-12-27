using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SojaExiles
{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;

        public AudioSource audioSource;
        public AudioClip openSound;
        public AudioClip closeSound;

        void Start()
        {
            open = false;
        }

        public void OnActivate(ActivateEventArgs args)
        {
            if (!open)
            {
                StartCoroutine(opening());
            }
            else
            {
                StartCoroutine(closing());
            }
        }

        IEnumerator opening()
        {
            print("you are opening the door");
            openandclose.Play("Opening");

            if (audioSource && openSound)
                audioSource.PlayOneShot(openSound);

            open = true;
            yield return new WaitForSeconds(.5f);
        }

        IEnumerator closing()
        {
            print("you are closing the door");
            openandclose.Play("Closing");

            if (audioSource && closeSound)
                audioSource.PlayOneShot(closeSound);

            open = false;
            yield return new WaitForSeconds(.5f);
        }
    }
}


