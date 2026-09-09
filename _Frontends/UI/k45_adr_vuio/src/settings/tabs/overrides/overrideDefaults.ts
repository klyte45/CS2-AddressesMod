import { translate } from "../../translate";
import { SimpleNameEntry } from "adr";

export const defaultSetting = (): SimpleNameEntry => ({
    IdString: null as any,
    Values: [],
    ValuesAlternative: [],
    Name: translate("overrideSettings.useVanillaOptionLbl"),
});

export const defaultSettingRoadByDistrict = (): SimpleNameEntry => ({
    IdString: null as any,
    Values: [],
    ValuesAlternative: [],
    Name: translate("overrideSettings.useSameAsCityOptionLbl"),
});

export enum OverrideTabs {
    RoadsDistricts = "RoadsDistricts",
    AddressFormat = "AddressFormat",
    Citizen = "Citizen",
    RoadPlates = "RoadPlates",
    RailPlates = "RailPlates",
    WaterPlates = "WaterPlates",
    AirPlates = "AirPlates",
    BusSerial = "BusSerial",
    TaxiSerial = "TaxiSerial",
    PoliceSerial = "PoliceSerial",
    FiretruckSerial = "FiretruckSerial",
    AmbulanceSerial = "AmbulanceSerial",
    GarbageSerial = "GarbageSerial",
    PostalSerial = "PostalSerial",
    DeathcareSerial = "DeathcareSerial",
}
