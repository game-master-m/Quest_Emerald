using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Data/GameData")]
public class GameData : ScriptableObject
{
    [SerializeField] private float bestTime = 300.0f;
    [SerializeField] private int gold;

    public float BestTime => bestTime;
    public int Gold => gold;

    public void AddGold(int amount)
    {
        gold += amount;
    }
    public void SetGold(int totalAmount)
    {
        gold = totalAmount;
    }
    public void SetBestTime(float newBesttime)
    {
        bestTime = newBesttime;
    }
}
