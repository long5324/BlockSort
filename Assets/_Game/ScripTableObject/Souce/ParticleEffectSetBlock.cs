using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "Effect/BlockSet")]
public class ParticleEffectSetBlock : ScriptableObject
{
    [Serializable]
    public struct EffectData
    {
        public BlockColor Color;
        public ParticleSystem ParticleEffect;
    }
        public List<EffectData> EffectDataBase = new List<EffectData>();
}
