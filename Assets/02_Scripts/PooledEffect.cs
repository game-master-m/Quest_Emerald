using System.Collections;
using UnityEngine;

public class PooledEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    private Coroutine returnCo;
    private WaitForSeconds returnTime;

    private void Awake()
    {
        returnTime = new WaitForSeconds(1.0f);
        returnCo = null;
    }
    public void PlayEffect(Transform trans)
    {
        transform.SetPositionAndRotation(trans.position, trans.rotation);
        ps.Stop();
        ps.Play();

        float returnDelay = ps.main.duration;

        if (returnCo != null) StopCoroutine(returnCo);
        returnCo = StartCoroutine(ReturnCo());
    }

    private IEnumerator ReturnCo()
    {
        yield return returnTime;
        ps.Stop();
        Managers.Pool.ReturnToPool(this);
    }
}
