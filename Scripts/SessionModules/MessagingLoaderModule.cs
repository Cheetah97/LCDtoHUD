namespace EemRdx.SessionModules
{
    public class MessagingLoaderModule : SessionModuleBase<ISessionKernel>, InitializableModule, UnloadableModule
    {
        public MessagingLoaderModule(ISessionKernel MySessionKernel) : base(MySessionKernel) { }
        protected override string DebugModuleName { get; } = "MessagingLoaderModule";

        public void Init()
        {
            Networking.Messaging.Register();
        }

        public void UnloadData()
        {
            Networking.Messaging.Unregister();
        }
    }
}
