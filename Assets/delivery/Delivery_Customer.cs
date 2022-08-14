using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delivery {
    public class Delivery_Customer : MonoBehaviour {
        [SerializeField]
        int points = 1;

        bool isReceivingPackage;
        int secsReceivingPackage;
        public int staySeconds = 3;
        bool isDone;

        private void OnTriggerEnter(Collider collider) {
            // Check if tag == player
            if (!collider.CompareTag("Player")) return;
            if (isDone) return;

            isReceivingPackage = true;
            StartCoroutine("WaitTillPackageIsDelivered");
        }

        IEnumerator WaitTillPackageIsDelivered() {
            Delivery_Manager.control.ShowStaySign(staySeconds - secsReceivingPackage);
            yield return new WaitForSeconds(1);
            if (!isReceivingPackage) yield return null;
            secsReceivingPackage += 1;

            if (secsReceivingPackage == staySeconds) {
                Done();
            }
            else {
                StartCoroutine("WaitTillPackageIsDelivered");
            }
        }

        private void OnTriggerExit(Collider collider) {
            if (isDone) return;
            if (!collider.CompareTag("Player")) return;
            isReceivingPackage = false;
            secsReceivingPackage = 0;
            Delivery_Manager.control.HideStaySign();
            StopAllCoroutines();
        }

        void Done() {
            isDone = true;
            Delivery_Manager.control.AddToPackagesDelivered(points);
            Kill();
        }

        public void Kill() {
            Destroy(gameObject);
        }
    }

}