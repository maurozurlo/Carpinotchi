using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    new public string name;
    public string description;
    public int id;
    public int price;
    public Sprite sprite;
}