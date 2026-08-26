# 09 — EUIS decommission checklist

Hard cut: ADR must run **without** Extra UI Screens installed. No fallback period.

Execute only after feature parity ([07](07-tab-by-tab-migration.md) M5).

## C# — `AddressesCs2Mod.cs`

Current (to remove):

```csharp
protected override bool EuisIsMandatory => true;
protected override bool UseEuisRegister => true;
protected override Dictionary<string, EuisAppRegister> EuisApps => new()
{
    ["main"] = new("Addresses Mod for CS2", $"{BaseUrlApps}/k45-adr-main.js", …)
};
```

Also remove `#if LOCAL` / `BaseUrlApps` if only used for EUIS apps.

### Target

- Stop overriding `UseEuisRegister` / `EuisIsMandatory` / `EuisApps` (defaults in `BasicIMod` already disable EUIS), **or** explicitly set `UseEuisRegister => false` and `EuisIsMandatory => false`
- Keep `DoOnLoad` → `AddUIModule` for `k45_adr_vuio`
- Keep all ECS system registrations unrelated to EUIS

### `BasicIMod` EUIS bridge

No need to delete EUIS helpers from shared `BasicIMod` if other mods use them — ADR simply must not call into registration.

Search ADR for:

- `EuisApps`
- `UseEuisRegister`
- `EuisIsMandatory`
- `RegisterAtEuis`
- `CouiHost` usages only needed for EUIS JS/CSS URLs
- Comments / regions titled “Binding EUIS” that are actually `k45::adr` binders (rename comments only; binders stay)

## Frontend packaging

| Item | Action |
|------|--------|
| `_Frontends/UI/main/` | Delete project after port |
| Build scripts that `npm run build` in `main` | Remove from CI / solution hooks |
| Published files `k45-adr-main.js` / `.css` | Stop shipping under `UI/` |
| Solution / csproj content includes for main dist | Remove |
| LOCAL webpack port `8715` docs | Replace with VUIO `webpack --watch` workflow |
| `AddressesCS2.sln` / frontend project refs | Drop main if listed |

## npm / workspace

| Item | Action |
|------|--------|
| `main/package.json` | Gone with folder |
| Root scripts referencing `@k45/adr-main` | Remove |
| `k45_adr_vuio` deps | **Path A:** vuio-commons only (adr sources inlined). **Path B:** adr-commons + vuio-commons. Never euis |
| adr-commons package | Delete if Path A; else strip EUIS tarball deps per [08](08-shared-libs.md) |

## Player-facing / docs

| Item | Action |
|------|--------|
| Mod description “requires Extra UI Screens” | Remove |
| Workshop / README dependency list | VUIO-only / no EUIS |
| Changelog | Note EUIS removal + Universal Mod Menu entry |
| In-game tip / i18n mentioning EUIS app | Update if any |

## Verification (must pass)

- [ ] Fresh playset **without** EUIS: game loads ADR
- [ ] Right-menu Mods glyph appears (because ADR appends UMM)
- [ ] ADR settings panel opens with full tabs
- [ ] No console/log errors about missing EUIS bridge / `k45-adr-main`
- [ ] Contextual VUIO (selection / editor / tool) still works
- [ ] `rg "euis-components|k45-adr-main|EuisApps|UseEuisRegister" BelzontAdr _Frontends` clean for product paths

## Rollback note (docs only)

Hard cut means no dual-ship. If map parity fails late, **delay the release** — do not re-enable EUIS for a half port. Fix M5 instead.

## Suggested commit message theme (implementation sprint)

`chore: remove EUIS main UI; settings via Universal Mod Menu (VUIO)`
