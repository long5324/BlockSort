using UnityEngine;

public class ChildBlock : MonoBehaviour
{
    [System.Serializable]
    public enum ColorBlock
    {
        black,
        blue,
        green,
        yellow,
        white,
        red,
        violet,
    }
    public ColorBlock color;
}
