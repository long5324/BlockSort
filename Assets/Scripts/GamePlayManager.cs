using DG.Tweening.Core.Easing;
using Lean.Pool;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public Material LightMaterial;
    public List<BlockControl> BottomBlock { get; set; }
    public List<GameObject> ListBlockGamePlay { get; set; }
    public List<Vector3> ListDefaulPossitionBlockGamePlay { get; set; } = new List<Vector3>();
    public List<Vector3> DelayCheck { get; set; } = new List<Vector3>();
    public float sizeYBlock { get; set; } = 0.0025f;
    public float MunberBlockEat = 10;
    public GameObject MapGamePlay;
    public Block DataBlockChild;
    Camera cam;
    ObjectSet selectedBlock;
    public BlockControl TagertBlock { get; set; }
    public int CountScaleScore { get; set; } = 0;
    public bool StartScaleScore { get; set; } = false;
    public int CurrenScore { get; set; } = 0;
    public int ScorePluss { get; set; } = 0;
    public Vector3 baseScale { get; private set; } = new Vector3(0.9f, 0.9f, 0.9f);
    private float referenceWidth = 1080f;
    private float referenceHeight = 2280f;
    private bool pause = false;
    DataInport Data;
    
    private void Start()
    {
        Data = DataInport.Ins;
        Application.targetFrameRate = 60;
        AdjustScaleToScreen();
        cam = Camera.main;
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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Block"))
            {
                selectedBlock = hit.collider.gameObject.GetComponent<ObjectSet>();
            }
        }
    }
    private GameObject previousBlock;

    void CheckBottomBlock()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GridBlock")))
        {
            GameObject bottomBlockObject = hit.collider.gameObject;
            BlockControl bottomBlockControl = bottomBlockObject.GetComponent<BlockControl>();

            if (TagertBlock != null)
            {
                TagertBlock.SetColor(LightMaterial);
            }


            if (bottomBlockControl != null && bottomBlockControl.CompareTag("BottomBlock"))
            {

                TagertBlock = bottomBlockControl;

                if (bottomBlockControl.transform.childCount > 0)
                {
                    TagertBlock = null;
                }

                if (previousBlock != null && previousBlock != bottomBlockObject)
                {
                    BlockControl previousBlockControl = previousBlock.GetComponent<BlockControl>();
                    if (previousBlockControl != null)
                    {

                        previousBlockControl.BacktoDFColor();
                    }


                    previousBlock = bottomBlockObject;
                }
            }

            else if (bottomBlockControl == null && TagertBlock != null)
            {
                TagertBlock.BacktoDFColor();
                TagertBlock = null;
            }
            foreach(var i in BottomBlock)
            {
                if(i != TagertBlock)
                {
                    i.BacktoDFColor();
                }
            }

            Vector3 targetPos = hit.point ;
            targetPos.y = hit.point.y + 2f;
            selectedBlock.transform.position = targetPos;
        }
        else
        {
          
            if (previousBlock != null)
            {
                TagertBlock = null;

                BlockControl previousBlockControl = previousBlock.GetComponent<BlockControl>();
                if (previousBlockControl != null)
                {
                 
                    previousBlockControl.BacktoDFColor();
                }

                previousBlock = null;
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
        if (TagertBlock == null) return;
        TagertBlock.GetComponent<BlockControl>().BacktoDFColor();
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
        if (P == null) { Debug.Log("Pnull" + Po); return; }
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
        if (TagertBlock == null)
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
        TagertBlock = null;
        selectedBlock = null;
    }

    void SetBlock()
    {
        if (selectedBlock == null || TagertBlock == null) return;
        for(int i=0; i < selectedBlock.ListChildBlock.Count; i++)
        {
            selectedBlock.ListChildBlock[i].transform.SetParent(TagertBlock.transform);
            selectedBlock.ListChildBlock[i].transform.localPosition = new Vector3(0,sizeYBlock*(i+1),0);
            selectedBlock.ListChildBlock[i].transform.localRotation = Quaternion.identity;
            selectedBlock.ListChildBlock[i].transform.localScale = baseScale;
            TagertBlock.ListChildBlock.Add(selectedBlock.ListChildBlock[i]);
        }
        if (Data.animationControl.ScorePlus || Data.animationControl.IsRun)
        {
            DelayCheck.Add(TagertBlock.PosionBlock);
           
        }
        else {
            Debug.Log("Check");
            CheckFirt(TagertBlock.PosionBlock);
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
            Col.size = new Vector3(Col.size.x, SizeY, Col.size.z);
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
