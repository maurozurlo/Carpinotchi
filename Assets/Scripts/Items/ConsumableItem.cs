using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Consumable")]
public class ConsumableItem : Item
{
    public int sanity;
    public int hygiene;
    public int energy;
    public int hunger;

    public void Consume()
    {
        Pet.control.energy.ChangeValue(energy);
        Pet.control.sanity.ChangeValue(sanity);
        Pet.control.hygiene.ChangeValue(hygiene);
        Pet.control.hunger.ChangeValue(hunger);
    }
}

