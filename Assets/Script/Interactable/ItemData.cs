using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int width = 1;
    public int height = 1;
    public float weight = 1f;
    public int maxStackSize = 1;
    public ItemType itemType;
    public GameObject worldPrefab;
    public bool isUsable = false;
    public float useTime = 0f;
}

public enum ItemType
{
    Resource,
    Weapon,
    Equipment,
    Consumable,
    Misc
}