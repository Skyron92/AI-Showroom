using System.Collections.Generic;
using UnityEngine;

namespace Behaviour_tree.Scripts
{
    public class BehaviourTree : Node {

        public BehaviourTree() => vName = "Tree";
        public BehaviourTree(string pVName) => vName = pVName;
        
        struct NodeLevel {
            public int level;
            public Node node;
        }

        public override Status Process()
        {
            Debug.Log(children[CurrentChild]);
            return children[CurrentChild].Process();
        }

        public void DebugTree() {
            string treePrintOut = "";
            Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
            Node currentNode = this;
            nodeStack.Push(new NodeLevel{level = 0, node = currentNode});

            while (nodeStack.Count != 0) {
                NodeLevel nextNode = nodeStack.Pop();
                treePrintOut += new string('-', nextNode.level) + nextNode.node.vName + "\n";
                for (int i = nextNode.node.children.Count - 1; i >= 0; i--) {
                    nodeStack.Push(new NodeLevel{level = nextNode.level + 1, node = nextNode.node.children[i]});
                }
            }
            
            Debug.Log(treePrintOut);
        }
    }
}
