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
    [SettingsUIShowGroupName(kLogSection, kAlertModalsSection, kToolControlsSection, kRoadNameLabelsSection)]
    // Tool mouse gestures are fixed (not rebindable); still registered for InputManager / tool usage.
    [SettingsUIMouseAction(kActionApplyMouse, ActionType.Button, RebindOptions.None, ModifierOptions.Allow, canBeEmpty: false, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIMouseAction(kActionCancelMouse, ActionType.Button, RebindOptions.None, ModifierOptions.Allow, canBeEmpty: false, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIMouseAction(kActionToggleMembership, ActionType.Button, RebindOptions.None, ModifierOptions.Allow, canBeEmpty: false, usages: new[] { "K45_ADR.Tool" })]
    [SettingsUIKeyboardAction(kActionEnableRoadGrouping, ActionType.Button, "K45_ADR.Tool")]
    public class AdrModData : BasicModData
    {
        public const string kGeneralTab = "General";
        public const string kToolControlsSection = "ToolControls";
        public const string kRoadNameLabelsSection = "RoadNameLabels";

        public const string kActionApplyMouse = "K45_ADR_RoadGroupingApply";
        public const string kActionCancelMouse = "K45_ADR_RoadGroupingCancel";
        public const string kActionToggleMembership = "K45_ADR_RoadGroupingToggleMembership";
        public const string kActionEnableRoadGrouping = "K45_ADR_EnableRoadGrouping";

        /// <summary>Slider index 0..4 → half-height = 2^(6+index)/10. Index 4 (102.4) = automatic (extents).</summary>
        public const int RoadNameLabelRefHalfHeightIndexMin = 0;
        public const int RoadNameLabelRefHalfHeightIndexMax = 4;
        public const int RoadNameLabelRefHalfHeightIndexAutomatic = 0;
        public const int RoadNameLabelRefHalfHeightExponentBase = 5;

        private int roadNameLabelRefHalfHeightIndex = 0;

        public AdrModData(IMod mod) : base(mod)
        {
        }

        public override void OnSetDefaults()
        {
            roadNameLabelRefHalfHeightIndex = 0;
        }

        [SettingsUISection(kGeneralTab, kToolControlsSection)]
        [SettingsUIKeyboardBinding(BindingKeyboard.A, kActionEnableRoadGrouping, ctrl: true, shift: true)]
        public ProxyBinding EnableRoadGroupingTool { get; set; }

        /// <summary>LMB select — fixed; hidden from options (RebindOptions.None).</summary>
        [SettingsUIHidden]
        [SettingsUIMouseBinding(BindingMouse.Left, kActionApplyMouse)]
        public ProxyBinding RoadGroupingApply { get; set; }

        /// <summary>RMB cancel — fixed; hidden from options (RebindOptions.None).</summary>
        [SettingsUIHidden]
        [SettingsUIMouseBinding(BindingMouse.Right, kActionCancelMouse)]
        public ProxyBinding RoadGroupingCancel { get; set; }

        /// <summary>Ctrl+LMB toggle membership — fixed; hidden from options (RebindOptions.None).</summary>
        [SettingsUIHidden]
        [SettingsUIMouseBinding(BindingMouse.Left, kActionToggleMembership, ctrl: true)]
        public ProxyBinding RoadGroupingToggleMembership { get; set; }

        /// <summary>
        /// Linear slider index. Maps to RefHalfHeight = 2^(6+index)/10 (6.4 … 102.4).
        /// Max index uses the old per-label extents formula (automatic).
        /// </summary>
        [SettingsUISection(kGeneralTab, kRoadNameLabelsSection)]
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
            => index <= RoadNameLabelRefHalfHeightIndexMin || index > RoadNameLabelRefHalfHeightIndexMax;

        /// <summary>RefHalfHeight for fixed steps: 2^(6+index)/10. Caller must not use this for automatic.</summary>
        public static float IndexToRefHalfHeight(int index)
            => math.exp2(RoadNameLabelRefHalfHeightExponentBase + index) / 10f;

        public string FormatRefHalfHeightIndex(int index)
        {
            return index switch
            {
                1 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight0")),
                2 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight1")),
                3 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight2")),
                4 => LocalizationExtensions.Translate(GetOptionLabelLocaleID("RoadNameLabelRefHalfHeight3")),
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
