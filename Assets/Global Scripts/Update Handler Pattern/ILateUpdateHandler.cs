namespace GlobalScripts.UpdateHandlerPattern
{
    public interface ILateUpdateHandler
    {
        public bool IsActive { get; set; }
        public void OnLateUpdate(float deltaTime);
    }
}
