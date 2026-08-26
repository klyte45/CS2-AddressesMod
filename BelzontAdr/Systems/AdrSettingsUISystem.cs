using Game.UI;
using Game.UI.InGame;

namespace BelzontAdr
{
    public partial class AdrSettingsUISystem : UISystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            World.GetOrCreateSystemManaged<GamePanelUISystem>()
                .SetDefaultArgs(new UI.AdrSettingsPanel());
        }
    }
}
