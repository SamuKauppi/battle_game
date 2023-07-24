using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    private readonly Dictionary<string, Queue<GameObject>> objectPool = new();
    [SerializeField] private PooledObject[] objectsToBePooled;
    GameObject objectToCheck;
    private void Awake()
    {
        Instance = this;
        foreach (PooledObject pooledObj in objectsToBePooled)
        {
            Queue<GameObject> tempQueue = new();
            if (pooledObj.parent == null)
            {
                pooledObj.parent = transform;
            }
            for (int j = 0; j < pooledObj.amount; j++)
            {
                GameObject obj = Instantiate(pooledObj.obj, pooledObj.parent);
                obj.SetActive(false);
                tempQueue.Enqueue(obj);
            }
            objectPool.Add(pooledObj.ident, tempQueue);
        }
    }

    public GameObject GetPooledObject(string ident, Vector3 pos = new Vector3())
    {
        if (!objectPool.ContainsKey(ident)) return null;


        for (int i = 0; i < objectPool[ident].Count; i++)
        {
            if (!objectPool[ident].Peek().activeSelf)
            {
                objectToCheck = objectPool[ident].Dequeue();
                objectToCheck.SetActive(true);
                objectToCheck.transform.position = pos;
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        foreach (PooledObject pooledObj in objectsToBePooled)
        {
            if (pooledObj.ident == ident)
            {
                objectToCheck = Instantiate(pooledObj.obj, pooledObj.parent);
                objectToCheck.SetActive(true);
                objectToCheck.transform.position = pos;
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        return null;
    }
}
