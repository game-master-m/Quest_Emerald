using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FloatEventChannelSO", menuName = "Events/Float Event Channel")]
public class FloatEventChannelSO : ScriptableObject
{
    public event Action<float> OnEvent;

    public void Raise(float value)
    {
        OnEvent?.Invoke(value);
    }
}