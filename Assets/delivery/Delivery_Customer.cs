using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delivery {
    public class Delivery_Customer : MonoBehaviour {
        int index;
        [SerializeField]
        int points = 1;

        bool isReceivingPackage;
        int secsReceivingPackage;
        public int staySeconds = 3;
        public float timeout = 6;
        bool isDone;
        Animator animator;
        Delivery_Manager manager;

        // Test
        public float turnTime = 1.0f;
        public float walkTime = 1.0f;
        public float walkDistance = 10.0f;
        private Quaternion startRotation;
        private Quaternion endRotation;
        private Vector3 startPosition;
        private Vector3 endPosition;

        // Randomizing
        public bool disableRandom;
        SkinnedMeshRenderer mesh;
        public Color[] shirtColors;
        public Color[] pantColors;
        public Color[] shoeColors;
        public Texture2D[] faceTextures;

        public void SetValues(int _index, int _points = 1, int _staySeconds = 3)
        {
            points = _points;
            staySeconds = _staySeconds;
            index = _index;
        }

        private void Awake() {
            animator = GetComponentInChildren<Animator>();
        }

		private void Start()
		{
            manager = Delivery_Manager.control;

            if(!disableRandom) Randomize();
        }

        void Randomize()
        {
            mesh = GetComponentInChildren<SkinnedMeshRenderer>();

            foreach (Material material in mesh.materials)
            {
                if (material.name.Contains("Shirt"))
                {
                    material.color = GetRandomColor(shirtColors);
                }
                if (material.name.Contains("Pants"))
                {
                    material.color = GetRandomColor(pantColors);
                }
                if (material.name.Contains("Shoes"))
                {
                    material.color = GetRandomColor(shoeColors);
                }
                if (material.name.Contains("CustomerFace"))
                {
                    material.mainTexture = faceTextures[Random.Range(0, faceTextures.Length)];
                }
            }
        }


        Color GetRandomColor(Color[] colorList) {
            return colorList[Random.Range(0, colorList.Length)];
        }

		private void OnTriggerEnter(Collider collider) {
            // Check if tag == player
            if (manager.gameState != Delivery_Manager.GameState.Playing)
            {
                StopAllCoroutines();
                return;
            }
            
            if (!collider.CompareTag("Player")) return;
            if (isDone) return;
            isReceivingPackage = true;
            StartCoroutine("WaitTillPackageIsDelivered");
        }

        IEnumerator WaitTillPackageIsDelivered() {
            manager.ShowStaySign(staySeconds - secsReceivingPackage);
            yield return new WaitForSeconds(1);
            if (manager.gameState != Delivery_Manager.GameState.Playing){
                StopAllCoroutines();
                isReceivingPackage = false;
                yield return null;
            }

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
            if (manager.gameState == Delivery_Manager.GameState.Ended) return;
            if (isDone) return;
            if (!collider.CompareTag("Player")) return;
            isReceivingPackage = false;
            secsReceivingPackage = 0;
            Delivery_Manager.control.HideSign();
            StopAllCoroutines();
        }

        void Done() {
            isDone = true;
            animator.SetTrigger("Cheer");
            Delivery_Manager.control.AddToPackagesDelivered(index, points);
            TurnAroundAndWalk();
        }

        public void TurnAroundAndWalk()
        {
            startRotation = transform.rotation;
            endRotation = startRotation * Quaternion.Euler(0, 180, 0);
            StartCoroutine(Turn());
        }

        IEnumerator Turn()
        {
            float fracComplete = 0;
            while (fracComplete <= 1)
            {
                fracComplete += Time.deltaTime / turnTime;
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, fracComplete);
                yield return null;
            }
            startPosition = transform.position;
            endPosition = transform.position + transform.forward * walkDistance;
            StartCoroutine(Walk());
            animator.SetTrigger("Run");
            Destroy(gameObject, 6);
		}

		IEnumerator Walk()
		{
			float fracComplete = 0;
			while (fracComplete <= 1)
			{
				fracComplete += Time.deltaTime / walkTime;
				transform.position = Vector3.Lerp(startPosition, endPosition, fracComplete);
				yield return null;
			}
		}
	}

}