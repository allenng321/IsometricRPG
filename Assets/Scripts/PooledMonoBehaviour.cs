using System;
using System.Collections;
using UnityEngine;

public class PooledMonoBehaviour : MonoBehaviour
{
    [SerializeField] private int initialPoolSize = 50;
    
    public event Action<PooledMonoBehaviour> OnReturnToPool;
    public int InitialPoolSize
    {
        get { return initialPoolSize; }
    }

    public void SetPoolParent(Transform parent)
    {
        var _pool = Pool.GetPool(this);
        _pool.transform.parent = parent;
    }

    public T Get<T>() where T : PooledMonoBehaviour
    {
        var _pool = Pool.GetPool(this);
        var pooledObject = _pool.Get<T>();
        pooledObject.gameObject.SetActive(true);

        return pooledObject;
    }

    public T Get<T>(Vector3 position, Quaternion rotation) where T : PooledMonoBehaviour
    {
        var pooledObject = Get<T>();
        pooledObject.transform.position = position;
        pooledObject.transform.rotation = rotation;

        return pooledObject;
    }

    protected virtual void OnDisable()
    {
        if (OnReturnToPool != null)
        {
            OnReturnToPool(this);
        }
    }
    
    protected void ReturnToPool(float delay = 0)
    {
        StartCoroutine(ReturnToPoolAfterSeconds(delay));
    }

    private IEnumerator ReturnToPoolAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}