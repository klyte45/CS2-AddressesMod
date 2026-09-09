using Belzont.Interfaces;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;

namespace BelzontAdr
{
    [SettingsUIShowGroupName(kLogSection, kAlertModalsSection, kKeybindingSection, kToolControlsSection)]
    [SettingsUIMouseAction(kActionApplyMouse, ActionType.Button, RebindOptions.All, ModifierOptions.Allow, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIMouseAction(kActionCancelMouse, ActionType.Button, RebindOptions.All, ModifierOptions.Allow, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIKeyboardAction(kActionEnableRoadGrouping, ActionType.Button, "K45_ADR.Tool")]
    public class AdrModData : BasicModData
    {
        public const string kKeybindingSection = "Keybinding";
        public const string kToolControlsSection = "ToolControls";

        public const string kActionApplyMouse = "K45_ADR_RoadGroupingApply";
        public const string kActionCancelMouse = "K45_ADR_RoadGroupingCancel";
        public const string kActionEnableRoadGrouping = "K45_ADR_EnableRoadGrouping";

        public AdrModData(IMod mod) : base(mod)
        {
        }

        public override void OnSetDefaults()
        {
        }

        [SettingsUISection(kKeybindingSection, kToolControlsSection)]
        [SettingsUIKeyboardBinding(BindingKeyboard.A, kActionEnableRoadGrouping, ctrl: true, shift: true)]
        public ProxyBinding EnableRoadGroupingTool { get; set; }

        [SettingsUISection(kKeybindingSection, kToolControlsSection)]
        [SettingsUIMouseBinding(BindingMouse.Left, kActionApplyMouse)]
        public ProxyBinding RoadGroupingApply { get; set; }

        [SettingsUISection(kKeybindingSection, kToolControlsSection)]
        [SettingsUIMouseBinding(BindingMouse.Right, kActionCancelMouse)]
        public ProxyBinding RoadGroupingCancel { get; set; }
    }
}
