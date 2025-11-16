using UnityEngine;
using System;

public class ItemStepper : MonoBehaviour, IItem
{
    public EItemType Type => EItemType.Stepper;

    private Action<IItem> m_releaseAction;

    [SerializeField] private IItemEventChannelSO onTouchItem;


    public void Initialize(Vector3 position, Quaternion rotation)
    {
        float randomAngle = UnityEngine.Random.Range(0.0f, 180.0f);

        Quaternion randomRotation = Quaternion.Euler(0.0f, randomAngle, 0.0f);
        transform.SetPositionAndRotation(position, randomRotation);
        gameObject.SetActive(true);
    }

    public void SetReleaseAction(Action<IItem> releaseAction)
    {
        m_releaseAction = releaseAction;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public string GetNameWhenMousePointed()
    {
        return null;
    }

    public void Touch()
    {
        onTouchItem.Raise(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != (int)ELayerName.Player) return;
        Touch();
    }
}
