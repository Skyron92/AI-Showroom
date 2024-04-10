using UnityEngine;

namespace Behaviour_tree.Scripts
{
    public class RobberBehaviour : MonoBehaviour
    {
        private BehaviourTree _vTree;

        private BehaviourTree vTree {
            get => _vTree;
            set => _vTree = value;
        }
        private Node _vSteal;

        private Node vSteal {
            get => _vSteal;
            set => _vSteal = value;
        }
        private Node _vGoToDiamond;
        
        private Node vGoToDiamond {
            get => _vGoToDiamond;
            set => _vGoToDiamond = value;
        }
        private Node _vGoToVan;
        
        private Node vGoToVan {
            get => _vGoToVan;
            set => _vGoToVan = value;
        }

        private Node _vEat;
        private Node _vPizza;
        private Node _vBuy;

        private Node vEat {
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
            vTree = new BehaviourTree();
            vSteal = new Node("Steal something");
            vGoToDiamond = new Node("    Go to diamond");
            vGoToVan = new Node("    Go to van");
            
            vSteal.AddChild(_vGoToDiamond);
            vSteal.AddChild(_vGoToVan);
            vTree.AddChild(_vSteal);

            vEat = new Node("Eat something");
            vPizza = new Node("    Go to pizza shop");
            vBuy = new Node("    Buy pizza");
            
            vEat.AddChild(vPizza);
            vEat.AddChild(vBuy);
            vTree.AddChild(vEat);
            
            vTree.DebugTree();
        }
    }
}