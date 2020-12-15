using UnityEngine;

[CreateAssetMenu (fileName = "ConjureData", menuName = "Custom/Create new ConjureData object")]
public class ConjureData : ConjureBase
{
    public float velocity = 0;
    public float dissipateDelay = 1.2f;
    public PooledMonoBehaviour conjureModel;

    public override void DoBehaviour()
    {
        // TODO
    }
    
    public override void Timeout(PooledMonoBehaviour instance)
    {
        instance.gameObject.SetActive(false);
    }
}