using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public List<GameObject> ListBlockGamePlay;
    [SerializeField] GameObject BottomBlockGameObject;
    [SerializeField] public float sizeYBlock { get; set; } = 0.003f;
    [SerializeField] public float MunberBlock = 11;
    public List<BlockControl> BottomBlock { get; set; } = new List<BlockControl>();
    Camera cam;
    ObjectSet selectedBlock;
    public BlockControl TagertBlock { get; set; }
    UIManager uiManager;
    List<Vector3> ListDefaulPossitionBlockGamePlay = new List<Vector3>();
    AnimationControl animationControl;
    public int CountScaleScore { get; set; } = 0;
    public bool StartScaleScore { get; set; } = false;
    public int CurrenScore { get; set; } = 0;
    public int ScorePluss { get; set; } = 0;
    bool pause = false;
    private Vector3 baseScale = new Vector3(0.9f, 0.9f, 0.9f);
    private float referenceWidth = 1080f;
    private float referenceHeight = 2280f;
    GameManager gameManager;
    ObjectBoolingControler ObjectBooling;
    public List<BlockControl> DelayCheck = new List<BlockControl>();
    private void Start()
    {
        animationControl = AnimationControl.Instance;
        ObjectBooling = ObjectBoolingControler.Instance;
        gameManager = GameManager.Instance;
        Application.targetFrameRate = 60;
        AdjustScaleToScreen();
        cam = Camera.main;
        foreach (var i in ListBlockGamePlay)
        {
            ListDefaulPossitionBlockGamePlay.Add(i.transform.position);
        }
        if (BottomBlockGameObject != null)
            for (int i = 0; i < BottomBlockGameObject.transform.childCount - 1; i++)
            {
                for (int j = 0; j < BottomBlockGameObject.transform.GetChild(i).childCount; j++)
                {
                    BottomBlockGameObject.transform.GetChild(i).GetChild(j).GetComponent<BlockControl>().PosionBlock = new Vector2(i, j);
                    BottomBlock.Add(BottomBlockGameObject.transform.GetChild(i).GetChild(j).gameObject.GetComponent<BlockControl>());
                }

            }
        RandomSpawnBlockChild();
        setColliderSize();
        /* setPause(true);
         setActiveListGamePlay(false);
         SetStartBlockPlay();*/
    }

   
    private void Update()
    {
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
        if (CheckLose() && animationControl.ListAni.Count == 0)
        {
            SaveScore(CurrenScore);
            uiManager.Losegame();

        }
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

        // Kiểm tra xem ray có cắt vào bất kỳ đối tượng nào trong Layer "GridBlock" không
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GridBlock")))
        {
            GameObject bottomBlockObject = hit.collider.gameObject;
            BlockControl bottomBlockControl = bottomBlockObject.GetComponent<BlockControl>();

            // Kiểm tra nếu TagertBlock không phải null và cập nhật màu
            if (TagertBlock != null)
            {
                TagertBlock.SetColor(gameManager.BlockData.DataBases[gameManager.BlockData.DataBases.Count - 2].BlockMaterial);
            }

            // Kiểm tra nếu TagertBlock là null và bottomBlockControl không phải null và có tag "BottomBlock"
            if ( bottomBlockControl != null && bottomBlockControl.CompareTag("BottomBlock"))
            {
                if (gameManager.BlockData.DataBases != null && gameManager.BlockData.DataBases.Count > 1)
                {
                    TagertBlock = bottomBlockControl;

                    // Nếu block đã có child, reset TagertBlock
                    if (bottomBlockControl.transform.childCount > 0)
                    {
                        TagertBlock = null;
                    }

                    // Nếu block trước đó khác block hiện tại, reset lại material của block trước đó
                    if (previousBlock != null && previousBlock != bottomBlockObject)
                    {
                        BlockControl previousBlockControl = previousBlock.GetComponent<BlockControl>();
                        if (previousBlockControl != null)
                        {
                            Material defaultMaterial = gameManager.BlockData.DataBases[gameManager.BlockData.DataBases.Count - 1].BlockMaterial;
                            previousBlockControl.GetComponent<Renderer>().material = defaultMaterial;
                        }
                    }

                    previousBlock = bottomBlockObject; // Cập nhật previousBlock
                }
            }
            // Nếu bottomBlockControl là null hoặc TagertBlock không phải null và không phải "BottomBlock"
            else if (bottomBlockControl == null && TagertBlock != null)
            {
                TagertBlock.SetColor(gameManager.BlockData.DataBases[gameManager.BlockData.DataBases.Count - 1].BlockMaterial);
                TagertBlock = null; // Reset TagertBlock
            }

            // Cập nhật vị trí của selectedBlock
            Vector3 targetPos = hit.point;
            targetPos.y = hit.point.y + 1f; // Điều chỉnh vị trí y cho block
            selectedBlock.transform.position = targetPos;
        }
        else
        {
            // Nếu raycast không cắt vào bất kỳ block nào, reset TagertBlock
            if (previousBlock != null)
            {
                TagertBlock = null;

                BlockControl previousBlockControl = previousBlock.GetComponent<BlockControl>();
                if (previousBlockControl != null)
                {
                    Material defaultMaterial = gameManager.BlockData.DataBases[gameManager.BlockData.DataBases.Count - 1].BlockMaterial;
                    previousBlockControl.GetComponent<Renderer>().material = defaultMaterial;
                }

                previousBlock = null; // Reset previousBlock
            }
        }
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
           TagertBlock.GetComponent<BlockControl>().SetColor(gameManager.BlockData.DataBases[gameManager.BlockData.DataBases.Count - 1].BlockMaterial);
    }

    public void CheckFirt(BlockControl P)
    {

        List<BlockControl> ListCheck = new List<BlockControl>();
        int manychange = 0;
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

        foreach (var i in ListCheck)
        {
            if (i.ListChildBlock.Count == 0 || P.ListChildBlock.Count ==0) continue;

            if (i.ListChildBlock[i.ListChildBlock.Count - 1].CurrenColor
                == P.ListChildBlock[P.ListChildBlock.Count - 1].CurrenColor)
            {
                manychange++;
            }
        }
        foreach (var i in ListCheck)
        {
            if (i.ListChildBlock.Count == 0 || P.ListChildBlock.Count == 0) continue;

            if (i.ListChildBlock[i.ListChildBlock.Count - 1].CurrenColor
                == P.ListChildBlock[P.ListChildBlock.Count - 1].CurrenColor)
            {
                if(manychange ==1)
                Sortspecifically(P, i);
                else if(manychange >1)
                {
                    Sortspecifically(i, P);
                }
            }
        }
        if(manychange == 0)
        {
            SortAll();
        }
       
    }
    public void setPause(bool b)
    {
        pause = b;
    }
    public void SortAll()
    {
        for (int i = 0; i < BottomBlock.Count; i++)
        {
            var current = BottomBlock[i];
            if (current.ListChildBlock.Count == 0) continue;

            List<BlockControl> ListCheck = new List<BlockControl>();
            foreach (var j in BottomBlock)
            {
                Vector2 diff = j.PosionBlock - current.PosionBlock;
                if (diff == new Vector2(1, 1) || diff == new Vector2(-1, -1) ||
                    diff == new Vector2(1, 0) || diff == new Vector2(0, 1) ||
                    diff == new Vector2(-1, 0) || diff == new Vector2(0, -1) ||
                    diff == new Vector2(1, -1) || diff == new Vector2(-1, 1))
                {
                    ListCheck.Add(j);
                }
            }
            foreach (var k in ListCheck)
            {
                if (current.ListChildBlock.Count == 0) continue;
                if (k.ListChildBlock.Count == 0) continue;
                if (k.ListChildBlock[0].CurrenColor != current.ListChildBlock[0].CurrenColor) continue;
                Sortspecifically(current, k);
                return;
            }
        }

    }
    void Sortspecifically(BlockControl start, BlockControl end)
    {
        foreach(var i in animationControl.ListAni)
        {
            if(i.BlockStart == start && i.BlockEnd == end) return;
        }
        int countChange = 0;
        int indexSatrt = start.ListChildBlock.Count-1;
        BlockColor Color = end.ListChildBlock[end.ListChildBlock.Count-1].CurrenColor;
        while (indexSatrt>=0&&start.ListChildBlock[indexSatrt].CurrenColor == Color)
        {
            countChange++;
            indexSatrt--;
        }
        animationControl.AddAni(start, end, countChange);
    }


    void EndClicK()
    {

        if (TagertBlock == null)
            for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {
                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        else
        {
            SetBlock();
             for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {
                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        }
            TagertBlock = null;
            selectedBlock = null;
    }
   /* void ChangeChildBlock(BlockControl Start, BlockControl End)
    {

        int countChange = Start.transform.childCount;
        for (int i = countChange - 1; i >= 0; i--)
        {
            Transform child = Start.transform.GetChild(i);
            child.SetParent(End.transform);
            child.localPosition = new Vector3(0, sizeYBlock * (countChange + 1 - End.transform.childCount), 0);
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.identity;
            End.ListChildBlock.Add(Start.ListChildBlock[i]);
        }
        Start.ListChildBlock.Clear();
    }*/
    void SetBlock()
    {
        if (selectedBlock == null || TagertBlock == null) return;
        for(int i=0; i < selectedBlock.ListChildBlock.Count; i++)
        {
            selectedBlock.ListChildBlock[i].transform.SetParent(TagertBlock.transform);
            selectedBlock.ListChildBlock[i].transform.localPosition = new Vector3(0,sizeYBlock*(i+1),0);
            selectedBlock.ListChildBlock[i].transform.localRotation = Quaternion.identity;

            TagertBlock.ListChildBlock.Add(selectedBlock.ListChildBlock[i]);
        }
        if (!animationControl.ScorePlus)
        {
            CheckFirt(TagertBlock);
        }
        else {
            DelayCheck.Add(TagertBlock);
        }
        selectedBlock.ListChildBlock.Clear();
        CheckGamePlay();
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
                BlockColor color = gameManager.BlockData.DataBases[ColorBlock[j]].BlockColor;
                List<Transform> ObjectGame = ObjectBooling.getObjectChile(color, currentBlock);
                for (int k = 0; k < currentBlock; k++)
                {
                    if (k < ObjectGame.Count)
                    {
                        ObjectGame[k].gameObject.SetActive(true); 
                        ObjectGame[k].transform.SetParent(i.transform); 
                        ObjectGame[k].transform.localPosition = new Vector3(0, sizeYBlock * i.transform.childCount, 0);  
                        ObjectGame[k].transform.localScale = new Vector3(1, 1, 1); 
                    }
                }
            }

            i.GetComponent<ObjectSet>().AddLisst();
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
    private void OnApplicationQuit()
    {
        SaveScore(CurrenScore);
    }

    public void UpdateScore()
    {
        int scalse = (CountScaleScore / 5) + 1;
        CurrenScore += scalse * ScorePluss;
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
        foreach (var i in BottomBlock)
        {
            if (i.ListChildBlock.Count == 0)
            {
                return false;
            }
        }
        return true;
    }
}
