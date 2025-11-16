using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntEventChannelSO", menuName = "Events/Int Event Channel")]
public class IntEventChannelSO : ScriptableObject
{
    public event Action<int> OnEvent;

    public void Raise(int amount)
    {
        OnEvent?.Invoke(amount);
    }
}