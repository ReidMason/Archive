namespace NoxCore.Helm
{
    public class IdleBehaviour : SteeringBehaviour
    {
        void Reset()
        {
            Label = "IDLE";
            SequenceID = 0;
            Weight = 1000;
        }
    }
}