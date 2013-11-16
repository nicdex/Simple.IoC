namespace Simple.IoC.Tests
{
    internal class MyInstaller : IInstaller
    {
        public void Install(IContainer container)
        {
            container.RegisterAllFromAssembly(GetType().Assembly);
        }
    }
}