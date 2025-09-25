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
        GUI.Label(new Rect(startX, yPos, 50, btnHeight), "Level:");

        // TextField
        string inputLevel = GUI.TextField(
            new Rect(startX + 60, yPos, 60, btnHeight),
            targetLevel.ToString()
        );

        // Parse input
        if (int.TryParse(inputLevel, out int parsedLevel))
            targetLevel = Mathf.Max(1, parsedLevel);

        // Nút tăng/giảm và Go Level trên cùng 1 hàng
        if (GUI.Button(new Rect(startX + 130, yPos, 30, btnHeight), "-"))
            targetLevel = Mathf.Max(1, targetLevel - 1);

        if (GUI.Button(new Rect(startX + 165, yPos, 30, btnHeight), "+"))
            targetLevel++;

        if (GUI.Button(new Rect(startX, yPos + btnHeight + spacing, btnWidth, btnHeight), "Go Level"))
        {
            Debug.Log("Jump to Level: " + targetLevel);
            GameManager.Ins.SetUpLevel(targetLevel);
            UIManager.Ins.GetUI<VictoryUI>().Close(1f);
            UIManager.Ins.GetUI<LoseUI>().Close(1f);
        }

        // Sau khi xong cụm Level thì mới tăng yPos cho phần sau
        yPos += (btnHeight + spacing) ;


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
