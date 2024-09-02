namespace GlobalScripts.UpdateHandlerPattern
{
    public interface IFixedUpdateHandler
    {
        public bool IsActive { get; set; }
        public void OnFixedUpdate();
    }
}
