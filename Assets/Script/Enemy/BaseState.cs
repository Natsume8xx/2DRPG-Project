public abstract class BaseState 
{
    protected Enemy currentEnemy;
    public abstract void OnEnter(Enemy enemy);
    public abstract void LogicUpdated();
    public abstract void PhysicsUpdated();
    public abstract void OnExit();
}
