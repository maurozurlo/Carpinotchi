using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    new public string name;
    [TextArea(3, 10)]
    public string description;
    public int id;
    public int price;
    public Sprite sprite;
}