using Belzont.Interfaces;

namespace BelzontAdr
{
    public class AdrModData : BasicModData
    {
#if THUNDERSTORE
        public AdrModData() : base() { }
#else
        public AdrModData(IMod mod) : base(mod)
        {
        }
#endif

        public override void OnSetDefaults()
        {
        }
    }
}
