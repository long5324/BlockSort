using DG.Tweening.Core.Easing;
using Lean.Pool;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static UnityEngine.GraphicsBuffer;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public Material MaterialDF;
    public Material LightMaterial;
    public List<BlockControl> BottomBlock { get; set; }
    public List<GameObject> ListBlockGamePlay { get; set; }
    public List<Vector3> ListDefaulPossitionBlockGamePlay { get; set; } = new List<Vector3>();
    public List<Vector3> DelayCheck { get; set; } = new List<Vector3>();
    public float sizeYBlock { get; set; } = 0.00325f;
    public float MunberBlockEat = 10;
    public GameObject MapGamePlay;
    public Block DataBlockChild;
    Camera cam;
    ObjectSet selectedBlock = null;
    public BlockControl TargetBlock { get; set; }
    public int CountScaleScore { get; set; } = 0;
    public bool StartScaleScore { get; set; } = false;
    public int CurrenScore { get; set; } = 0;
    public int ScorePluss { get; set; } = 0;
    
    public Vector3 baseScale { get; private set; } = new Vector3(0.9f, 0.9f, 0.9f);
    private float referenceWidth = 1080f;
    private float referenceHeight = 2280f;
    private bool pause = false;
    private List<BlockControl> ListBlockLock = new List<BlockControl>();

    DataInport Data;
    public LayerMask BlockLM;

    private void Start()
    {
        Data = DataInport.Ins;
        Application.targetFrameRate = 60;
        AdjustScaleToScreen();
        cam = Camera.main;

    }
    public void UpdateListBlockLock()
    {
        ListBlockLock.Clear();
        foreach (var i in BottomBlock)
        {
            if (i.State == StateBlock.Lock)
            {
                ListBlockLock.Add(i);
               
            }
        }
    }
    public void SetUpChangeLevel()
    {
        ListDefaulPossitionBlockGamePlay.Clear(); BottomBlock.Clear();  
        foreach (var i in ListBlockGamePlay)
        {
            ListDefaulPossitionBlockGamePlay.Add(i.transform.position);
        }
        foreach (Transform i in MapGamePlay.transform)
        {
            BottomBlock.Add(i.GetComponent<BlockControl>());
        }
      
    }
    private void Update()
    {
        if (pause) return;
       
        if (selectedBlock == null && Input.GetMouseButtonDown(0))
        {
            TargetBlockPlay();
            CheckUnLockBlock();
        }
        if (selectedBlock != null && Input.GetMouseButton(0))
        {
            CheckBottomBlock();
        }
        if (Input.GetMouseButtonUp(0))
        {
           
            SetAllDefaut();
            EndClicK();
        }
        if(!Data.animationControl.IsRun && !Data.animationControl.ScorePlus && Data.animationControl.Ani.BlockStart ==null && DelayCheck.Count >0)
        {
            CheckFirt(DelayCheck[0]);
            DelayCheck.RemoveAt(0);
        }
    }
    public void SetPause(bool p)
    {
        pause = p;
    }
    public void setColliderSize()
    {
        foreach (var i in ListBlockGamePlay)
        {
            BoxCollider Col = i.GetComponent<BoxCollider>();
            Col.size = new Vector3(Col.size.x, Col.size.y *( i.transform.childCount ), Col.size.z);
            Col.center =new Vector3(0, Col.size.y/2f, 0);
        }
    }
    void AdjustScaleToScreen()
    {
        float currentWidth = Screen.width;
        float currentHeight = Screen.height;
        float widthRatio = currentWidth / referenceWidth;
        float heightRatio = currentHeight / referenceHeight;
        float scaleRatio = Mathf.Min(widthRatio, heightRatio);
        transform.localScale = baseScale * scaleRatio;
    }
    public void SetActiveListGamePlay(bool b)
    {
        foreach (var i in ListBlockGamePlay)
        {
            i.SetActive(b);
        }
    }
    void TargetBlockPlay()
    {
        if (cam == null)
        {
            return;
        }
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, BlockLM))
        {
            selectedBlock = hit.collider.gameObject.GetComponent<ObjectSet>();
        }
    }

    void CheckBottomBlock()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition + new Vector3(0,150,0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GridBlock")))
        {
            if(hit.collider == null) return;
            GameObject bottomBlockObject = hit.collider.gameObject;
            Vector3 targetPos = hit.point;
            targetPos.y = hit.point.y + 0.5f;
            selectedBlock.transform.position = targetPos ;
            if (TargetBlock == null )
            {
                TargetBlock = hit.collider.gameObject.GetComponent<BlockControl>();
            }
            else
            {
                if(hit.collider.gameObject == TargetBlock.gameObject)
                {
                    return;
                }
                else
                {
                    TargetBlock.BacktoDFColor();
                    BlockControl Check = hit.collider.gameObject.GetComponent<BlockControl>();
                    if (Check != null)
                    {
                        TargetBlock = Check;
                    }
                    if (TargetBlock.State == StateBlock.Lock)
                        return;
                    TargetBlock.SetColor(LightMaterial);
                    if (TargetBlock.transform.childCount > 0)
                    {
                        TargetBlock.BacktoDFColor();
                    }
                }
            }
        }
    }
    private void CheckUnLockBlock()
    {
        if (ListBlockLock.Count == 0) return;
        Debug.Log(1);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Lock")))
        { 
            BlockControl target = null;
            foreach (var i in ListBlockLock)
            {
                if (i.gameObject == hit.collider.gameObject)
                {
                    target = i;
                    break;
                }
            }

            if (target != null)
            {
                target.State = StateBlock.Nomal;
                target.BacktoDFColor();
                Destroy(target.GameObjectMod.gameObject);
                target.GameObjectMod = null;
                target.gameObject.layer = 3;
                ListBlockLock.Remove(target);
            }
        }
    }

    public bool CheckLost()
    {
        foreach(var i in BottomBlock)
        {
            if(i.ListChildBlock.Count ==0) return false;
            if(i.CheckArow().Count > 0) return false;
        }
        return true;
    }

    void CheckGamePlay()
    {
        bool c = false;
        foreach(var i in ListBlockGamePlay)
        {
            if (i.transform.childCount > 0)
            {
                c = true;break;
            }
        }
        if (!c)
        {
            RandomSpawnBlockChild();    
        }
    }
    void SetAllDefaut()
    {
        if (TargetBlock == null) return;
        TargetBlock.GetComponent<BlockControl>().BacktoDFColor();
    }

    public void CheckFirt(Vector3 Po)
    {

        BlockControl P = null;
        foreach (var i in BottomBlock)
        {
            if (i.PosionBlock == Po)
            {
                P = i;
                break;
            }
        }
        if (P == null) {return; }
        List<BlockControl> ListBlock = P.CheckArow();
        if (ListBlock == null || ListBlock.Count == 0) return;
            if (ListBlock.Count == 1)
            {
                Data.animationControl.AddAni(ListBlock[0], P);
            }
            else if (ListBlock.Count > 1)
            {

                Data.animationControl.AddAni(P, ListBlock[0]);
            }
        

    }

    void EndClicK()
    {
        if(selectedBlock == null) { return; }
        if (TargetBlock == null)
            for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {
                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        else
        {
          
             for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {
                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        }
        SetBlock();
        selectedBlock = null;
        TargetBlock = null;
    }

    void SetBlock()
    {
        if (selectedBlock == null || TargetBlock == null || TargetBlock.transform.childCount >0 || TargetBlock.State == StateBlock.Lock) return;
        for(int i=0; i < selectedBlock.ListChildBlock.Count; i++)
        {
            selectedBlock.ListChildBlock[i].transform.SetParent(TargetBlock.transform);
            selectedBlock.ListChildBlock[i].transform.localPosition = new Vector3(0,sizeYBlock*(i+1),0);
            selectedBlock.ListChildBlock[i].transform.localRotation = Quaternion.identity;
            selectedBlock.ListChildBlock[i].transform.localScale = baseScale;
            TargetBlock.ListChildBlock.Add(selectedBlock.ListChildBlock[i]);
        }
        if (Data.animationControl.ScorePlus || Data.animationControl.IsRun)
        {
            DelayCheck.Add(TargetBlock.PosionBlock);
           
        }
        else { 
            CheckFirt(TargetBlock.PosionBlock);
        }
        selectedBlock.ListChildBlock.Clear();
        CheckGamePlay();
       
    }
    public void RandomSpawnBlockChild()
    {
        foreach (var i in ListBlockGamePlay)
        {
            int countColor = Random.Range(1, 4); 
            int countBlock = Random.Range(2, 7); 
            List<int> ColorBlock = new List<int>();;
            for (int j = 0; j < countColor; j++)
            {
                ColorBlock.Add(Random.Range(0, 7));
            }
            int BlockE = countBlock;
            for (int j = 0; j < countColor; j++) 
            {
                if (BlockE <= 0) break;
                int currentBlock = Random.Range(1, BlockE + 1);  
                BlockE -= currentBlock;

                BlockColor color = Data.gameManager.BlockData.BlockDataBase[ColorBlock[j]].Color;
                List<Transform> ObjectGame = new List<Transform>();
                for (int k = 0; k < countBlock; k++)
                {
                   ObjectGame.Add(GameManager.Ins.SpawnBlockChild(color).transform);

                }
                for (int k = 0; k < currentBlock; k++)
                {
                    if (k < ObjectGame.Count)
                    {
                        ObjectGame[k].gameObject.SetActive(true); 
                        ObjectGame[k].transform.SetParent(i.transform);
                        ObjectGame[k].transform.localRotation = Quaternion.identity;
                        ObjectGame[k].transform.localPosition = new Vector3(0, sizeYBlock * i.transform.childCount, 0);  
                        ObjectGame[k].transform.localScale = baseScale; 
                    }
                }
            }
            ObjectSet OJS = i.GetComponent<ObjectSet>();

            OJS.AddLisst();

        }
        foreach (var i in ListBlockGamePlay)
        {
            BoxCollider Col = i .GetComponent<BoxCollider>();
            ObjectSet OJS = i.GetComponent<ObjectSet>();
            float SizeY = 0.005f * OJS.ListChildBlock.Count;
            Col.size = new Vector3(Col.size.x , SizeY , Col.size.z );
            Col.center = new Vector3(Col.center.x, SizeY/3, Col.center.z);
        }
    }

    public int CheckScore(BlockControl Count)
    {
        int countScore = 0;
        BlockColor Color = Count.ListChildBlock[Count.ListChildBlock.Count-1].CurrenColor;
        for(int i = Count.ListChildBlock.Count - 1; i >=0; i--)
        {
            if (Count.ListChildBlock[i].CurrenColor == Color)
            {
                countScore++;
            }
            else { break; }
        }
    
        return countScore;
    }
    public void UpdateScore()
    {
        int scalse = (CountScaleScore / 5) + 1;
        CurrenScore += scalse * ScorePluss;
        UIManager.Ins.GetUI<GameplayUI>().SetFillScore(CurrenScore,Data.gameManager.MaxCurrenScore);
        CountScaleScore = 0;
        ScorePluss = 0;
    }
}
