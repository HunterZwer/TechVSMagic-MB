using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instance;
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ObjectPool>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ObjectPool");
                    _instance = obj.AddComponent<ObjectPool>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!poolDict.ContainsKey(tag) || poolDict[tag].Count == 0)
        {
            return null;
        }

        return poolDict[tag].Dequeue();
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        string tag = obj.tag;
        if (!poolDict.ContainsKey(tag))
        {
            poolDict[tag] = new Queue<GameObject>();
        }

        obj.SetActive(false);
        poolDict[tag].Enqueue(obj);
    }
}