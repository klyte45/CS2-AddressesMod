using Belzont.Interfaces;
using Belzont.Utils;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI.Localization;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    [SettingsUIShowGroupName(kLogSection, kAlertModalsSection, kKeybindingTab, kToolControlsSection, kRoadNameLabelsSection)]
    [SettingsUIMouseAction(kActionApplyMouse, ActionType.Button, RebindOptions.All, ModifierOptions.Allow, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIMouseAction(kActionCancelMouse, ActionType.Button, RebindOptions.All, ModifierOptions.Allow, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIMouseAction(kActionToggleMembership, ActionType.Button, RebindOptions.All, ModifierOptions.Allow, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIKeyboardAction(kActionEnableRoadGrouping, ActionType.Button, "K45_ADR.Tool")]
    public class AdrModData : BasicModData
    {
        public const string kKeybindingTab = "Keybinding";
        public const string kToolControlsSection = "ToolControls";
        public const string kLabelsTab = "Labels";
        public const string kRoadNameLabelsSection = "RoadNameLabels";

        public const string kActionApplyMouse = "K45_ADR_RoadGroupingApply";
        public const string kActionCancelMouse = "K45_ADR_RoadGroupingCancel";
        public const string kActionToggleMembership = "K45_ADR_RoadGroupingToggleMembership";
        public const string kActionEnableRoadGrouping = "K45_ADR_EnableRoadGrouping";

        /// <summary>Slider index 0..4 → half-height = 2^(6+index)/10. Index 4 (102.4) = automatic (extents).</summary>
        public const int RoadNameLabelRefHalfHeightIndexMin = 0;
        public const int RoadNameLabelRefHalfHeightIndexMax = 4;
        public const int RoadNameLabelRefHalfHeightIndexAutomatic = 4;
        public const int RoadNameLabelRefHalfHeightExponentBase = 6;

        private int roadNameLabelRefHalfHeightIndex = 1;

        public AdrModData(IMod mod) : base(mod)
        {
        }

        public override void OnSetDefaults()
        {
            roadNameLabelRefHalfHeightIndex = 1;
        }

        [SettingsUISection(kKeybindingTab, kToolControlsSection)]
        [SettingsUIKeyboardBinding(BindingKeyboard.A, kActionEnableRoadGrouping, ctrl: true, shift: true)]
        public ProxyBinding EnableRoadGroupingTool { get; set; }

        [SettingsUISection(kKeybindingTab, kToolControlsSection)]
        [SettingsUIMouseBinding(BindingMouse.Left, kActionApplyMouse)]
        public ProxyBinding RoadGroupingApply { get; set; }

        [SettingsUISection(kKeybindingTab, kToolControlsSection)]
        [SettingsUIMouseBinding(BindingMouse.Right, kActionCancelMouse)]
        public ProxyBinding RoadGroupingCancel { get; set; }

        [SettingsUISection(kKeybindingTab, kToolControlsSection)]
        [SettingsUIMouseBinding(BindingMouse.Left, kActionToggleMembership, ctrl: true)]
        public ProxyBinding RoadGroupingToggleMembership { get; set; }

        /// <summary>
        /// Linear slider index. Maps to RefHalfHeight = 2^(6+index)/10 (6.4 … 102.4).
        /// Max index uses the old per-label extents formula (automatic).
        /// </summary>
        [SettingsUISection(kLabelsTab, kRoadNameLabelsSection)]
        [SettingsUISlider(
            min = RoadNameLabelRefHalfHeightIndexMin,
            max = RoadNameLabelRefHalfHeightIndexMax,
            step = 1f,
            unit = "integer",
            updateOnDragEnd = true)]
        [SettingsUIDisplayName(typeof(AdrModData), nameof(GetRoadNameLabelRefHalfHeightDisplayName))]
        public int RoadNameLabelRefHalfHeightIndex
        {
            get => roadNameLabelRefHalfHeightIndex;
            set
            {
                int clamped = math.clamp(value, RoadNameLabelRefHalfHeightIndexMin, RoadNameLabelRefHalfHeightIndexMax);
                if (roadNameLabelRefHalfHeightIndex == clamped)
                {
                    return;
                }

                roadNameLabelRefHalfHeightIndex = clamped;
                NotifyRoadNameLabelRefHalfHeightChanged();
            }
        }

        public LocalizedString GetRoadNameLabelRefHalfHeightDisplayName()
        {
            string baseLabel = LocalizationExtensions.Translate(GetOptionLabelLocaleID(nameof(RoadNameLabelRefHalfHeightIndex)));
            return LocalizedString.Value($"{baseLabel}: {FormatRefHalfHeightIndex(roadNameLabelRefHalfHeightIndex)}");
        }

        public static bool IsAutomaticRefHalfHeight(int index)
            => index >= RoadNameLabelRefHalfHeightIndexAutomatic;

        /// <summary>RefHalfHeight for fixed steps: 2^(6+index)/10. Caller must not use this for automatic.</summary>
        public static float IndexToRefHalfHeight(int index)
            => math.exp2(RoadNameLabelRefHalfHeightExponentBase + index) / 10f;

        public string FormatRefHalfHeightIndex(int index)
        {
            return index switch
            {
                0 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight0")),
                1 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight1")),
                2 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight2")),
                3 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight3")),
                _ => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeightAutomatic"))
            };
        }

        private void NotifyRoadNameLabelRefHalfHeightChanged()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            var main = world.GetExistingSystemManaged<AdrMainSystem>();
            main?.OnChangedRoadNameGenerationRules();
        }
    }
}
