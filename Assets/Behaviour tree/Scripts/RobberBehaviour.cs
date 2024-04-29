using UnityEngine;
using UnityEngine.AI;

namespace Behaviour_tree.Scripts
{
    public class RobberBehaviour : MonoBehaviour
    {
        private BehaviourTree _vTree;
        private NavMeshAgent _agent;
        public GameObject van;
        public GameObject diamond;
        public GameObject backDoor;
        public GameObject frontDoor;

        [Range(0, 1000)] public int money = 800;

        private enum ActionState { IDLE, WALKING }

        private ActionState _state = ActionState.IDLE;
        
        private Node.Status _treeStatus = Node.Status.Running;

        private BehaviourTree vTree {
            get => _vTree;
            set => _vTree = value;
        }

        private Sequence _vEat;
        private Node _vPizza;
        private Node _vBuy;

        private Sequence vEat {
            get => _vEat;
            set => _vEat = value;
        }
        
        private Node vPizza {
            get => _vPizza;
            set => _vPizza = value;
        }
        
        private Node vBuy{
            get => _vBuy;
            set => _vBuy = value;
        }
        
        private void Start() {
            _agent = GetComponent<NavMeshAgent>();
            vTree = new BehaviourTree();
            Sequence vSteal = new Sequence("Steal something");
            Leaf hasGotMoney = new Leaf("Has got money", HasMoney);
            Leaf vGoToFrontDoor = new Leaf("   Go to front door", GoToFrontDoor);
            Leaf vGoToBackDoor = new Leaf("   Go to back door", GoToBackDoor);
            Leaf vGoToDiamond = new Leaf("   Go to diamond", GoToDiamond);
            Leaf vGoToVan = new Leaf("   Go to van", GoToVan);
            Selector openDoor = new Selector("Open door");
            
            openDoor.AddChild(vGoToBackDoor);
            openDoor.AddChild(vGoToFrontDoor);
            
            vSteal.AddChild(hasGotMoney);
            vSteal.AddChild(openDoor);
            vSteal.AddChild(vGoToDiamond);
            vSteal.AddChild(vGoToVan);
            vTree.AddChild(vSteal);

            vEat = new Sequence("   Eat something");
            vPizza = new Node("    Go to pizza shop");
            vBuy = new Node("    Buy pizza");
            
            vEat.AddChild(vPizza);
            vEat.AddChild(vBuy);
            vTree.AddChild(vEat);
            
            vTree.DebugTree();
        }

        private Node.Status GoToFrontDoor() => GoToDoor(frontDoor);
        private Node.Status GoToBackDoor() => GoToDoor(backDoor);
        private Node.Status HasMoney()
        {
            if (money >= 500) return Node.Status.Failure;
            return Node.Status.Success;
        }

        private Node.Status GoToDiamond() {
           Node.Status status = GoToLocation(diamond.transform.position);
           if (status == Node.Status.Success) diamond.transform.parent = transform;
           return status;
        }
        private Node.Status GoToVan() {
            Node.Status status = GoToLocation(van.transform.position);
            if (status == Node.Status.Success) {
                money += 300;
                diamond.SetActive(false);
            }
            return status;
        }
        private Node.Status GoToDoor(GameObject door) { 
            Node.Status status = GoToLocation(door.transform.position);
            if (status == Node.Status.Success) {
                if (door.GetComponent<Lock>().isLocked) return Node.Status.Failure;
                door.SetActive(false);
                return Node.Status.Success;
            }
            else {
                return status;
            }
        } 

        Node.Status GoToLocation(Vector3 destination) {
            var distanceToTarget = Vector3.Distance(destination, transform.position);
            if (_state == ActionState.IDLE)
            {
                _agent.SetDestination(destination);
                _state = ActionState.WALKING;
            }
            else if(Vector3.Distance(_agent.pathEndPosition, destination) >= 2) {
                _state = ActionState.IDLE;
                return Node.Status.Failure;
            }
            else if (distanceToTarget < 2) {
                _state = ActionState.IDLE;
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }

        private void Update()
        {
            if(_treeStatus != Node.Status.Success) _treeStatus = _vTree.Process();
        }
    }
}