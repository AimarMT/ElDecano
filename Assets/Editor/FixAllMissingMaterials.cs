using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixAllMissingMaterials : MonoBehaviour
{
    // Cambia esta ruta a donde tengas tu material por defecto
    private static string defaultMaterialPath = "Assets/Materials/DefaultMaterial.mat";

    [MenuItem("Tools/Fix All Missing Materials")]
    public static void FixMaterials()
    {
        Material defaultMat = AssetDatabase.LoadAssetAtPath<Material>(defaultMaterialPath);
        if (defaultMat == null)
        {
            Debug.LogError("No se encontró DefaultMaterial en la ruta: " + defaultMaterialPath);
            return;
        }

        int fixedCount = 0;

        // ---- Repara materiales en la escena ----
        MeshRenderer[] sceneRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer r in sceneRenderers)
        {
            fixedCount += ReplaceMissingMaterials(r, defaultMat);
        }

        // ---- Repara materiales en prefabs de Assets ----
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer r in renderers)
            {
                fixedCount += ReplaceMissingMaterials(r, defaultMat);
            }

            // Guardar cambios en prefab
            PrefabUtility.SavePrefabAsset(prefab);
        }

        Debug.Log($" Reasignación completada. Total materiales reemplazados: {fixedCount}");
    }

    private static int ReplaceMissingMaterials(MeshRenderer renderer, Material defaultMat)
    {
        int replaced = 0;
        Material[] mats = renderer.sharedMaterials;
        bool changed = false;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null || mats[i].shader == null)
            {
                mats[i] = defaultMat;
                replaced++;
                changed = true;
                Debug.Log($"Material reemplazado en: {renderer.gameObject.name}", renderer.gameObject);
            }
        }

        if (changed)
        {
            renderer.sharedMaterials = mats;
        }

        return replaced;
    }
}

