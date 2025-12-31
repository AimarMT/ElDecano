using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;       // Prefabs de NPC
    public int cantidadNPCs = 20;         // Cuántos NPCs quieres
    public float radioSpawn = 50f;        // Tamaño del área de spawn

    void Start()
    {
        for (int i = 0; i < cantidadNPCs; i++)
        {
            SpawnNPC();
        }
    }

    void SpawnNPC()
    {
        Vector3 pos = transform.position + new Vector3(
            Random.Range(-radioSpawn, radioSpawn),
            0,
            Random.Range(-radioSpawn, radioSpawn)
        );

        int index = Random.Range(0, npcPrefabs.Length);
        Instantiate(npcPrefabs[index], pos, Quaternion.identity);
    }
}
