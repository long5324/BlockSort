using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GameManager : Singleton<GameManager>
{
    public Block BlockData;
    GamePlayManager gamePlayManger;
    InitGrid Gird;
    HomeUIControl HomeUIControl;
    private void Start()
    {
        gamePlayManger = GamePlayManager.Instance;
        Gird = InitGrid.Instance;
        StartCoroutine(DelayActiveBlockList());
    }
    public void StartGame()
    {
        Gird.gameObject.SetActive(true);
        Gird.transform.DOLocalMoveY(-5, 1);
        StartCoroutine(WaitSpawn());
       
    }
    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        gamePlayManger.RandomSpawnBlockChild();
        gamePlayManger.setColliderSize();
    }
    IEnumerator DelayActiveBlockList()
    {
        yield return new WaitForSeconds(1);
        Gird.gameObject.SetActive(false);
    }
}
