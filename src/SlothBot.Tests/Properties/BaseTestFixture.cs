using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SlothBot.Tests.Properties
{

    [TestFixture]
    public abstract class BaseTestFixture<T>
    {
        private readonly List<IDisposable> resources = new List<IDisposable>();

        public T sut { get; protected set; }
        public abstract T SetupFixture();

        [SetUp]
        public void Setup()
        {
            sut = SetupFixture();
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
            Cleanup();
        }

        protected virtual void Cleanup()
        {
        }

        public void Dispose()
        {
            if (sut is IDisposable)
                (sut as IDisposable).Dispose();

            resources.ForEach(r => r.Dispose());
            resources.Clear();
            Cleanup();
        }

        public void OnDispose(IDisposable obj)
        {
            resources.Add(obj);
        }
    }
}
