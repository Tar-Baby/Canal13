using UnityEngine;
using System.Collections;

public class PersistentCoroutineRunner : MonoBehaviour
{
    private static PersistentCoroutineRunner _instance;
    public static PersistentCoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create the runner at runtime if it doesn’t exist
                var go = new GameObject("PersistentCoroutineRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<PersistentCoroutineRunner>();
            }
            return _instance;
        }
    }
}