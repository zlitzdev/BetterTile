using System.Collections;

using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    internal sealed class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner s_instance;

        public static CoroutineRunner instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    s_instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj);
                }
                return s_instance;
            }
        }

        public static Coroutine Run(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null && s_instance != null)
            {
                s_instance.StopCoroutine(coroutine);
            }
        }
    }

}
