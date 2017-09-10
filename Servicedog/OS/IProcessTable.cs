namespace Servicedog.OS
{
    public interface IProcessTable
    {
        IProcessInfo Get(int id);
    }
}