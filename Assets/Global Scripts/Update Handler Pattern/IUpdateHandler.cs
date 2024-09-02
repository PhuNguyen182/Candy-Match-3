namespace GlobalScripts.UpdateHandlerPattern
{
    public interface IUpdateHandler
    {
        public bool IsActive { get; set; }
        public void OnUpdate(float deltaTime);
    }
}
