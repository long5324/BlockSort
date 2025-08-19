using UnityEngine;

public class BlockMaterialControl : MonoBehaviour
{
    public static BlockMaterialControl Instance { get; private set; }
    public Material MaterialGray;
    public Material MaterialOR;
    public Material MaterialBlue;
    public Material MaterialGreen;
    public Material MaterialBlack;
    public Material MaterialYellow;
    public Material MaterialWhite;
    public Material MaterialRed;
    public Material MaterialViolet;
    private void Awake()
    {
        Instance = this;
    }
}
