using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

namespace Hummingbird {
    /// <summary>
    /// Manages a collection of flower plants and attached flowers
    /// </summary>
    public class FlowerArea : MonoBehaviour {
        
        //The diameter of the area where the agent and flowers can be used for observing relative distance from agent to flower
        public const float AreaDiameter = 20f;
        
        //The list of all the flower plants in the flower area
        private List<GameObject> _flowerPlants;
        
        //A lookup dictionary for looking up a flower from a nectar collider
        private Dictionary<Collider, Flower> _nectarFlowerDictionary;

        /// <summary>
        /// The list of all flowers in the flower area
        /// </summary>
        public List<Flower> Flowers { get; private set; }
        
        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, AreaDiameter);
        }

        /// <summary>
        /// Reset flowers and flower plants
        /// </summary>
        public void ResetFlowers() {
            //Rotate each flower plant around the Y axis and 
            foreach (GameObject flowerPlant in _flowerPlants) {
                float xRotation = UnityEngine.Random.Range(-5f, 5f);
                float yRotation = UnityEngine.Random.Range(-180f, 180f);
                float zRotation = UnityEngine.Random.Range(-5f, 5f);
                flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            }
            
            //Reset each flower
            foreach (Flower flower in Flowers) {
                flower.ResetFlower();
            }
        }

        /// <summary>
        /// Gets the <see cref="Flower"/> that a nectar collider belong to
        /// </summary>
        /// <param name="collider">The nectar collider</param>
        /// <returns>The matching flower</returns>
        public Flower GetFlowerFromNectar(Collider collider) {
            return _nectarFlowerDictionary[collider];
        }

        private void Awake() {
            //Initialize variables
            _flowerPlants = new List<GameObject>();
            _nectarFlowerDictionary = new Dictionary<Collider, Flower>();
            Flowers = new List<Flower>();
        }

        private void Start() {
            // Find all flowers that are children of this GameObject/Transform
            FindChildFlowers(transform);
        }
        
        /// <summary>
        /// Recursively find all flowers and flower plants that are children of a transform parent
        /// </summary>
        /// <param name="parent">The parennt of the children to check</param>
        private void FindChildFlowers(Transform parent) {
            for (int i = 0; i < parent.childCount; i++) {
                Transform child = parent.GetChild(i);
                if (child.CompareTag("flower_plant")) {
                    //Found a flower plant, add it to the flowerPlants list
                    _flowerPlants.Add(child.gameObject);
                    
                    //Look for the flowers within the flower plant
                    FindChildFlowers(child);
                }
                else {
                    //Not a flower plant, look for a Flower component
                    Flower flower = child.GetComponent<Flower>();
                    if (flower != null) {
                        Flowers.Add(flower);
                        
                        _nectarFlowerDictionary.Add(flower.nectarCollider, flower);
                    }
                    else {
                        FindChildFlowers(child);
                    }
                }
            }
        }
    }
}
