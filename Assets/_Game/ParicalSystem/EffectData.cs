using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct InfoEffect
{
    public BlockColor color;
    public ParticleSystem particleSystem;
}
[CreateAssetMenu(fileName = "ParticleObject", menuName = "Effect/ParticleEffect")]
public class EffectData : ScriptableObject 
{
   public List<InfoEffect> DataBaseEffect;
   public ParticleSystem StartEffect(BlockColor color , Vector3 position )
    {
      foreach (var i in DataBaseEffect)
        {
            if(color == i.color)
            {
                Debug.Log("haveColor");
               return Instantiate(i.particleSystem, position, Quaternion.Euler(55, 60, 7));
            }
        }
      return null;
    }
}
