namespace Behaviour_tree.Scripts
{
    public class Leaf : Node
    {
        public delegate Status Tick();
        public Tick ProcessMethod;
        
        public Leaf(){}

        public Leaf(string pName, Tick processMethod)
        {
            vName = pName;
            ProcessMethod = processMethod;
        }

        public override Status Process()
        {
            return ProcessMethod != null ? ProcessMethod() : Status.Failure;
        }
    }
}