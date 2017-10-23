using UnityEngine;

[System.Serializable]
public class LikeData
{
    public string name; // いいねの種類
    public Color32 color; // いいねの色

    public LikeData(string _name, Color32 _col)
    {
        name = _name;
        color = _col;
    }

}