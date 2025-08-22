using DG.Tweening.Core.Easing;

using NUnit;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GameManager : Singleton<GameManager>
{
    public List<GameObject> ListBlockGamePlay;
    [SerializeField] GameObject BottomBlockGameObject;
    [SerializeField] GameObject PrefabBlockChild;
    [SerializeField] public float sizeYBlock { get;  set; } = 0.003f;
    [SerializeField] public float MunberBlock = 11;

    public  List<BlockControl> BottomBlock { get; set; } = new List<BlockControl>();
    Camera cam;
    GameObject selectedBlock;
    AnimationControl animationControl;
    BlockMaterialControl blockMaterialControl;
    UIManager uiManager;
    List<Vector3> ListDefaulPossitionBlockGamePlay = new List<Vector3>();
    public GameObject TagertBlock { get; set; }
    public int CountScaleScore { get; set; } = 0;
    public bool StartScaleScore { get; set; } = false;
    public int CurrenScore { get; set; } = 0;
    public int ScorePluss { get; set; } = 0;
    bool pause = false;
    private Vector3 baseScale = new Vector3(0.9f, 0.9f, 0.9f);
    private float referenceWidth = 1080f;
    private float referenceHeight = 2280f;
    private void Start()
    {
        Application.targetFrameRate = 60;
        AdjustScaleToScreen();
        uiManager = UIManager.Instance;
        animationControl = AnimationControl.Instance;
        blockMaterialControl = BlockMaterialControl.Instance;
        cam = Camera.main;
        foreach(var i in ListBlockGamePlay)
        {
            ListDefaulPossitionBlockGamePlay.Add(i.transform.position);
        }
        if (BottomBlockGameObject != null)
        for(int i = 0; i < BottomBlockGameObject.transform.childCount - 1; i++)
        {
            for (int j = 0; j < BottomBlockGameObject.transform.GetChild(i).childCount; j++) {
                BottomBlockGameObject.transform.GetChild(i).GetChild(j).GetComponent<BlockControl>().PosionBlock = new Vector2(i, j);
                BottomBlock.Add(BottomBlockGameObject.transform.GetChild(i).GetChild(j).gameObject.GetComponent<BlockControl>());
            }

        }

        uiManager.SetMaxScore(LoadScore());
        RandomSpawnBlockChild();
        setPause(true);
        setActiveListGamePlay(false);
        SetStartBlockPlay();
    }

    private void Update()
    {
        if (pause) return;
        if (Input.GetMouseButtonDown(0))
        {
            TagertBlockPlay();
        }
        if (selectedBlock != null && Input.GetMouseButton(0))
        {
            CheckBottomBlock();
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndClicK();
        }
        if (CheckLose() && animationControl.ListAni.Count ==0)
        {
            SaveScore(CurrenScore);
            uiManager.Losegame();
               
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
    public void setActiveListGamePlay(bool b)
    {
        foreach (var i in ListBlockGamePlay) { 
            i.SetActive(b);
        }
    }
    public void CheckFirt(BlockControl P)
    {
        List<BlockControl> ListCheck = new List<BlockControl>();
        int countCanChange = 0;

        foreach (var j in BottomBlock)
        {
            if (j.PosionBlock == P.PosionBlock + new Vector2(1, 1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(1, 0)
                || j.PosionBlock == P.PosionBlock + new Vector2(0, 1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, 0)
                || j.PosionBlock == P.PosionBlock + new Vector2(0, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(1, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, 1))
            {
                ListCheck.Add(j);
            }
        }
        foreach (var k in ListCheck)
        {
            if (k.ListChildBlock.Count > 0
                && P.ListChildBlock.Count > 0
                && k.ListChildBlock[0].Material.name == P.ListChildBlock[0].Material.name)
            {
                countCanChange++;
            }
        }

        foreach (var k in ListCheck)
        {
            if (countCanChange > 1)
                Sortspecifically(k, P);
            else if(countCanChange ==1) Sortspecifically(P, k);
        }
        countCanChange = 0;
        foreach (var k in ListCheck)
        {
            if (k.ListChildBlock.Count > 0
                && P.ListChildBlock.Count > 0
                && k.ListChildBlock[0].Material.name == P.ListChildBlock[0].Material.name)
            {
                countCanChange++;
            }
        }
        if(countCanChange > 0)
        {
            CheckFirt( P);
        }
       
    }
    public void CheckBlock()
    {
        foreach (var i in BottomBlock)
        {
            // Kiểm tra nếu số lượng ListChildBlock lớn hơn số lượng child
            if (i.ListChildBlock.Count > i.transform.childCount)
            {
                i.ListChildBlock.Clear(); // Xóa ListChildBlock nếu cần thiết
            }

            // Vòng lặp qua tất cả các đối tượng con trong transform của block
            for (int j = 0; j < i.transform.childCount; j++)
            {
                ChildBlock cl = new ChildBlock();
                cl.Material = i.transform.GetChild(j).GetComponent<Renderer>().material;
                i.ListChildBlock.Add(cl);
            }
        }
    }

    public void setPause(bool b)
    {
        pause = b;
    }
    public void SortAll()
    {
        bool check = false;
        for (int i = 0; i < BottomBlock.Count; i++)
        {
            if (BottomBlock[i].ListChildBlock.Count == 0) continue;

            List<BlockControl> ListCheck = new List<BlockControl>();
            foreach (var j in BottomBlock)
            {
                if (j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, 1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, 0)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(0, 1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, 0)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(0, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, 1))
                {
                    ListCheck.Add(j);
                }
            }
            foreach (var k in ListCheck)
            {
                if (k.ListChildBlock.Count == 0) continue;
                if (BottomBlock[i].transform.childCount == 0) continue;
                if (BottomBlock[i].ListChildBlock.Count == 0) continue; 
                if (k.ListChildBlock[0].Material.name != BottomBlock[i].ListChildBlock[0].Material.name) continue;

                Sortspecifically(BottomBlock[i],k);
            }
        }
        if (check)
        {
            SortAll();
        }
       
    }
    void Sortspecifically(BlockControl start , BlockControl end)
    {
        int countChange = 0;

        if (end.ListChildBlock.Count > 0) 
        {
            while (countChange < start.ListChildBlock.Count
                   && start.ListChildBlock[countChange].Material.name == end.ListChildBlock[0].Material.name)
            {
                countChange++;
            }
        }
        if(countChange > 0) 

        animationControl.AddAni(start, end, countChange);
        

        for (int z = 0; z < countChange; z++)
        {
            end.ListChildBlock.Insert(0, end.ListChildBlock[0]);
            start.ListChildBlock.RemoveAt(0);
        }
        
    }
    void TagertBlockPlay() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Block"))
            {
                selectedBlock = hit.collider.gameObject;
            }
        }
    }
    void CheckBottomBlock()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GridBlock")))
        {
            Vector3 targetPos = hit.point;
            Ray downRay = new Ray(targetPos + Vector3.up * 50f, Vector3.down);
            RaycastHit downHit;
            if (Physics.Raycast(downRay, out downHit, 100f, LayerMask.GetMask("GridBlock")))
            {
                foreach (var i in BottomBlock)
                {
                    if (blockMaterialControl != null)
                        i.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[0];
                    TagertBlock = null;
                }
                GameObject BottomBlockTagert = downHit.collider.gameObject;
                if (BottomBlockTagert.CompareTag("BottomBlock"))
                {
                    if (blockMaterialControl != null)
                    {
                        BottomBlockTagert.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[1];
                        TagertBlock = BottomBlockTagert;
                        if(BottomBlockTagert.transform.childCount > 0)
                        {
                            TagertBlock = null;
                        }
                    }
                }
                targetPos.y = downHit.point.y + 1f;
            }
            else
            {
                targetPos.y = hit.point.y + 1f;
            }
            selectedBlock.transform.position = targetPos;
        }
    }
    void EndClicK()
    {
        foreach (var i in BottomBlock)
        {
            if (blockMaterialControl != null)
                i.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[0];
        }
       
        if(TagertBlock==null)
        for (int i = 0; i < ListBlockGamePlay.Count; i++)
        {
            ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
        }
        else {
            SetBlock();
        }
        if(TagertBlock!=null)
        CheckFirt(TagertBlock.GetComponent<BlockControl>());
        selectedBlock = null;
    }
    void ChangeChildBlock(BlockControl Start, BlockControl End)
    {
        int countChange = Start.transform.childCount;
        for (int i = countChange - 1; i >= 0; i--)
        {
            Transform child = Start.transform.GetChild(i);
            child.SetParent(End.transform);
            child.localPosition = new Vector3(0, sizeYBlock * (countChange+1- End.transform.childCount), 0);
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.identity;
            End.ListChildBlock.Add(Start.ListChildBlock[i]);
        }
        Start.ListChildBlock.Clear();
    }
    void SetBlock()
    {
        if (selectedBlock == null || TagertBlock == null) return;

        ChangeChildBlock(
            selectedBlock.GetComponent<BlockControl>(),
            TagertBlock.GetComponent<BlockControl>()
        );
        for (int i = 0; i < ListBlockGamePlay.Count; i++)
        {
            ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
        }
        bool hasChild = ListBlockGamePlay.Exists(b => b.transform.childCount > 0);

        if (!hasChild)
        {
            foreach (var block in ListBlockGamePlay)
            {
                block.SetActive(true);
            }
            RandomSpawnBlockChild();
        }
    }
    void SetStartBlockPlay()
    {
        foreach (var i in ListBlockGamePlay)
        {
            for (int j = 0; j < i.transform.childCount; j++)
            {
                Vector3 pos = i.transform.GetChild(j).transform.position;
                i.transform.GetChild(j).transform.position = new Vector3(pos.x, pos.y + 4, pos.z);
            }
        }
    }

    void RandomSpawnBlockChild()
    {

        foreach (var i in ListBlockGamePlay)
        {
            int countColor = Random.Range(1, 4);
            int countBlock = Random.Range(2, 7);
            List<int> ColorBlock = new List<int>();
            for (int j = 0; j < countColor; j++)
            {
                ColorBlock.Add(UnityEngine.Random.Range(2, 8));
            }
            int BlockE = countBlock;
            for (int j = 1; j <= countColor; j++)
            {
                if (BlockE <= 0) break ;
                int cureenBlock = Random.Range(1, BlockE + 1);
                BlockE = BlockE - cureenBlock;
                for (int k = 1; k <= cureenBlock; k++)
                {
                    GameObject ChildGameObject = Instantiate(PrefabBlockChild, i.transform.position + new Vector3(0, sizeYBlock * (j + 1), 0), Quaternion.identity);
                    ChildGameObject.transform.SetParent(i.transform);
                    ChildGameObject.transform.localPosition = new Vector3(0, sizeYBlock * i.transform.childCount, 0);
                    ChildGameObject.transform.localScale = new Vector3(1, 1, 1);
                    ChildGameObject.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[ColorBlock[j - 1]];
                    ChildBlock Child = new ChildBlock();
                    Child.Material = ChildGameObject.gameObject.GetComponent<Renderer>().material;
                    i.GetComponent<BlockControl>().ListChildBlock.Add(Child);
                }
            }
        }
    }
    public int CheckScore(BlockControl Count)
    {
        int countScore = 1;
        if (Count == null || Count.ListChildBlock == null || Count.ListChildBlock.Count == 0)
            return 0;

        if (Count.transform.childCount < MunberBlock)
            return countScore;
        string nameMaterial = Count.ListChildBlock[0].Material != null
            ? Count.ListChildBlock[0].Material.name
            : string.Empty;
        while (countScore < Count.ListChildBlock.Count)
        {
            var block = Count.ListChildBlock[countScore];
            if (block == null || block.Material == null) break;

            if (block.Material.name != nameMaterial) break;

            countScore++;
        }

        return countScore;
    }
    private void OnApplicationQuit()
    {
        SaveScore(CurrenScore);
    }
    
    public void UpdateScore()
    {
        int scalse = (CountScaleScore / 5)+1;
        CurrenScore  += scalse * ScorePluss;
        uiManager.SetScore(CurrenScore);
        CountScaleScore = 0;
        ScorePluss = 0;
    }
    public void SaveScore(int score)
    {
        int lastScore = LoadScore();
        if (score > lastScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }
    }

    public int LoadScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0); 
    }
    public bool CheckLose()
    {
        foreach(var i in BottomBlock)
        {
            if(i.ListChildBlock.Count==0){
             return false;
            }
        }
        return true;    
    }
}
