using Game.UI.InGame;

namespace BelzontAdr.UI
{
    /// <summary>
    /// Backend GamePanel token for the ADR settings UI host.
    /// Full name must match <c>AdrSettingsPanelId</c> in k45_adr_vuio.
    /// </summary>
    public class AdrSettingsPanel : GamePanel
    {
        public override bool blocking => true;

        public override LayoutPosition position => LayoutPosition.Center;
    }
}
