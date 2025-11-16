using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IItemEventChannelSO", menuName = "Events/IItem Event Channel")]
public class IItemEventChannelSO : ScriptableObject
{
    public event Action<IItem> OnEvent;

    public void Raise(IItem item)
    {
        OnEvent?.Invoke(item);
    }
}