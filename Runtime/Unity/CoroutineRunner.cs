using UnityEngine;

namespace MychIO.Unity
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var coroutineRunnerGameObject = new GameObject("CoroutineRunnerMychIO");
                    _instance = coroutineRunnerGameObject.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(coroutineRunnerGameObject); // Ensure it persists across scenes
                }
                return _instance;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}