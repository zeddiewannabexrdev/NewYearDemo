using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zef.Pool
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;

        [Header("Pools (children or assigned manually)")]
        public List<Pool> pools = new List<Pool>();

        private Dictionary<string, Pool> poolMap;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            poolMap = new Dictionary<string, Pool>(pools.Count);

            foreach (var pool in pools)
            {
                if (pool == null) continue;

                pool.Init();
                string key = pool.gameObject.name;
                poolMap.Add(key, pool);
                if (poolMap.ContainsKey(key))
                {
                    Debug.Log($"[PoolManager] Duplicate pool name: {key}");
                    continue;
                }

                // pool.Init();
                // poolMap.Add(key, pool);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            pools.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out Pool p))
                {
                    pools.Add(p);
                }
            }
        }
#endif

        /// <summary>
        /// Get object from pool by name
        /// </summary>
        public GameObject Get(string poolName, bool autoActive = true)
        {
            if (!poolMap.TryGetValue(poolName, out Pool pool))
            {
                Debug.LogError($"[PoolManager] Pool not found: {poolName}");
                return null;
            }

            GameObject obj = pool.Get();

            if (obj == null)
                return null;

            if (autoActive)
                obj.SetActive(true);

            return obj;
        }

        public GameObject GetAndAutoDeny(string poolName, float time, bool autoActive = true)
        {
            GameObject result = Get(poolName, autoActive);
            if (result != null) 
                StartCoroutine(Deny(result, time));
            return result;
        }
        IEnumerator Deny(GameObject obj, float time)
        {
            int id = obj.GetInstanceID();

            yield return new WaitForSeconds(time);

            if (obj != null && obj.activeInHierarchy && obj.GetInstanceID() == id)
            {
                obj.SetActive(false);
            }
        }

    }
}
