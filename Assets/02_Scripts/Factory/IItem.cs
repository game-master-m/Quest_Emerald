using UnityEngine;
using System;

public interface IItem
{
    public EItemType Type { get; }

    void Initialize(Vector3 position, Quaternion rotation);

    void SetReleaseAction(Action<IItem> releaseAction);

    void MousePointed();
    void Touch();
    GameObject GetGameObject();
}