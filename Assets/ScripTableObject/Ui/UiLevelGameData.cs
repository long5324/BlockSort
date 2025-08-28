using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct InfoLever
{
    public int NumberLevel ;
    public Sprite AvatarLevel;
}
[CreateAssetMenu(fileName = "UIGameHove", menuName = "UI/DataGameLever")]
public class UiLevelGameData : ScriptableObject
{
    List<InfoLever> LevelDatabase = new List<InfoLever>();
}
