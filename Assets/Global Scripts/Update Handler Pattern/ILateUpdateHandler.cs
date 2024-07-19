namespace GlobalScripts.UpdateHandlerPattern
{
    public interface ILateUpdateHandler
    {
        public void OnLateUpdate(float deltaTime);
    }
}
