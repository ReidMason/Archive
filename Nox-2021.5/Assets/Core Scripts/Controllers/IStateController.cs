namespace NoxCore.Controllers
{
    public interface IStateController
    {
        void processState();
        bool setState(string newState);
    }
}