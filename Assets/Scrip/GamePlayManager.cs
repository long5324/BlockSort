using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GamePlayManager : Singleton<GamePlayManager>
{
    public List<GameObject> ListBlockGamePlay;
    [SerializeField] public float sizeYBlock { get; set; } = 0.003f;
    [SerializeField] public float MunberBlock = 11;
    public List<BlockControl> BottomBlock ;
    public GameObject MapGamePlay;
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
    public bool pausegame =false;
    private void Start()
    {
        animationControl = AnimationControl.Instance;
        ObjectBooling = ObjectBoolingControler.Instance;
        gameManager = GameManager.Instance;
        Application.targetFrameRate = 60;
        AdjustScaleToScreen();
        cam = Camera.main;
        SetUpChangeLevel();
        RandomSpawnBlockChild();
       
        /* setPause(true);
         setActiveListGamePlay(false);
         SetStartBlockPlay();*/
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
    public void SetPause( bool b)
    {
        pausegame = b;
    }
    private void Update()
    {
        if(pausegame) return;
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
        if (CheckLose() && animationControl.Ani == null)
        {
            SaveScore(CurrenScore);
          //  uiManager.Losegame();
        }
        if(!animationControl.IsRun &&animationControl.Ani.BlockStart==null && DelayCheck.Count >0)
        {
            Debug.Log("Run Delay Check");
            CheckFirt(DelayCheck[0]);
            DelayCheck.RemoveAt(0);
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
        if(P== null) return;    
        List<BlockControl> ListBlock = P.CheckArow();
        if(ListBlock == null || ListBlock.Count == 0) return;
        if(ListBlock.Count ==1)
        animationControl.AddAni(ListBlock[0], P);
        else if(ListBlock.Count > 1)
        {
            animationControl.AddAni(P,ListBlock[0]);
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

            TagertBlock.ListChildBlock.Add(selectedBlock.ListChildBlock[i]);
        }
        if (animationControl.ScorePlus || animationControl.IsRun)
        {
            DelayCheck.Add(TagertBlock);
           
        }
        else {
            CheckFirt(TagertBlock);
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
    public void ResetGameplay()
    {
        foreach(var i in ListBlockGamePlay)
        {
            for (int j = i.transform.childCount - 1; j >= 0; j--)
            {
                DestroyImmediate(i.transform.GetChild(j).gameObject);
                ObjectSet OS = i.transform.GetChild(j).gameObject.GetComponent<ObjectSet>();
                OS = new ObjectSet();
            }
        }
        RandomSpawnBlockChild();
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
                BlockColor color = gameManager.BlockData.DataBases[ColorBlock[j]].BlockColor;
                List<Transform> ObjectGame = ObjectBooling.getObjectChile(color, currentBlock);
                for (int k = 0; k < currentBlock; k++)
                {
                    if (k < ObjectGame.Count)
                    {
                        ObjectGame[k].gameObject.SetActive(true); 
                        ObjectGame[k].transform.SetParent(i.transform);
                        ObjectGame[k].transform.localRotation = Quaternion.identity;
                        ObjectGame[k].transform.localPosition = new Vector3(0, sizeYBlock * i.transform.childCount, 0);  
                        ObjectGame[k].transform.localScale = Vector3.one; 
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
