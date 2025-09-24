using UnityEngine;

public class CheatMenu : MonoBehaviour
{
    bool showMenu = true; // để test thì bật sẵn
    int targetLevel = 1;

    float scale = 2f;     // scale UI to
    float startX = 20f;   // vị trí X gốc
    float startY = 40f;   // vị trí Y bắt đầu
    float btnWidth = 200f;
    float btnHeight = 30f;
    float spacing = 10f;  // khoảng cách giữa các nút

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            showMenu = !showMenu;
    }

    void OnGUI()
    {
        if (!showMenu) return;

        // Scale toàn bộ UI
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));

        // Vẽ khung menu
        GUI.Box(new Rect(10, 10, 220,370), "Cheat Menu");

        float yPos = startY;

        // --- Vàng ---
        if (GUI.Button(new Rect(startX, yPos, btnWidth, btnHeight), "Thêm 100000 vàng"))
            Currency.Ins.AddCoin(100000);

        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, btnWidth, btnHeight), "Xóa hết vàng"))
            Currency.Ins.DeleteCoin();

        // --- Booster ---
        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, btnWidth, btnHeight), "Add 100 booster"))
        {
            Currency.Ins.AddBooster(BootersName.DestroyBlock, 100);
            Currency.Ins.AddBooster(BootersName.ChangeBlock, 100);
            Currency.Ins.AddBooster(BootersName.Rool, 100);
        }

        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, btnWidth, btnHeight), "Xóa hết booster"))
            Currency.Ins.DeleteBooters();

        // --- Level chọn ---
        yPos += btnHeight + spacing;
        GUI.Label(new Rect(startX, yPos, btnWidth, btnHeight), "Level: " + targetLevel);

        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, 40, btnHeight), "-"))
            targetLevel = Mathf.Max(1, targetLevel - 1);

        if (GUI.Button(new Rect(startX + 50, yPos, 40, btnHeight), "+"))
            targetLevel++;

        if (GUI.Button(new Rect(startX + 100, yPos, 100, btnHeight), "Go Level"))
        {
            Debug.Log("Jump to Level: " + targetLevel);
            GameManager.Ins.SetUpLevel(targetLevel);
            UIManager.Ins.GetUI<VictoryUI>().Close(1f);
            UIManager.Ins.GetUI <LoseUI>().Close(1f);
        }

        // --- Win / Lose ---
        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, btnWidth, btnHeight), "Win Level"))
            GameManager.Ins.Winlevel();

        if (GUI.Button(new Rect(startX, yPos += btnHeight + spacing, btnWidth, btnHeight), "Lose"))
        {
            UIManager.Ins.GetUI<LoseUI>().Open();
            UIManager.Ins.GetUI<GameplayUI>().Close(2f);
            GameManager.Ins.DestroyLever();
        }
    }
}
