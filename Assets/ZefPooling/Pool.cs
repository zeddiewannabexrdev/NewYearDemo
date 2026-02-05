using System.Collections.Generic;
using UnityEngine;
namespace Zef.Pool
{
    /// <summary>
    /// Simple GameObject Pool (Editor-friendly)
    /// Key = Pool GameObject name
    /// </summary>
    public class Pool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] int maxElement;
        List<GameObject> pooling;
        public void Init()
        {
            if (prefab == null)
            { 
                prefab = transform.GetChild(0).gameObject;
                
            }
            prefab.SetActive(false);
            //gameObject.name = prefab.name;
            pooling = new List<GameObject>();
            for (int i = 0; i < maxElement; i++)
            {
                AddNew(); 
            }
        }
        public GameObject Get()
        {
            for (int i = 0; i < pooling.Count; i++)
            {
                if (!pooling[i].activeInHierarchy)
                    return pooling[i];
            }
            return AddNew();
        }
        GameObject AddNew()
        { 
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pooling.Add(obj);
            return obj;
        }
    }
}