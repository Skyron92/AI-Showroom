using System;
using UnityEngine;

namespace MachineLearning
{
    /// <summary>
    /// Manages a single flower with nectar
    /// </summary>
    public class Flower : MonoBehaviour
    {
        [Tooltip("The color when the flower is full")]
        public Color fullFlowerColor = new Color(1f,0f,.3f);
        
        [Tooltip("The color when the flower is empty")]
        public Color emptyFlowerColor = new Color(.4f,0f,1f);

        /// <summary>
        /// The trigger collider representing the nectar
        /// </summary>
        [HideInInspector]
        public Collider nectarCollider;
        
        // The solid collider representing the flower petals
        private Collider _flowerCollider;
        
        // The flower's material
        private Material _flowerMaterial;
        
        /// <summary>
        /// A vector pointing straight out of the flower
        /// </summary>
        public Vector3 FlowerUpVector => nectarCollider.transform.up;

        /// <summary>
        /// The center position of the nectar collider
        /// </summary>
        public Vector3 FlowerCenterPosition => nectarCollider.transform.position;
        /// <summary>
        /// The amount of nectar remaining in the flower
        /// </summary>
        public float NectarAmount { get; private set; }

        /// <summary>
        /// Whether the flower has any nectar remaining
        /// </summary>
        public bool HasNectar => NectarAmount > 0f;

        /// <summary>
        /// Attempts to remove nectar from the flower
        /// </summary>
        /// <param name="amount">The amount of nectar to remove</param>
        /// <returns>The actual amount successfully removed</returns>
        public float Feed(float amount)
        {
            float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);
            
            NectarAmount -= amount;

            if (NectarAmount <= 0) {
                NectarAmount = 0;
                _flowerCollider.gameObject.SetActive(false);
                nectarCollider.gameObject.SetActive(false);
                _flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
            }

            return nectarTaken;
        }
        
        public void ResetFlower()
        {
            NectarAmount = 1f;
            _flowerCollider.gameObject.SetActive(true);
            nectarCollider.gameObject.SetActive(true);
            _flowerMaterial.SetColor("_BaseColor", fullFlowerColor);
        }

        private void Awake()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            _flowerMaterial = meshRenderer.material;
            _flowerCollider = transform.Find("FlowerCollider").GetComponent<Collider>(); 
            nectarCollider = transform.Find("FlowerNectarCollider").GetComponent<Collider>();
            ResetFlower();
        }
    }
}
