using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SojaExiles
{
    public class opencloseDoor1 : MonoBehaviour
    {
        public Animator openandclose1;
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

        public IEnumerator opening()
        {
            print("you are opening the door");
            openandclose1.Play("Opening 1");

            if (audioSource && openSound)
                audioSource.PlayOneShot(openSound);

            open = true;
            yield return new WaitForSeconds(.5f);
        }

        public IEnumerator closing()
        {
            print("you are closing the door");
            openandclose1.Play("Closing 1");

            if (audioSource && closeSound)
                audioSource.PlayOneShot(closeSound);

            open = false;
            yield return new WaitForSeconds(.5f);
        }
    }
}

