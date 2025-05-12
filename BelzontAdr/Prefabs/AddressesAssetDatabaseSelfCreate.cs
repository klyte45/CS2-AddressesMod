#if !RELEASE
#define LOCAL
#endif
using AddressesCS2.Prefabs;
using BelzontCommons.Assets;
using Colossal.IO.AssetDatabase;

namespace BelzontAdr
{
    internal class AddressesAssetDatabaseSelfCreate : AssetDatabaseSelfCreate
    {
        private AssetDatabase<K45AddressesDatabase> _assetDatabase;
        public override IAssetDatabase CreateDatabase() => _assetDatabase = AssetDatabase<K45AddressesDatabase>.GetInstance(new K45AddressesDatabase());
        public override void SelfPopulate()
        {
            SelfPopulate(_assetDatabase);
        }
    }
}
