using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            [SerializeField] private GameObject prefab;
            [SerializeField] private int size;

            public GameObject Prefab => prefab;
            public int Size => size;
        }

        [SerializeField] private List<Pool> pools;

        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.Size; i++)
                {
                    GameObject obj = Instantiate(pool.Prefab, transform);
                    obj.name = pool.Prefab.name;
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.Prefab.name, objectPool);
            }
        }

        public GameObject SpawnObject(string prefabName)
        {
            if (!poolDictionary.ContainsKey(prefabName))
            {
                Debug.LogWarning("Pool with name " + prefabName + " doesn't exist!");
                return null;
            }

            Queue<GameObject> objectPool = poolDictionary[prefabName];

            if (objectPool.Count == 0)
            {
                // Debug.LogWarning("Object pool for " + prefabName + " is empty! Consider increasing initial pool size.");
                // return null;

                Debug.LogWarning("Instantiating " + prefabName + " because pool is empty! Consider increasing initial pool size.");

                GameObject newObj = Instantiate(pools.FirstOrDefault(x => x.Prefab.name == prefabName).Prefab, transform);
                newObj.name = prefabName;
                return newObj;
            }

            GameObject obj = objectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            string prefabName = obj.name;
            if (poolDictionary.ContainsKey(prefabName))
            {
                poolDictionary[prefabName].Enqueue(obj);
            }
            else
            {
                Debug.LogWarning("No pool found for object: " + prefabName);
            }
        }
    }
}
