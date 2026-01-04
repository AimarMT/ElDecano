using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SegundaCinematica : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    [SerializeField] string loadingSceneName = "LoadingScene";
    [SerializeField] LoadingSceneData loadingData;


    public Image appearDark;
    public float fadeDuration = 2f;

    private bool triggered = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            StartCoroutine(LoadNextGame());
        }
    }


    IEnumerator LoadNextGame()
    {
        appearDark.gameObject.SetActive(true);

        float t = 0f;
        Color color = appearDark.color;
        color.a = 0f;
        appearDark.color = color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = t / fadeDuration;
            appearDark.color = color;
            yield return null;
        }

        loadingData.sceneToLoad = sceneToLoad;
        SceneManager.LoadScene(loadingSceneName);
    }
}
