using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MachineLearning
{
    /// <summary>
    /// Manages a collection of flower plants and attached flowers
    /// </summary>
    public class FlowerArea : MonoBehaviour
    {
        public const float AreaDiameter = 20f;

        private List<GameObject> _flowerPlants;

        private Dictionary<Collider, Flower> _nectarFlowerDictionary;
        
        public List<Flower> Flowers { get; private set; }

        public void ResetFlowers()
        {
            // Rotate each flower plant around the Y axis and subtly around X and Z
            foreach (var flowerPlant in _flowerPlants)
            {
                float xRotation = Random.Range(-5f, 5f);
                float yRotation = Random.Range(-180f, 180f);
                float zRotation = Random.Range(-5f, 5f);
                flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            }

            foreach (var flower in Flowers) {
                flower.ResetFlower();
            }
        }

        /// <summary>
        /// Get the <see cref="Flower"/> that a nectar collier belongs to
        /// </summary>
        /// <param name="collider">The nectar collider</param>
        /// <returns>The matching flower</returns>
        public Flower GetFlowerFromNectar(Collider collider)
        {
            return _nectarFlowerDictionary[collider];
        }

        private void Awake()
        {
            _flowerPlants = new List<GameObject>();
            _nectarFlowerDictionary = new Dictionary<Collider, Flower>();
            Flowers = new List<Flower>();
        }

        private void Start()
        {
            FindChildFlower(transform);
        }

        private void FindChildFlower(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.CompareTag("flower_plant")) {
                    _flowerPlants.Add(child.gameObject);
                    FindChildFlower(child);
                }
                else
                {
                    if (child.TryGetComponent(out Flower flower)) {
                        Flowers.Add(flower);
                        _nectarFlowerDictionary.Add(flower.nectarCollider, flower);
                    }
                    else {
                        FindChildFlower(child);
                    }
                }
            }
        }
    }
}