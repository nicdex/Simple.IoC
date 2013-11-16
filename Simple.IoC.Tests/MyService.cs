namespace Simple.IoC.Tests
{
    internal class MyService
    {
    }

    internal interface IMyInterface
    {
        void F();
    }

    internal class MyServiceWithInterface : IMyInterface
    {
        public void F()
        {
        }
    }
}
