using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSpawnController : MonoBehaviour
{
    public SceneSpawnData spawnData;

    public void SpawnInScene(string sceneName, GameObject prefab, Vector3 position)
    {
        spawnData.AddRequest(sceneName, prefab, position);

        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (!targetScene.isLoaded)
        {
            StartCoroutine(LoadSceneAndSpawn(sceneName));
        }
        else
        {
            SpawnNow(sceneName);
        }
    }

    private IEnumerator LoadSceneAndSpawn(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SpawnNow(sceneName);
    }

    private void SpawnNow(string sceneName)
    {
        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        foreach (var req in spawnData.GetRequestsForScene(sceneName))
        {
            GameObject obj = Instantiate(req.prefab, req.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(obj, targetScene);
        }
        spawnData.RemoveRequestsForScene(sceneName);
    }
}
