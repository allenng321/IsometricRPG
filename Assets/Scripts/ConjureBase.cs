using UnityEngine;

public abstract class ConjureBase : ScriptableObject
{
    public abstract void DoBehaviour();
    public abstract void Timeout(PooledMonoBehaviour instance);
}
