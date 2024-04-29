namespace Behaviour_tree.Scripts
{
    public class Sequence : Node
    {
        public Sequence(string pName)
        {
            vName = pName;
        }

        public override Status Process()
        {
            Status childStatus = children[CurrentChild].Process();
            if (childStatus == Status.Running) return Status.Running;
            if (childStatus == Status.Failure) return childStatus;

            CurrentChild++;
            if (CurrentChild >= children.Count) {
                CurrentChild = 0;
                return Status.Success;
            }
            
            return Status.Running;
        }
    }
}