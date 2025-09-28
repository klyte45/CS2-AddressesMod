using Belzont.Interfaces;
using Colossal.IO.AssetDatabase;
using Colossal.PSI.Common;
using System;
using System.IO;

namespace AddressesCS2.Prefabs
{
    internal struct K45AddressesDatabase : IAssetDatabaseDescriptor<K45AddressesDatabase>, IEquatable<K45AddressesDatabase>
    {
        public readonly bool canWriteSettings =>
#if DEBUG
            true;
#else
            false;
#endif

        public readonly string name => "AddressesMod";

        public readonly IAssetFactory assetFactory => DefaultAssetFactory.instance;

        private readonly string EffectivePath => Path.Combine(BasicIMod.ModInstallFolder, "Prefabs", ".Content");

        public readonly IDataSourceProvider dataSourceProvider => new FileSystemDataSource(name, EffectivePath, assetFactory, 0L);

        public readonly DlcId dlcId => DlcId.Virtual;

        public readonly bool Equals(K45AddressesDatabase other) => true;

        public override readonly bool Equals(object obj) => obj is K45AddressesDatabase;

        public override int GetHashCode() => GetType().GetHashCode();

    }
}
