using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MachineLearning
{
    public class HummingbirdAgent : Agent
    {
        [Tooltip("Force to apply when moving")]
        public float moveForce = 2f;

        [Tooltip("Speed to pitch up or down")] 
        public float pitchSpeed = 100f;
            
        [Tooltip("Speed to rotate around the up axis")]
        public float yawSpeed = 100f;

        [Tooltip("Transform at the tip of the beak")]
        public Transform beakTip;

        [Tooltip("The agent's camera")] 
        public Camera agentCamera;

        [Tooltip("Whether this is training mode or gameplay mode")]
        public bool trainingMode;

        private new Rigidbody _rigidbody;

        private FlowerArea _flowerArea;

        private Flower _nearestFlower;

        private float _smoothPitchChange = 0f;
        private float _smoothYawChange = 0f;

        private const float MaxPitchAngle = 80f;
        private const float BeakTipRadius = 0.008f;

        private bool _frozen;
        
        public float NectarObtained { get; private set; }

        public override void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _flowerArea = GetComponentInParent<FlowerArea>();

            if (!trainingMode) MaxStep = 0;
        }

        public override void OnEpisodeBegin()
        {
            if (trainingMode) {
                _flowerArea.ResetFlowers();
            }

            NectarObtained = 0f;
            
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            bool inFrontOfFlower = true;
            if (trainingMode) inFrontOfFlower = Random.value > .5f;

            MoveToSafeRandomPosition(inFrontOfFlower);
            UpdateNearestFlower();
        }

        private void MoveToSafeRandomPosition(bool inFrontOfFlower)
        {
            bool safePositionFound = false;
            int attemptRemaining = 100;
            Vector3 potentialPosition = Vector3.zero;
            Quaternion potentialRotation = new Quaternion();
            while (!safePositionFound && attemptRemaining > 0)
            {
                attemptRemaining--;
                if (inFrontOfFlower)
                {
                    Flower randomFlower = _flowerArea.Flowers[Random.Range(0, _flowerArea.Flowers.Count)];

                    float distanceFromFlower = Random.Range(.1f, .2f);
                    potentialPosition = randomFlower.transform.position +
                                        randomFlower.FlowerUpVector * distanceFromFlower;
                   
                    // Point beak at flower (bird's head is center of transform)
                    Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                    potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
                }
                else
                {
                    float height = Random.Range(1.2f, 2.5f);
                    float radius = Random.Range(2f, 7f);
                    Quaternion direction = quaternion.Euler(0f, Random.Range(-180f, 180f),0f);
                    potentialPosition = _flowerArea.transform.position + Vector3.up * height +
                                        direction * Vector3.forward * radius;

                    float pitch = Random.Range(-60f, 60f);
                    float yaw = Random.Range(-180f, 180f);
                    potentialRotation = Quaternion.Euler(pitch,yaw,0f);
                }
                
                // Check if the agent will collide with anything
                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

                safePositionFound = colliders.Length == 0;
            }
            Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

            transform.position = potentialPosition;
            transform.rotation = potentialRotation;
        }
        
        private void UpdateNearestFlower() {
            foreach (var flower in _flowerArea.Flowers) {
                if (_nearestFlower == null && flower.HasNectar) _nearestFlower = flower;
                else if(flower.HasNectar) {
                    float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                    float distanceToCurrentNearestFlower = Vector3.Distance(_nearestFlower.transform.position, beakTip.position);
                    if (!_nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower) _nearestFlower = flower;
                }
            }
        }
    }
}