using DG.Tweening;
using DG.Tweening.Core.Easing;
using HumanSort;
using JetBrains.Annotations;
using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
[System.Serializable]
public enum Boosters
{
    None,
    DestroyBlock,
    ChangeBlock
}
public class GamePlayManager : Singleton<GamePlayManager>
{
    [Header("Material")]
    public Material MaterialDF;
    public Material LightMaterial;
    [Header("Float")]
    public float sizeYBlock = 0.00325f;
    public float MunberBlockEat = 10;
    [Header("Layer Mash")] 
    public LayerMask BlockLM; 
    [Header("Other")]   
    public ParticleSystem EffectDestroyBlock;
    public ParticleSystem EffectBlockEat;
    public Block DataBlockChild;
   
    private Camera cam;
    public ObjectSet selectedBlock { get; private set; }  = null;
    public BlockControl TargetBlock { get; set; }
    public int CountScaleScore { get; set; } = 0;
    public int CurrenScore { get; set; } = 0;
    public int ScorePluss { get; set; } = 0;
    public List<BlockControl> BottomBlock { get; set; }
    public List<GameObject> ListBlockGamePlay { get; set; }
    public List<Vector3> ListDefaulPossitionBlockGamePlay { get; set; } = new List<Vector3>();
    public List<Vector3> DelayCheck { get; set; } = new List<Vector3>();
    public GameObject MapGamePlay { get; set; }
    public Vector3 baseScale { get; private set; } = new Vector3(0.9f, 0.9f, 0.9f);
    public Boosters StateBooter { get; set; }
    private float referenceWidth = 1080f;
    private float referenceHeight = 2280f;
    private bool pause = false;
    private bool RunBoosters = false;
    private List<BlockControl> ListBlockLock = new List<BlockControl>();
    private DataInport Data;
    public  bool TutorialActive { get; set; } = false;
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
    bool CheckClickLockBlock = false;
    private void Update()
    {
        if (TutorialActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Tutorial.Ins.Off();
                TutorialActive = false;
            }
        }
        if (selectedBlock==null&&RunBoosters && !AnimationControl.Ins.IsRun && !AnimationControl.Ins.ScorePlus)
        {

            if (Input.GetMouseButtonDown(0))
            {
                CheckClickBoosters();
            }
        }else if(RunBoosters && selectedBlock!=null && !AnimationControl.Ins.IsRun && !AnimationControl.Ins.ScorePlus)
        {

            if (selectedBlock != null && Input.GetMouseButton(0))
            {
                CheckBottomBlock();
            }
            if (Input.GetMouseButtonUp(0))
            {

                SetAllDefaut();
                EndClicK();
                Destroy(ObjectP);
                EndBoosters();
            }
        }
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
            if(CheckClickLockBlock) 
                CheckUnLockBlock();
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
        if (GameManager.Ins.RotateLevel != null)
        {
            GameManager.Ins.RotateLevel.enabled = !p;    
        }
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
    GameObject ObjectP;
    private void CheckClickBoosters()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int mask = (1 << 3) | (1 << 8);
        if (Physics.Raycast(ray, out hit, 100f, mask))
        {
            BlockControl TargetBlock  =  hit.collider.gameObject.GetComponent<BlockControl>();
            if (TargetBlock == null) return;
          
            if(TargetBlock.State == StateBlock.LockCount && StateBooter == Boosters.DestroyBlock)
            {
                Animation.Ins.HandleBlockBlockTarget(TargetBlock);
                EndBoosters();
                RunBoosters = false;
                return;
            }
            if (TargetBlock.State != StateBlock.Nomal) return;
            if (TargetBlock.transform.childCount == 0) return;
            if (StateBooter == Boosters.DestroyBlock)
            {
                WaitHammerAnimation(TargetBlock);
                //RunBoostersBreackBlock(TargetBlock);
                EndBoosters();
               
                RunBoosters = false;
            }
            else if(StateBooter == Boosters.ChangeBlock)
            {
              
                if (selectedBlock == null)
                {
                    ObjectP = new GameObject("GroupBlock");
                    ObjectP.transform.SetParent(TargetBlock.transform.parent);
                    ObjectP.transform.position = TargetBlock.transform.position;
                    BlockChangeTg = TargetBlock;
                    while (TargetBlock.transform.childCount > 0)
                    {
                        TargetBlock.transform.GetChild(0).SetParent(ObjectP.transform);
                    }
                    ObjectSet newObject = ObjectP.AddComponent<ObjectSet>();
                    newObject.ListChildBlock.AddRange(TargetBlock.ListChildBlock);
                    TargetBlock.ListChildBlock.Clear();
                    selectedBlock = newObject;
                  
                }

            }
                
        }

    }
    public void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            Destroy(child);
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
                if(TargetBlock != null)
                TargetBlock.SetColor(LightMaterial);
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
                        TargetBlock.SetColor(LightMaterial);
                    }
                    if (TargetBlock.State == StateBlock.Lock)
                        return;
                    TargetBlock.SetColor(LightMaterial);
                    Transform PostionLightEfffect = Data.gameManager.EffectSelectBlock.transform;
                    PostionLightEfffect.gameObject.SetActive(true);
                    PostionLightEfffect.position = new Vector3(TargetBlock.transform.position.x, PostionLightEfffect.position.y, TargetBlock.transform.position.z);
                    if (TargetBlock.transform.childCount > 0)
                    {
                        PostionLightEfffect.gameObject.SetActive(false);
                        TargetBlock.BacktoDFColor();
                    }
                }
            }
        }
    }
    BlockControl targetUnlock;
    private void CheckUnLockBlock()
    {
       
        if (ListBlockLock.Count == 0) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Lock")))
        {
            if (!CheckClickLockBlock)
            {
                CheckClickLockBlock = true  ;
                return;
            }
            targetUnlock = null;
            foreach (var i in ListBlockLock)
            {
                if (i.gameObject == hit.collider.gameObject)
                {
                    targetUnlock = i;
                    break;
                }
            }

            if (targetUnlock != null)
            {
                UIManager.Ins.GetUI<PopupUIunlock>().Open();
            }
        }
        CheckClickLockBlock = false;
    }
    public void ShakeObject(Transform TF)
    {
        TF.DOShakePosition(0.2f, 0.1f, 10, 90, false, true);
    }
    public void UnLockEvent()
    {
        targetUnlock.State = StateBlock.Nomal;
        targetUnlock.BacktoDFColor();
        Destroy(targetUnlock.GameObjectMod.gameObject);
        targetUnlock.GameObjectMod = null;
        targetUnlock.gameObject.layer = 3;
        ListBlockLock.Remove(targetUnlock);
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
        Transform PostionLightEfffect = Data.gameManager.EffectSelectBlock.transform;
        PostionLightEfffect.gameObject.SetActive(false);
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
        if (TargetBlock == null && RunBoosters)
        {
            SetBlock();
            selectedBlock = null;
            TargetBlock = null;
            return;
        }
        if (TargetBlock == null)

                for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {
                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        else
        {
          if(!RunBoosters)
             for (int i = 0; i < ListBlockGamePlay.Count; i++)
            {

                ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
            }
        }
        SetBlock();
        selectedBlock = null;
        TargetBlock = null;
    }
    private BlockControl BlockChangeTg;
    void SetBlock()
    {
        if((TargetBlock == null || TargetBlock.transform.childCount > 0 || TargetBlock.State == StateBlock.Lock) && RunBoosters)
        {
            TargetBlock = BlockChangeTg;
        }
        if (selectedBlock == null || TargetBlock == null || TargetBlock.transform.childCount >0 || TargetBlock.State == StateBlock.Lock) return;
        for(int i=0; i < selectedBlock.ListChildBlock.Count; i++)
        {
            selectedBlock.ListChildBlock[i].transform.SetParent(TargetBlock.transform);
            selectedBlock.ListChildBlock[i].transform.localPosition = new Vector3(0,sizeYBlock*(i+1),0);
            selectedBlock.ListChildBlock[i].transform.localRotation = Quaternion.identity;
            selectedBlock.ListChildBlock[i].transform.localScale = baseScale;
           
            TargetBlock.ListChildBlock.Add(selectedBlock.ListChildBlock[i]);
        }
        ParticleSystem EffectSetBlock = GameManager.Ins.ParticleEffectSetBlock;
        EffectSetBlock.gameObject.SetActive(true);
        EffectSetBlock.gameObject.transform.position = TargetBlock.transform.position + new Vector3(0, 0.1f, 0);
        EffectSetBlock.Clear();
        EffectSetBlock.Play();
        AudioControl.Ins.PlaySFX("Set");
        if (Data.animationControl.ScorePlus || Data.animationControl.IsRun)
        {
            DelayCheck.Add(TargetBlock.PosionBlock);
        }
        else { 
            CheckFirt(TargetBlock.PosionBlock);
        }
        selectedBlock.ListChildBlock.Clear();
        CheckGamePlay();

        StartCoroutine(GameManager.Ins.EventEndGame());
    }
    public void UpdateSocre(int NumberSocre)
    {
        CurrenScore += NumberSocre;
        UIManager.Ins.GetUI<GameplayUI>().SetScore(CurrenScore, Data.gameManager.MaxCurrenScore);
        if (CurrenScore >= Data.gameManager.MaxCurrenScore)
        {
            Data.gameManager.Winlevel();
        }
    }
    int countspawn = 0;
    int countetup = 0;
    public void RandomSpawnBlockChild()
    {
        countspawn = 0;
        countetup = 0;
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
                for (int k = 0; k < currentBlock; k++)
                {
                    countspawn++;
                    Transform obj = GameManager.Ins.SpawnBlockChild(color).transform;

                    countetup++;
                    obj.gameObject.SetActive(true);
                    obj.SetParent(i.transform);
                    obj.localRotation = Quaternion.identity;
                    obj.localPosition = new Vector3(0, sizeYBlock * i.transform.childCount, 0);
                    obj.localScale = baseScale;
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

    public void EventSupport(BlockControl BlockStart, BlockControl BlockEnd )
    {
      
        BlockData Data = new BlockData();
        Data.Color = BlockEnd.ListChildBlock[BlockEnd.ListChildBlock.Count - 1].CurrenColor;
        Data.BlockMaterial = BlockEnd.ListChildBlock[BlockEnd.ListChildBlock.Count - 1].MeshRenderer.material;
        List<ChildBlock> ListChildSpawn = new List<ChildBlock>();
        for (int i = 0; i <= 10; i++)
        {
            ChildBlock obj = LeanPool.Spawn(GameManager.Ins.BlockData.BlockPrefab);
            obj.Configure(Data);
            ListChildSpawn.Add(obj);
        }
        foreach (var i in ListChildSpawn)
        {
            i.transform.SetParent(BlockStart.transform);
            i.transform.localScale = new Vector3(0.9f,0.9f,0.9f);
            i.transform.localRotation = Quaternion.identity;
            i.transform.localPosition = Vector3.zero;
            BlockStart.ListChildBlock.Add(i);
        }
        Debug.Log(BlockStart.PosionBlock + " " + BlockEnd.PosionBlock);
        IfData Ani = new IfData();
        Ani.BlockStart = BlockStart;
        Ani.BlockEnd = BlockEnd;
        AnimationControl.Ins.Ani = Ani;
        AnimationControl.Ins.IsRun = true;
        Animation.Ins.RunUpBlocks(BlockStart,BlockEnd);
        AnimationControl.Ins.DeLayCheckScore = null;
    }
    public void WaitHammerAnimation(BlockControl TargetBlock)
    {
        Animator HammerAnimator = GameManager.Ins.HammerAnimator;
        HammerAnimator.gameObject.SetActive(true);
        float x = TargetBlock.transform.localPosition.x + 0.7f;
        float y = TargetBlock.transform.childCount * sizeYBlock * 50;
        float z = TargetBlock.transform.localPosition.z;
        HammerAnimator.transform.localPosition = new Vector3(x, y, z);
        BlockHit = TargetBlock;
        HammerAnimator.SetTrigger("Hit");
        
    }
    BlockControl BlockHit = null;
    public void RunBoostersBreackBlock() {
        if (BlockHit == null) return;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in BlockHit.transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            LeanPool.Despawn(child);
        }
        GameManager.Ins.ParticleEffectHammer.gameObject.SetActive(true);
        ParticleSystem effect = Instantiate(EffectDestroyBlock);
        effect.transform.SetParent(BlockHit.transform, false);
        effect.transform.localPosition = new Vector3(0, 0.003f, 0);
        effect.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        var main = effect.main;
        Color blockColor = BlockHit.ListChildBlock[BlockHit.ListChildBlock.Count - 1].MeshRenderer.sharedMaterial.color;
        main.startColor = blockColor;
        UpdateSocre(BlockHit.ListChildBlock.Count);
        BlockHit.ListChildBlock.Clear();
        BlockHit = null;
    }
    
    public void EndBoosters()
    {

        EvenTransPosBoosters(false);
    }
    public void SetUpBooster()
    {
        EvenTransPosBoosters(true);
    }
    public void EvenTransPosBoosters(bool Start)
    {
        Transform CameraTransform = GameManager.Ins.CurrenLevelGameObject.transform;
        GameplayUI gameplayUI = UIManager.Ins.GetUI<GameplayUI>();
        Transform TranformGamePlay = GameManager.Ins.CurrenGamePlay.transform;
        if (Start)
        {
            GameManager.Ins.CurrenGridLevel.rotate.enabled = false;
            SetPause(true);
            GameManager.Ins.CurrenLevelGameObject.transform.DORotate(new Vector3(-16, 0, 16), 1f);
            GameManager.Ins.PanelGamePlay.transform.DORotate(new Vector3(-16, 0, 16), 1f);
            foreach (RectTransform t in gameplayUI.TranformButtonBoosters)
            {
                t.DOAnchorPos3DY(-700, 0.5f);
            }
            TranformGamePlay.DOLocalMoveY(-10, 0.5f).OnComplete(() =>
            {
                gameplayUI.PanelIntroduceBoosters.DOAnchorPos3DX(0, 0.5f).OnComplete(() =>
                {
                    RunBoosters = true;
                });
            });

        }
        else
        {
            GameManager.Ins.CurrenLevelGameObject.transform.DORotate(new Vector3(0, 0, 0), 1f);
            GameManager.Ins.PanelGamePlay.transform.DORotate(new Vector3(0, 0, 0), 1f);
            gameplayUI.PanelIntroduceBoosters.DOAnchorPos3DX(-1500, 0.5f).OnComplete(() =>
            {
            foreach (RectTransform t in gameplayUI.TranformButtonBoosters)
              {
                  t.DOAnchorPos3DY(160, 0.5f);
              }
                TranformGamePlay.DOLocalMoveY(0, 0.5f).OnComplete(() =>
                {
                    RunBoosters = false;
                    SetPause(false);
                    GameManager.Ins.CurrenGridLevel.rotate.enabled = true;
                }); ;
            });
        }
    }
}
