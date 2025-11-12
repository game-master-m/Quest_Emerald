using UnityEngine;
using System;

public class ItemBlueBall : MonoBehaviour, IItem
{
    public EItemType Type => EItemType.Blue;

    private Action<IItem> m_releaseAction;

    [SerializeField] private IItemEventChannelSO onTouchItem;


    public void Initialize(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
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

    public void MousePointed()
    {
        //Á¤º¸ Ãâ·Â
    }

    public void Touch()
    {
        //È¹µæ
        m_releaseAction?.Invoke(this);
        onTouchItem.Raise(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != (int)ELayerName.Player) return;
        Touch();
    }
}
