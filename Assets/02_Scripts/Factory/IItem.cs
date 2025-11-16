using UnityEngine;
using System;

public interface IItem
{
    public EItemType Type { get; }

    void Initialize(Vector3 position, Quaternion rotation);

    void SetReleaseAction(Action<IItem> releaseAction);

    string GetNameWhenMousePointed();
    void Touch();
    GameObject GetGameObject();
}