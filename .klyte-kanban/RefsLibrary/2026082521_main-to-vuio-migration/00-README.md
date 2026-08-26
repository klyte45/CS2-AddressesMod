# Main → VUIO migration reference pack

Guide for retiring the EUIS app at `_Frontends/UI/main/` and recreating its full settings UI inside `_Frontends/UI/k45_adr_vuio/`, opened from the vanilla Universal Mod Menu.

This pack is **documentation only**. It does not migrate product code. Implementation follows the multi-sprint plan in [11-sprint-roadmap.md](11-sprint-roadmap.md) (S7 cutover removes `main` and `adr-commons`).

## Scope

| In scope | Out of scope |
|----------|--------------|
| Kill EUIS `main` app completely (hard cut) | Shipping TS/C# migration in this pack |
| Host settings in `k45_adr_vuio` via UMM + **GamePanel** | Overriding Economy/CityInfo/etc.; Editor `Portal` host |
| Convert euis-components → vuio-commons / `cs2/*` | Changing C# `k45::adr.*` binding contracts |
| Full feature parity including Region map | Kanban sprint/task creation (follow-up) |
| **Inline** `adr-commons` into vuio; delete package in final sprint | Deleting `main` / `adr-commons` before feature parity |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Rollout | **Hard cut in S7** — no player release until S7; EUIS `main` + `adr-commons` removed in S7 ([11](11-sprint-roadmap.md)) |
| Settings shell UX | **Vanilla-first GamePanel** — `GamePanelType` + `gamePanelComponents` + `centerLeftLayout`; size **70vh × 80vw** (review later); `ListWithContentTab` nav (not Editor Portal) |
| Override subtabs | **Another `ListWithContentTab`** — left list = 14 tabs + H2 section headers; right = tab body. Do not port `Cs2SideTabs` |
| i18n | **Keep `K45::ADR.main[...]`** — no CSV churn; optional rename later |
| Region editor | **Same milestone** — full map parity before killing `main` |
| EUIS dependency | **Hard cut** — no fallback period; VUIO-only mod |
| Doc pack | This full 11-file set |

## Reading order

1. [01-architecture-and-host.md](01-architecture-and-host.md) — why VUIO + UMM, not EUIS
2. [02-feature-inventory.md](02-feature-inventory.md) — complete feature matrix
3. [03-shell-and-umm.md](03-shell-and-umm.md) — registration + panel shell
4. [04-widget-conversion-map.md](04-widget-conversion-map.md) — EUIS → VUIO widgets
5. [05-styles-conversion.md](05-styles-conversion.md) — SCSS / CSS modules
6. [06-behaviors-and-bindings.md](06-behaviors-and-bindings.md) — RPC, events, risky behaviors
7. [07-tab-by-tab-migration.md](07-tab-by-tab-migration.md) — ordered port + target layout
8. [08-shared-libs.md](08-shared-libs.md) — adr-commons / vuio-commons / kill list
9. [09-euis-decommission.md](09-euis-decommission.md) — C# / npm / packaging checklist
10. [10-i18n-and-assets.md](10-i18n-and-assets.md) — keys, icons, assets
11. [11-sprint-roadmap.md](11-sprint-roadmap.md) — **implementation strategy** (S1–S7; S7 deletes `main` + `adr-commons`)

## Success criteria (end of roadmap / S7)

- [ ] `moduleRegistry.append('UniversalModMenu', …)` registers an ADR button; right-menu Mods icon appears when any mod appends UMM
- [ ] ADR settings registered as GamePanel (`GamePanelType` + `gamePanelComponents`); UMM button calls `toggleGamePanel`
- [ ] Settings GamePanel opens at roughly **70vh × 80vw** (size may be adjusted later in development)
- [ ] Settings GamePanel exposes all four root tabs with feature parity to current `main`
- [ ] Region map (pan/zoom/overlays/selection) works from VUIO panel
- [ ] `UseEuisRegister` / `EuisIsMandatory` / `EuisApps` removed or disabled; mod loads without EUIS
- [ ] `_Frontends/UI/main/` **deleted**
- [ ] `adr-commons` **inlined** into `k45_adr_vuio` and `_Frontends/UI/_shared/adr-commons/` **deleted** ([08](08-shared-libs.md) Path A)
- [ ] Existing selection-info / editor / tool VUIO surfaces still work unchanged

## Source ↔ target cheat sheet

| Today | Tomorrow |
|-------|----------|
| `_Frontends/UI/main/` (single-spa EUIS) | `_Frontends/UI/k45_adr_vuio/src/settings/` |
| `@klyte45/euis-components` | `@klyte45/vuio-commons` + `cs2/ui` / `VanillaComponentResolver` |
| `EuisApps["main"]` | UMM button → `toggleGamePanel(AdrSettingsPanelId)` |
| EUIS iframe shell | `gamePanelComponents` + `centerLeftLayout` (TransportationOverview pattern) |
| `MainSideTabMenuComponent` | GamePanel `Panel` + side list (`ListWithContentTab` pattern) |
| `translate` → `K45::ADR.main[...]` | **Same keys** (settings UI may keep main namespace) |

## Agent rules (when implementing from this pack)

Follow project Cursor rules; this pack does not override them:

| Rule | Implication for this migration |
|------|--------------------------------|
| `frontend-mcp-research` / `backend-research` | Research game UI / Managed APIs via **kcs2m MCP only** — do not browse game install or extracted UI dumps |
| `frontend-cohtml` | No CSS `gap` / Grid; prefer rem + patterns already in `_Frontends`; ask before new web features |
| `i18n-entries` | **Never** edit `BelzontAdr/i18n/i18n.csv` — new strings go in `*_entries.txt` (7 columns) or locale `.md` |
| `build-workflow` | Agents may `npm run build` frontend; ask the user to compile the C# solution |
| `klyte-kanban` | This pack lives under `RefsLibrary/` only; do not create board tasks unless asked |

## Open questions

Product decisions in **Confirmed decisions** were settled earlier. **Implementation sequencing** is locked in [11-sprint-roadmap.md](11-sprint-roadmap.md) (S1–S7; all open questions cleared).
