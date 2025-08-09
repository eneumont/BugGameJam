using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneSpawnData", menuName = "Spawning/SceneSpawnData")]
public class SceneSpawnData : ScriptableObject
{
    [System.Serializable]
    public class SpawnRequest
    {
        public string sceneName;
        public GameObject prefab;
        public Vector3 position;
    }

    public List<SpawnRequest> spawnRequests = new List<SpawnRequest>();

    public void AddRequest(string sceneName, GameObject prefab, Vector3 position)
    {
        spawnRequests.Add(new SpawnRequest
        {
            sceneName = sceneName,
            prefab = prefab,
            position = position
        });
    }

    public List<SpawnRequest> GetRequestsForScene(string sceneName)
    {
        return spawnRequests.FindAll(r => r.sceneName == sceneName);
    }

    public void RemoveRequestsForScene(string sceneName)
    {
        spawnRequests.RemoveAll(r => r.sceneName == sceneName);
    }
}
