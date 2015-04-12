﻿using Finsa.CodeServices.Clock;
using Ninject.Modules;

namespace UnitTests
{
    /// <summary>
    ///   Bindings for KVLite.
    /// </summary>
    internal sealed class NinjectConfig : NinjectModule
    {
        public override void Load()
        {
            Bind<IClock>().To<MockClock>().InSingletonScope();
            //Bind<ICache>().To<PersistentCache>().WhenInjectedInto<PersistentCacheTests>();
            //Bind<ICache>().To<VolatileCache>().WhenInjectedInto<VolatileCacheTests>();
        }
    }
}