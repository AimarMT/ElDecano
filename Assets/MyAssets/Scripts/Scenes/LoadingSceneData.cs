using UnityEngine;

[CreateAssetMenu(
    fileName = "LoadingSceneData",
    menuName = "Loading/Scene Data"
)]
public class LoadingSceneData : ScriptableObject
{
    public string sceneToLoad;
}
