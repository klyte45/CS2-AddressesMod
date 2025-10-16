using Belzont.Utils;
using Game.SceneFlow;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public abstract class BaseVehicleSerialSettingEditorController : DataBaseController
    {
        protected abstract string Prefix { get; }
        protected abstract VehicleSerialSettings Data { get; set; }

        private bool m_initialized = false;

        public MultiUIValueBinding<string[]> LettersAllowed { get; private set; }
        public MultiUIValueBinding<uint> FlagsOwnSerial { get; private set; }
        public MultiUIValueBinding<uint> FlagsCarNumber { get; private set; }
        public MultiUIValueBinding<int> BuildingIdOnStart { get; private set; }

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
            FlagsOwnSerial = new(default, $"{Prefix}{nameof(FlagsOwnSerial)}", EventCaller, CallBinder);
            FlagsCarNumber = new(default, $"{Prefix}{nameof(FlagsCarNumber)}", EventCaller, CallBinder);
            BuildingIdOnStart = new(default, $"{Prefix}{nameof(BuildingIdOnStart)}", EventCaller, CallBinder);

            LettersAllowed.OnScreenValueChanged += (x) => { var dt = Data; dt.LettersAllowed = x; Data = dt; };
            FlagsOwnSerial.OnScreenValueChanged += (x) => { var dt = Data; dt.FlagsOwnSerial = x; Data = dt; };
            FlagsCarNumber.OnScreenValueChanged += (x) => { var dt = Data; dt.FlagsCarNumber = x; Data = dt; };
            BuildingIdOnStart.OnScreenValueChanged += (x) => { var dt = Data; dt.BuildingIdOnStart = x < 0 ? null : x > 0; Data = dt; };

            m_initialized = true;
            FetchData();
        }

        public void FetchData()
        {
            if (!m_initialized) return;
            LettersAllowed.Value = Data.LettersAllowed;
            FlagsOwnSerial.Value = Data.FlagsOwnSerial;
            FlagsCarNumber.Value = Data.FlagsCarNumber;
            BuildingIdOnStart.Value = Data.BuildingIdOnStart.HasValue ? Data.BuildingIdOnStart.Value ? 1 : 0 : -1;
        }

        public void ForceUpdateScreens()
        {
            LettersAllowed.UpdateUIs();
            FlagsOwnSerial.UpdateUIs();
            FlagsCarNumber.UpdateUIs();
            BuildingIdOnStart.UpdateUIs();
        }
    }

    public class BusSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.bus.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.BusSerialSettings; set => adrVehicleSystem.BusSerialSettings = value; }
    }

    public class TaxiSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.taxi.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.TaxiSerialSettings; set => adrVehicleSystem.TaxiSerialSettings = value; }
    }

    public class PoliceSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.police.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.PoliceSerialSettings; set => adrVehicleSystem.PoliceSerialSettings = value; }
    }

    public class FiretruckSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.firetruck.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.FiretruckSerialSettings; set => adrVehicleSystem.FiretruckSerialSettings = value; }
    }

    public class AmbulanceSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.ambulance.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.AmbulanceSerialSettings; set => adrVehicleSystem.AmbulanceSerialSettings = value; }
    }

    public class GarbageSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.garbage.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.GarbageSerialSettings; set => adrVehicleSystem.GarbageSerialSettings = value; }
    }

    public class PostalSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.postal.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.PostalSerialSettings; set => adrVehicleSystem.PostalSerialSettings = value; }
    }
    public class DeathcareSerialSettingsEditorController : BaseVehicleSerialSettingEditorController
    {
        protected override string Prefix => "vehicleSerial.deathcare.";
        protected override VehicleSerialSettings Data { get => adrVehicleSystem.PostalSerialSettings; set => adrVehicleSystem.PostalSerialSettings = value; }
    }

}

