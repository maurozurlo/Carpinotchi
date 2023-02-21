using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cashier
{
    public class Cashier_Box : MonoBehaviour
    {
        public ItemType type;

        public Material glow, dull;
        MeshRenderer mesh;
        Cashier_Manager manager;
        public bool selected;

        private void Start()
        {
            mesh = GetComponent<MeshRenderer>();
            dull = mesh.material;
            manager = Cashier_Manager.control;

        }

        public void Glow()
        {
            mesh.material = glow;
        }

        public void Dull()
        {
            mesh.material = dull;
        }

        private void OnMouseOver()
        {
            if (manager.selectedItemType != ItemType._)
            {
                Glow();
                selected = true;
            }
        }

		private void OnMouseExit()
		{
            Dull();
            selected = false;

        }


	}

}
