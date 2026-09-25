namespace Hauntscope.Core.StateMachines
{
    public interface IState
    {
        void Enter();

        void Exit();

        void Tick(float deltaTime);
    }
}
