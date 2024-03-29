using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

namespace Hummingbird
{
    /// <summary>
    /// A hummingbird Machine Learning Agent
    /// </summary>
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

        //The rigidbody of the agent
        private Rigidbody _rigidbody;
        
        //The flower area that the agent is in
        private FlowerArea _flowerArea;
        
        //The nearest flower to the agent
        private Flower _nearestFlower;
        
        //Allows for smoother pitch changes
        private float _smoothPitchChange = 0f;
        
        //Allows for smoother yaw changes
        private float _smoothYawChange = 0f;
        
        //Maximum angle that the bird can pitch up or down
        private const float MaxPitchAngle = 80f;
        
        //Maximum distance from the beak tip to accept nectar collision
        private const float BeakTipRadius = 0.008f;
        
        //Whether the agent is frozen (intentionally not flying;
        private bool _frozen = false;
        
        /// <summary>
        /// The amount of nectar the agent has obtained this episode
        /// </summary>
        public float NectarObtained { get; private set; }

        /// <summary>
        /// Initialize the agent
        /// </summary>
        public override void Initialize() {
            _rigidbody = GetComponent<Rigidbody>();
            _flowerArea = GetComponentInParent<FlowerArea>();
            
            // If not training mode, no max step, play forever
            if (!trainingMode) MaxStep = 0;
        }

        /// <summary>
        /// Reset the agent when an episode begins
        /// </summary>
        public override void OnEpisodeBegin() {
            if (trainingMode)
            {
                //Only reset flowers in training when there is one agent per area
                _flowerArea.ResetFlowers();
            }
            
            //Reset nectar obtained
            NectarObtained = 0f;
            
            //Zero out velocities so that movement stops before a new episode begins
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            //Default to spawning in front of a flower
            bool inFrontOfFlower = true;
            if (trainingMode)
            {
                // Spawn in front of flower 50% of the time during training
                inFrontOfFlower = UnityEngine.Random.value > .5f;
            }
            
            //Move the agent to a new random position
            MoveToSafeRandomPosition(inFrontOfFlower);
            
            //Recalculate the nearest flower now that the agent has moved
            UpdateNearestFlower();
        }

        /// <summary>
        /// Update the nearest flower to the agent
        /// </summary>
        private void UpdateNearestFlower() {
            foreach (Flower flower in _flowerArea.Flowers) {
                if (_nearestFlower == null && flower.HasNectar) {
                    // No current nearest flower and this flower has nectar, so set to this flower
                    _nearestFlower = flower;
                }
                else if(flower.HasNectar) {
                    // Calculate distance to this flower and distance to the current nearest flower
                    float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                    float distanceToCurrenntNearestFlower = Vector3.Distance(_nearestFlower.transform.position, beakTip.position);
                    
                    // If current nearest flower is empty OR  this flower is closer, update the nearest flower
                    if (!_nearestFlower.HasNectar || distanceToFlower < distanceToCurrenntNearestFlower) _nearestFlower = flower;
                }
            }
        }

        /// <summary>
        /// Move the agent to a safe random position (i.e does not collide with anything)
        /// If in frfont of flower, also point the beak at the flower
        /// </summary>
        /// <param name="inFrontOfFlower"></param>
        /// <exception cref="NotImplementedException">Whether to choose a spot in front of flower</exception>
        private void MoveToSafeRandomPosition(bool inFrontOfFlower)
        {
            bool safePositionFound = false;
            int attemptsRemaining = 100; // Prevent an infinite loop
            Vector3 potentialPosition = Vector3.zero;
            Quaternion potentialRotation = new Quaternion();
            
            //Loop until a safe position is found or we run out of attempts
            while (!safePositionFound && attemptsRemaining > 0)
            {
                attemptsRemaining--;
                if (inFrontOfFlower)
                {
                    // Pick a ranndom flower
                    Flower randomFlower = _flowerArea.Flowers[UnityEngine.Random.Range(0, _flowerArea.Flowers.Count)];
                    
                    // Position 10 to 20 cm in front of the flower
                    float distaceFromFlower = UnityEngine.Random.Range(.1f, .2f);
                    potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distaceFromFlower;
                    
                    //Point beak at flower (bird's head is center of transform)
                    Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                    potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
                }
                else
                {
                    // Pick a random height from the ground
                    float height = UnityEngine.Random.Range(1.2f, 2.5f);
                    
                    //Pick a random radius from the center of the area 
                    float radius = UnityEngine.Random.Range(2f, 7f);
                    
                    // Pick a random direction rotated around the y axis
                    Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
                    
                    // Combine height, radius and direction to pick a potential position
                    potentialPosition = _flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;
                    
                    // Choose and set ranbdom starting pitch and yaw
                    float pitch = UnityEngine.Random.Range(-60f, 60f);
                    float yaw = UnityEngine.Random.Range(-180f, 180f);
                    potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
                }
                
                // Check to see if the agent will collide with anything
                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);
                
                //Safe position has been found if no colliders are overlapped
                safePositionFound = colliders.Length == 0;
            }
            
            Debug.Assert(safePositionFound, "Could not find a safe position to spawn");
            
            // Set the position and rotation
            transform.position = potentialPosition;
            transform.rotation = potentialRotation;
        }

        /// <summary>
        /// Called when an action is received from either the player input or the neural network
        ///
        /// actions.ContinuousActions[i] represents :
        /// Index 0 : move vector x (+1 right, -1 left)
        /// Index 1 : move vector y (+1 up, -1 down)
        /// Index 2 : move vector z (+1 forward, -1 backward)
        /// Index 3 : pitch angle (+1 pitch up, -1 pitch down)
        /// Index 4 : yaw angle (+1 turn right, -1 turn left)
        /// </summary>
        /// <param name="actions">The actions to take</param>
        public override void OnActionReceived(ActionBuffers actions) {
            // Don't take actions if frozen
            if(_frozen) return;
            
            // Calculate movement vector
            Vector3 move = new Vector3(actions.ContinuousActions[0],actions.ContinuousActions[1],actions.ContinuousActions[2]);
            
            // Add force in the direction of the move vector
            _rigidbody.AddForce(move * moveForce);
            
            // Get the current rotation
            Vector3 rotationVector = transform.rotation.eulerAngles;
            
            // Calculate pitch and yaw rotation
            float pitchChange = actions.ContinuousActions[3];
            float yawChange = actions.ContinuousActions[4];
            
            // Calculate smooth rotation changes
            _smoothPitchChange = Mathf.MoveTowards(_smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
            _smoothYawChange = Mathf.MoveTowards(_smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
            
            // Calculate new pitch and yaw based on smoothed values
            // Clamp pitch to avoid flipping upside down
            float pitch = rotationVector.x + _smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
            if (pitch > 180f) pitch -= 360f;
            pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

            float yaw = rotationVector.y + _smoothYawChange * Time.fixedDeltaTime * yawSpeed;
            
            // Apply the new rotation
            transform.rotation = quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary>
        /// Collect vector observations from the environment
        /// </summary>
        /// <param name="sensor">The vector sensor</param>
        public override void CollectObservations(VectorSensor sensor) 
        {
            // If nearestFlower is null, observe an empty array and return early
            if (_nearestFlower == null) {
                sensor.AddObservation(new float[10]);
                return;
            }
            
            // Observe the agent's local rotation (4 observations)
            sensor.AddObservation(transform.localRotation.normalized);
            
            // Get a vector from the beak tip to the nearest flower
            Vector3 toFlower = _nearestFlower.FlowerCenterPosition - beakTip.position;
            
            // Observe a normalized vector pointing to the nearest flower (3 observations)
            sensor.AddObservation(toFlower.normalized);
            
            // Observe a dot product that indicates whether the beak tip is in front of the flower (1 observation)
            // (+1 means the beak tip is directly in front of the flower, -1 means directly behind
            sensor.AddObservation(Vector3.Dot(toFlower.normalized, -_nearestFlower.FlowerUpVector.normalized));
            
            // Observe a dot product that indicates whether the beak is pointing toward the flower (1 observation)
            // (+1 means the beak is pointing directly at the flower, -1 means directly away)
            sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -_nearestFlower.FlowerUpVector.normalized));
            
            // Observe the relative distance from the beak tip to the flower (1 observation)
            sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);
            
            // 10 total observations
        }

        /// <summary>
        /// When Behavior Type is set to "Heuristic Only" oin the agent's Behavior Parameters
        /// this function will be called. Its return values will be fed into
        /// <see cref="OnActionReceived"/> instead of using the neural network
        /// </summary>
        /// <param name="actionsOut">An output action</param>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Create placeholders for all movement/turning
            Vector3 forward = Vector3.zero;
            Vector3 left = Vector3.zero;
            Vector3 up = Vector3.zero;
            float pitch = 0f;
            float yaw = 0f;
            
            // Convert keyboard inputs to movement and turning
            // All values should be between -1 and +1
            
            //Forward / backward
            if (Input.GetKey(KeyCode.Z)) forward = transform.forward;
            else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;
            
            // Left / right
            if (Input.GetKey(KeyCode.Q)) left = -transform.right;
            else if (Input.GetKey(KeyCode.D)) left = transform.right;
            
            // Up / down
            if (Input.GetKey(KeyCode.E)) up = transform.up;
            else if (Input.GetKey(KeyCode.C)) up = -transform.up;
            
            // Pitch up/down
            if (Input.GetKey(KeyCode.UpArrow)) pitch = 1f;
            else if (Input.GetKey(KeyCode.DownArrow)) pitch = -1f;
            
            // Turn left/right
            if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;
            else if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f;
            
            // Combine the movement vectors and normalize
            Vector3 combined = (forward + left + up).normalized;
            
            // Add the 3 movement values, pitch and yaw to the actionOut.ContinuousActions array
            var continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = combined.x;
            continuousActions[1] = combined.y;
            continuousActions[2] = combined.z;
            continuousActions[3] = pitch;
            continuousActions[4] = yaw;
        }
    }
}
