
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
[System.Serializable]
public class BlockLevelDefaut
{
    public BlockColor Color;
    public int NumberSpawm;                            
}
[System.Serializable]
public enum StateBlock
{
    none,
    Nomal,
    Lock,
    LockCount,
    Support
}
public class BlockControl : MonoBehaviour
{
    [Header("SupportSaveLevel")]
    public List<BlockLevelDefaut> DataSpawn = new List<BlockLevelDefaut>();
    public List<ChildBlock> ListChildBlock ;
    public Vector3 PosionBlock ;
    public Renderer Renderer { get; set; }
    [SerializeField] public List< BlockControl >BlockLink = new List<BlockControl> ();
    public StateBlock State = StateBlock.Nomal;
    [ShowIf("IsLocked")]
    public GameObject GameObjectMod;
    [ShowIf("State",StateBlock.LockCount)]
    public ParticleSystem Effect;
    private void Start()
    {
        Renderer = GetComponent<Renderer>();
    }
    public void DeleteLockCount()
    {
        if (GameObjectMod == null) return;
        Destroy(GameObjectMod.transform.GetChild(0).gameObject);
    }
    public int CheckCount()
    {
        return GameObjectMod.transform.childCount;
    }
    public void SetColor(Material material)
    {
        Renderer.sharedMaterial = material;
    }
    private bool IsLocked()
    {
        return State == StateBlock.Lock || State == StateBlock.LockCount;
    }
    public void BacktoDFColor()
    {
        if (Renderer == null)
        {
            GetComponent<Renderer>().sharedMaterial = GamePlayManager.Ins.MaterialDF;
            return;
        }
        Renderer.sharedMaterial = GamePlayManager.Ins.MaterialDF;
    }
    public BlockColor CheckColor()
    {
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return BlockColor.None;

        ChildBlock last = ListChildBlock[ListChildBlock.Count - 1];
        if (last == null)
            return BlockColor.None;

        return last.CurrenColor;
    }
    public int GetNumberSameColor()
    {
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return 0;

        int count = 0;
        int index = ListChildBlock.Count - 1;
        BlockColor color = ListChildBlock[index].CurrenColor;
        while (index >= 0 && ListChildBlock[index] != null && ListChildBlock[index].CurrenColor == color)
        {
            count++;
            index--;
        }

        return count;
    }
    public void ClearDataBlockChild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {

            if (!Application.isPlaying)
            {
                if (transform.GetChild(i).gameObject.layer == 0)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }

            }
            else
            {
                if (transform.GetChild(i).gameObject.layer == 0)
                {

                    Destroy(transform.GetChild(i).gameObject);
                }

            }
        }
        ListChildBlock.Clear();
    }
    public void UpdateList()
    {
        
        if (transform.childCount == 0) { ListChildBlock.Clear(); return; }
        ListChildBlock.Clear();
         foreach (Transform i in transform)
        {
            ListChildBlock.Add(i.GetComponent<ChildBlock>());
        }
    }
    [Button(ButtonSizes.Large)]
    public void SpawnBlockChild()
    {
        ClearDataBlockChild();
        if (State == StateBlock.Lock || State == StateBlock.LockCount) return;
        List<BlockLevelDefaut> DataTg = new List<BlockLevelDefaut>();
        if (State == StateBlock.Lock || State == StateBlock.LockCount) {
            if (State == StateBlock.LockCount)
            {  DataTg = new List<BlockLevelDefaut>(DataSpawn); }
                DataSpawn.Clear();
        } 
        foreach (var i in DataSpawn)
        {
            for(int j = 0;j < i.NumberSpawm; j++)
            {
                GamePlayManager gamePlaymanager = GamePlayManager.Ins;
                ChildBlock Obj = GameManager.Ins.SpawnBlockNotBool(i.Color);
                Obj.transform.SetParent(transform);
                Obj.transform.localScale = gamePlaymanager.baseScale;
                Obj.transform.localPosition = new Vector3(0, gamePlaymanager.sizeYBlock * transform.childCount, 0);
                ListChildBlock.Add(Obj);

            }
        }
        if (State == StateBlock.LockCount) DataSpawn = DataTg;
    }
    
    public void SpawnBlockChildWithBool()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {

            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
            {
                if (transform.GetChild(i).gameObject.layer == 0)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
               
            }

            ListChildBlock.Clear();
        }
        int Index = 0;
        foreach (var i in DataSpawn)
        {
            for (int j = 0; j < i.NumberSpawm; j++)
            {
                Index++;
                GamePlayManager gamePlaymanager = GamePlayManager.Ins;
                ChildBlock Obj = GameManager.Ins.SpawnBlockChild(i.Color);
                Obj.transform.SetParent(transform);
                Obj.transform.localScale = gamePlaymanager.baseScale;
                Obj.transform.localPosition = new Vector3(0, gamePlaymanager.sizeYBlock * Index, 0);
                ListChildBlock.Add(Obj);

            }
        }
   
    }
    public void BackNomal()
    {
        State = StateBlock.Nomal;
        gameObject.layer = 3;
        Destroy(GameObjectMod.gameObject);
    }
    public void PlayEffect()
    {
        if(Effect != null)
        {
            Effect.Play();
        }
    }
    public void UpdateDataSpawn()
    {
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return;

        DataSpawn.Clear();

        int CurrenCheck = 0;
        BlockColor CurrenColor = ListChildBlock[0].CurrenColor;
        int count = 0;

        while (CurrenCheck < ListChildBlock.Count)
        {
            if (CurrenColor == ListChildBlock[CurrenCheck].CurrenColor)
            {
                count++;
            }
            else
            {
                BlockLevelDefaut Df = new BlockLevelDefaut();
                Df.Color = CurrenColor;
                Df.NumberSpawm = count;
                DataSpawn.Add(Df);
                CurrenColor = ListChildBlock[CurrenCheck].CurrenColor;
                count = 1;
            }

            CurrenCheck++;
        }
        BlockLevelDefaut last = new BlockLevelDefaut();
        last.Color = CurrenColor;
        last.NumberSpawm = count;
        DataSpawn.Add(last);
    }

    public void CopyDataFrom(BlockControl other)
    {
        transform.localPosition = other.transform.localPosition;
        BlockLink = other.BlockLink;
        ListChildBlock = other.ListChildBlock;
    }
    public List<ChildBlock> GetSameBlock ()
    {
        List<ChildBlock> ListBlock = new List<ChildBlock>();    
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return ListBlock;
        int index = ListChildBlock.Count - 1;
        
        BlockColor color = ListChildBlock[index].CurrenColor;
        while (index >= 0 && ListChildBlock[index] != null && ListChildBlock[index].CurrenColor == color)
        {
             ListBlock.Add(ListChildBlock[index]);
             index--;
        }
        return ListBlock;
    }
    public BlockControl(BlockControl bc)
    {
     ListChildBlock = bc.ListChildBlock;
     PosionBlock = bc.PosionBlock;
     Renderer  = bc.Renderer;
     BlockLink = bc.BlockLink;
}
    public List<BlockControl> CheckArow()
    {
        List<BlockControl> ListBlock = new List<BlockControl>();
       
        foreach (var i in BlockLink)
        {
            if (i != null && CheckColor() == i.CheckColor() && i.CheckColor() != BlockColor.None )
            {
                if(i.State == StateBlock.Nomal)
                ListBlock.Add(i);
            }
        }
        return ListBlock;
    }
    public void ClearLink()
    {
        if(BlockLink!=null) 
        BlockLink.Clear();
    }
}
