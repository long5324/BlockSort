using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO; // để dùng File IO
using UnityEngine;
[System.Serializable]
public enum StateCurrency
{
    coin,
    gem,
}
[System.Serializable]
public struct LevelReward
{
    public StateCurrency ChooseCurrency;
    public int NumberItem;
}
[System.Serializable]
public class InfoCurrency
{
    public int coin;
    public int gem;
}

public class Currency : Singleton<Currency>
{
    public InfoCurrency DataCurrency;

    private string savePath;

    [Header("Coin Change")]
    [SerializeField] int NumberCoinAdd; 
    [Button(ButtonSizes.Large)]
    public void AddCoin()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");

        LoadData();
        SetDataCoin(NumberCoinAdd);
    }
    [Button(ButtonSizes.Large)]
    public void DeleteCoin()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");

        LoadData();
        SetDataCoin(0);

    }
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "currency.json");

        LoadData(); 
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

    public void SaveData()
    {
        string json = JsonUtility.ToJson(DataCurrency, true); 
        File.WriteAllText(savePath, json);
        Debug.Log("Đã lưu: " + savePath);
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            DataCurrency = JsonUtility.FromJson<InfoCurrency>(json);
        }
        else
        {
            DataCurrency = new InfoCurrency();
            SaveData();
        }
    }
    
}
