using Belzont.Utils;
using Game.SceneFlow;
using System;

namespace BelzontAdr
{
    public abstract class BaseVehiclePlateSettingEditorController : DataBaseController
    {
        protected abstract string Prefix { get; }
        protected abstract VehiclePlateSettings Data { get; set; }

        private bool m_initialized = false;

        public MultiUIValueBinding<string[]> LettersAllowed { get; private set; }
        public MultiUIValueBinding<uint> FlagsLocal { get; private set; }
        public MultiUIValueBinding<uint> FlagsCarNumber { get; private set; }
        public MultiUIValueBinding<uint> FlagsRandomized { get; private set; }
        public MultiUIValueBinding<int> MonthsFromEpochOffset { get; private set; }
        public MultiUIValueBinding<uint> SerialIncrementEachMonth { get; private set; }

        protected AdrVehicleSystem adrVehicleSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            adrVehicleSystem = World.GetOrCreateSystemManaged<AdrVehicleSystem>();
            GameManager.instance.onGameLoadingComplete += (x, y) => FetchData();
        }


        protected override void DoInitValueBindings(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {
            LettersAllowed = new(default, $"{Prefix}{nameof(LettersAllowed)}", EventCaller, CallBinder);
            FlagsLocal = new(default, $"{Prefix}{nameof(FlagsLocal)}", EventCaller, CallBinder);
            FlagsCarNumber = new(default, $"{Prefix}{nameof(FlagsCarNumber)}", EventCaller, CallBinder);
            FlagsRandomized = new(default, $"{Prefix}{nameof(FlagsRandomized)}", EventCaller, CallBinder);
            MonthsFromEpochOffset = new(default, $"{Prefix}{nameof(MonthsFromEpochOffset)}", EventCaller, CallBinder);
            SerialIncrementEachMonth = new(default, $"{Prefix}{nameof(SerialIncrementEachMonth)}", EventCaller, CallBinder);

            LettersAllowed.OnScreenValueChanged += (x) => { var dt = Data; dt.LettersAllowed = x; Data = dt; };
            FlagsLocal.OnScreenValueChanged += (x) => { var dt = Data; dt.FlagsLocal = x; Data = dt; };
            FlagsCarNumber.OnScreenValueChanged += (x) => { var dt = Data; dt.FlagsCarNumber = x; Data = dt; };
            FlagsRandomized.OnScreenValueChanged += (x) => { var dt = Data; dt.FlagsRandomized = x; Data = dt; };
            MonthsFromEpochOffset.OnScreenValueChanged += (x) => { var dt = Data; dt.MonthsFromEpochOffset = x; Data = dt; };
            SerialIncrementEachMonth.OnScreenValueChanged += (x) => { var dt = Data; dt.SerialIncrementEachMonth = x; Data = dt; };

            CallBinder($"{Prefix}newRandomSeed", () => { var dt = Data; dt.GenerateNewSeed(); Data = dt; });

            m_initialized = true;
            FetchData();
        }

        public void FetchData()
        {
            if (!m_initialized) return;
            LettersAllowed.Value = Data.LettersAllowed;
            FlagsLocal.Value = Data.FlagsLocal;
            FlagsCarNumber.Value = Data.FlagsCarNumber;
            FlagsRandomized.Value = Data.FlagsRandomized;
            MonthsFromEpochOffset.Value = Data.MonthsFromEpochOffset;
            SerialIncrementEachMonth.Value = Data.SerialIncrementEachMonth;
        }

        public void ForceUpdateScreens()
        {
            LettersAllowed.UpdateUIs();
            FlagsLocal.UpdateUIs();
            FlagsCarNumber.UpdateUIs();
            FlagsRandomized.UpdateUIs();
            MonthsFromEpochOffset.UpdateUIs();
            SerialIncrementEachMonth.UpdateUIs();
        }
    }

    public class RoadVehiclePlateEditorController : BaseVehiclePlateSettingEditorController
    {
        protected override string Prefix => "vehiclePlate.road.";
        protected override VehiclePlateSettings Data { get => adrVehicleSystem.RoadVehiclesPlatesSettings; set => adrVehicleSystem.RoadVehiclesPlatesSettings = value; }
    }

    public class RailVehiclePlateEditorController : BaseVehiclePlateSettingEditorController
    {
        protected override string Prefix => "vehiclePlate.rail.";
        protected override VehiclePlateSettings Data { get => adrVehicleSystem.RailVehiclesPlatesSettings; set => adrVehicleSystem.RailVehiclesPlatesSettings = value; }
    }

    public class AirVehiclePlateEditorController : BaseVehiclePlateSettingEditorController
    {
        protected override string Prefix => "vehiclePlate.air.";
        protected override VehiclePlateSettings Data { get => adrVehicleSystem.AirVehiclesPlatesSettings; set => adrVehicleSystem.AirVehiclesPlatesSettings = value; }
    }

    public class WaterVehiclePlateEditorController : BaseVehiclePlateSettingEditorController
    {
        protected override string Prefix => "vehiclePlate.water.";
        protected override VehiclePlateSettings Data { get => adrVehicleSystem.WaterVehiclesPlatesSettings; set => adrVehicleSystem.WaterVehiclesPlatesSettings = value; }
    }
}

