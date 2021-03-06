﻿using MyCouch.IntegrationTests.TestFixtures;
using MyCouch.Testing;
using Xunit;

namespace MyCouch.IntegrationTests.ClientTests
{
    public abstract class ClientTestsOf<T>
        : TestsOf<T>, IUseFixture<ClientFixture> where T : class
    {
        protected IClient Client { get; set; }

        protected abstract void OnTestInit();

        public void SetFixture(ClientFixture data)
        {
            Client = data.Client;
            OnTestInit();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!(this is IPreserveStatePerFixture))
                Client.ClearAllDocuments();
        }
    }
}