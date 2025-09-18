using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct InfoGameLevel
{
    public int NumberLever;
    public GameObject GameObjectLevel;
    public int ScoreMax;
    public LevelReward LevelRewards;
}
[CreateAssetMenu(fileName = "LevelData", menuName = "Data/LevelData")]

public class GameLevelData : ScriptableObject
{
   public List<InfoGameLevel> ListGameLevel = new List<InfoGameLevel>();
}
