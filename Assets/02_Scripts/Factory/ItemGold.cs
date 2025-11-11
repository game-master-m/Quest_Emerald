using UnityEngine;
using System;

public class ItemGold : MonoBehaviour, IItem
{
    public EItemType Type => EItemType.Gold;

    private Action<IItem> m_releaseAction;

    [SerializeField] private IItemEventChannelSO onTouchGold;


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
    }
}
