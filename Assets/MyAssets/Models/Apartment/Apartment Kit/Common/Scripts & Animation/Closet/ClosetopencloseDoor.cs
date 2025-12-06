using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SojaExiles
{
    public class ClosetopencloseDoor : MonoBehaviour
    {
        public Animator Closetopenandclose;
        public bool open;

        void Start()
        {
            open = false;
        }

        // Esto se llamará cuando pulses el gatillo derecho en el XR Simple Interactable
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
            print("you are opening the closet");
            Closetopenandclose.Play("ClosetOpening");
            open = true;
            yield return new WaitForSeconds(.5f);
        }

        IEnumerator closing()
        {
            print("you are closing the closet");
            Closetopenandclose.Play("ClosetClosing");
            open = false;
            yield return new WaitForSeconds(.5f);
        }
    }
}
