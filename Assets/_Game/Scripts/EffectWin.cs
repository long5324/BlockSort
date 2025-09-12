using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ListParticleEffect
{
    public List<ParticleSystem> Particles;
    public bool Time;
    [ShowIf("Time")]
    public float ValueTime;
    
    public bool EndLastParticle;
}
public class EffectWin : MonoBehaviour
{
    public List<ListParticleEffect> ListEffect;
    [Button(ButtonSizes.Large)]
    public void StartEffect()
    {
        StartCoroutine(PlayEffects());
    }

    private IEnumerator PlayEffects()
    {
        foreach (var effect in ListEffect)
        {
            if (effect.EndLastParticle)
            {
                float duration = 0;
                foreach (var ps in effect.Particles)
                {
                    if (ps != null)
                    {
                        ps.gameObject.SetActive(true);
                        ps.Play(true);
                       if(duration < ps.main.startLifetime.constantMax)
                        {
                            duration = ps.main.startLifetime.constantMax;
                        }
                    }
                }
                yield return new WaitForSeconds(duration);
            }
            else
            {
             
                foreach (var ps in effect.Particles)
                {
                    if (ps != null)
                    {
                        ps.gameObject.SetActive(true);
                        ps.Play(true);
                    }
                }

                // Chờ theo time nếu cần
                if (effect.Time)
                {
                    Debug.Log($"⏳ Chờ {effect.ValueTime} giây (theo Time)");
                    yield return new WaitForSeconds(effect.ValueTime);
                }
            }
        }
        Debug.Log("✅ Hoàn tất tất cả effect!");
    }

}
