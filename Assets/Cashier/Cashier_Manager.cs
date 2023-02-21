using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cashier
{
    public class Cashier_Manager : MonoBehaviour
    {
        public static Cashier_Manager control;

        public GameObject[] _targets;

        private GameObject selectedItem;
        public ItemType selectedItemType;

        List<Cashier_Box> targets = new List<Cashier_Box>();

        public int score;

        public void SetSelectedItem(GameObject go, ItemType itemType)
        {
            selectedItem = go;
            selectedItemType = itemType;
        }

        public void UnSelectItem()
        {
            selectedItem = null;
            selectedItemType = ItemType._;
        }

        private void Awake()
        {
            if (!control)
            {
                control = this;
            }

            foreach (GameObject target in _targets)
            {
                targets.Add(target.GetComponent<Cashier_Box>());
            }
        }

        public void CheckIfMatch()
        {
            foreach (Cashier_Box target in targets)
            {
                if (target.selected && target.type == selectedItemType)
                {
                    score++;
                    Debug.Log(score);
                }
            }

            Destroy(selectedItem);
            selectedItemType = ItemType._;
        }
    }
}
