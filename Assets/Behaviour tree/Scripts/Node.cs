using System.Collections.Generic;

namespace Behaviour_tree.Scripts {
    public class Node {
        public enum Status { Success, Running, Failure }
        public Status CurrentStatus;

        public List<Node> children = new List<Node>();
        public int CurrentChild;
        public string vName;
        public Node(){}
        public Node(string pName) => vName = pName;

        public virtual Status Process()
        {
            return children[CurrentChild].Process();
        }

        public void AddChild(Node child) {
            children.Add(child);
        }
    }
}
