using System;
using UnityEngine;


namespace Hummingbird {
    /// <summary>
    /// Manages a single flower with nectar
    /// </summary>
    public class Flower : MonoBehaviour {

        [Tooltip("The color when the flower is full"), SerializeField] private Color fullFlowerColor = new Color(1f, 0f, .3f);
        [Tooltip("The color when the flower is full"), SerializeField] private Color emptyFlowerColor = new Color(.5f, 0f, 1f);

        /// <summary>
        /// The trigger collider representing the nectar
        /// </summary>
        [HideInInspector] public Collider nectarCollider;

        /// <summary>
        /// The solid collider representing the flower petals
        /// </summary>
        private Collider _flowerCollider;

        private Material _flowerMaterial;

        public Vector3 FlowerUpVector => nectarCollider.transform.up;

        public Vector3 FlowerCenterPosition => nectarCollider.transform.position;
        
        public float NectarAmount { get; private set; }

        public bool HasNectar => NectarAmount > 0f;

        private void Awake() {
            nectarCollider = GetComponent<Collider>();
        }

        /// <summary>
        /// Attempts to remove nectar from the flower
        /// </summary>
        /// <param name="amount">The amount of nectar to remove</param>
        /// <returns>The actual amount successfully removed</returns>
        public float Feed(float amount) {
            float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);
            NectarAmount -= amount;
            if (HasNectar) {
                NectarAmount = 0f;
                
                _flowerCollider.gameObject.SetActive(false);
                nectarCollider.gameObject.SetActive(false);

                _flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
            }

            return nectarTaken;
        }
    }
}