using System;
using NUnit.Framework;

namespace Simple.IoC.Tests
{
    [TestFixture]
    public class ContainerFixture
    {
        [Test]
        public void When_I_resolve_a_service_that_is_not_registered_Then_an_InvalidOperationException_is_thrown()
        {
            var sut = new Container();
            var e = Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof(MyService)));
            Assert.That(e.Message, Is.EqualTo("Component for service of type Simple.IoC.Tests.MyService is not registered."));
        }

        [Test]
        public void When_I_resolve_T_a_service_that_is_not_registered_Then_an_InvalidOperationException_is_thrown()
        {
            var sut = new Container();
            var e = Assert.Throws<InvalidOperationException>(() => sut.Resolve<MyService>());
            Assert.That(e.Message, Is.EqualTo("Component for service of type Simple.IoC.Tests.MyService is not registered."));
        }

        [Test]
        public void When_I_register_a_service_with_no_interface_once_Then_service_is_resolvable()
        {
            var sut = new Container();
            sut.RegisterComponent(typeof(MyService));
            sut.Resolve(typeof (MyService));
        }

        [Test]
        public void When_I_register_T_a_service_with_no_interface_once_Then_service_is_resolvable()
        {
            var sut = new Container();
            sut.RegisterComponent<MyService>();
            sut.Resolve<MyService>();
        }

        [Test]
        public void When_I_register_a_service_with_no_interface_twice_Then_an_InvalidOperationException_is_thrown()
        {
            var sut = new Container();
            sut.RegisterComponent(typeof(MyService));
            var e = Assert.Throws<InvalidOperationException>(() => sut.RegisterComponent(typeof (MyService)));
            Assert.That(e.Message, Is.EqualTo("Component for service of type Simple.IoC.Tests.MyService is already registered."));
        }
        
        [Test]
        public void When_I_register_T_a_service_with_no_interface_twice_Then_an_InvalidOperationException_is_thrown()
        {
            var sut = new Container();
            sut.RegisterComponent<MyService>();
            var e = Assert.Throws<InvalidOperationException>(() => sut.RegisterComponent<MyService>());
            Assert.That(e.Message, Is.EqualTo("Component for service of type Simple.IoC.Tests.MyService is already registered."));
        }

        [Test]
        public void When_I_register_a_service_with_one_interface_once_Then_service_is_resolvable()
        {
            var sut = new Container();
            sut.RegisterComponent(typeof(IMyInterface), typeof(MyServiceWithInterface));
            sut.Resolve(typeof (IMyInterface));
        }
        
        [Test]
        public void When_I_register_T_a_service_with_once_interface_once_Then_service_is_resolvable()
        {
            var sut = new Container();
            sut.RegisterComponent<IMyInterface, MyServiceWithInterface>();
            sut.Resolve<IMyInterface>();
        }

        [Test]
        public void When_I_install_an_installer_Then_services_from_that_installer_are_resolvable()
        {
            var sut = new Container();
            sut.Install(new MyInstaller());
            sut.Resolve<IMyInterface>();
            sut.Resolve<MyService>();
            sut.Resolve<IInstaller>();
            sut.Resolve<ContainerFixture>();
        }

        [Test]
        public void When_I_install_all_from_all_assemblies_Then_all_services_are_resolvable()
        {
            var sut = new Container();
            sut.InstallFromAllAssemblies();
            //From Simple.IoC.Tests
            sut.Resolve<IMyInterface>();
            sut.Resolve<MyService>();
            sut.Resolve<IInstaller>();
            sut.Resolve<ContainerFixture>();
        }

        [Test]
        public void When_I_resolve_the_container_Then_it_returns_same_reference()
        {
            var sut = new Container();
            var container = sut.Resolve<IContainer>();
            Assert.That(container, Is.SameAs(sut));
        }
    }
}