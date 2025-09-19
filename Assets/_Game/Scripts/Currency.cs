using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public enum StateCurrency
{
    coin,
    gem,
}

[System.Serializable]
public enum BootersName
{
    none,
    DestroyBlock,
    ChangeBlock,
    Rool
}

[System.Serializable]
public struct LevelReward
{
    public StateCurrency ChooseCurrency;
    public int NumberItem;
}

[System.Serializable]
public struct BootersInfo
{
    public BootersName Booters;
    public int NumberBooters;
}

[System.Serializable]
public class ItemManager
{
    public List<BootersInfo> BootersData = new List<BootersInfo>();
}

[System.Serializable]
public class InfoCurrency
{
    public int coin;
    public int gem;
}

// ====== SAVE DATA STRUCT ======
[System.Serializable]
public class SaveDataStruct
{
    public InfoCurrency currency;
    public ItemManager items;
}

public class Currency : Singleton<Currency>
{
    public InfoCurrency DataCurrency = new InfoCurrency();
    public ItemManager DataItem = new ItemManager();
    private string savePath;

    [Header("Coin Change")]
    [SerializeField] int NumberCoinAdd;
    [SerializeField] BootersInfo BootersAdd;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");
        LoadData();
    }

    // ----------------- COIN / GEM -----------------
    [Button(ButtonSizes.Large)]
    public void AddCoin()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");
        LoadData();
        AddCoin(NumberCoinAdd);

    }
    [Button(ButtonSizes.Large)]
    public void DeleteCoin()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");
        LoadData();
        SetDataCoin(0);
    }
    [Button(ButtonSizes.Large)]
    public void AddBooters()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");
        LoadData();
        AddBooster(BootersAdd.Booters, BootersAdd.NumberBooters);
    }
    public void SetDataCoin(int value)
    {
        DataCurrency.coin = value;
        SaveData();
    }

    public void AddCoin(int value)
    {
        DataCurrency.coin += value;
        SaveData();
    }

    public void AddGem(int value)
    {
        DataCurrency.gem += value;
        SaveData();
    }

    // ----------------- BOOSTERS -----------------
    public void AddBooster(BootersName name, int amount)
    {
        bool found = false;
        for (int i = 0; i < DataItem.BootersData.Count; i++)
        {
            if (DataItem.BootersData[i].Booters == name)
            {
                var info = DataItem.BootersData[i];
                info.NumberBooters += amount;
                DataItem.BootersData[i] = info;
                found = true;
                break;
            }
        }

        if (!found)
        {
            DataItem.BootersData.Add(new BootersInfo { Booters = name, NumberBooters = amount });
        }

        SaveData();
    }

    public int GetBooster(BootersName name)
    {
        foreach (var b in DataItem.BootersData)
        {
            if (b.Booters == name) return b.NumberBooters;
        }
        return 0;
    }
    
    public bool UseBooster(BootersName name)
    {
        for (int i = 0; i < DataItem.BootersData.Count; i++)
        {
            if (DataItem.BootersData[i].Booters == name && DataItem.BootersData[i].NumberBooters > 0)
            {
                var info = DataItem.BootersData[i];
                info.NumberBooters -= 1;
                DataItem.BootersData[i] = info;
                SaveData();
                return true;
            }
        }
        return false;
    }

    // ----------------- SAVE / LOAD -----------------
    public void SaveData()
    {
        SaveDataStruct save = new SaveDataStruct
        {
            currency = DataCurrency,
            items = DataItem
        };

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Đã lưu: " + savePath);
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveDataStruct save = JsonUtility.FromJson<SaveDataStruct>(json);

            DataCurrency = save.currency;
            DataItem = save.items;
        }
        else
        {
            DataCurrency = new InfoCurrency();
            DataItem = new ItemManager { BootersData = new List<BootersInfo>() };
            SaveData();
        }
    }
}
