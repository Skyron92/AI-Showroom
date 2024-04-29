namespace Behaviour_tree.Scripts
{
    public class Selector : Node {
      
        public Selector(string pName) {
            vName = pName;
        }

        public override Status Process()
        {
            Status childstatus = children[CurrentChild].Process();
            if (childstatus == Status.Running) return Status.Running;
            if (childstatus == Status.Success) {
                CurrentChild = 0;
                return Status.Success;
            }

            CurrentChild++;
            if (CurrentChild >= children.Count) {
                CurrentChild = 0;
                return Status.Failure;
            }

            return Status.Running;
        }
    }
}