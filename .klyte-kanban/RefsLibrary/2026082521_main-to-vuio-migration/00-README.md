# Main → VUIO migration reference pack

Guide for retiring the EUIS app at `_Frontends/UI/main/` and recreating its full settings UI inside `_Frontends/UI/k45_adr_vuio/`, opened from the vanilla Universal Mod Menu.

This pack is **documentation only**. It does not migrate product code. A later implementation sprint should follow these files in order.

## Scope

| In scope | Out of scope |
|----------|--------------|
| Kill EUIS `main` app completely (hard cut) | Shipping TS/C# migration in this pack |
| Host settings in `k45_adr_vuio` via UMM + **GamePanel** | Overriding Economy/CityInfo/etc.; Editor `Portal` host |
| Convert euis-components → vuio-commons / `cs2/*` | Changing C# `k45::adr.*` binding contracts |
| Full feature parity including Region map | Kanban sprint/task creation (follow-up) |
| Retarget or **inline** `adr-commons` (prefer inline into vuio after `main` gone) | Deleting `main` on disk before code sprint |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Rollout | **Big-bang** — UMM panel complete; EUIS `main` retired in same sprint |
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

## Success criteria (implementation sprint)

- [ ] `moduleRegistry.append('UniversalModMenu', …)` registers an ADR button; right-menu Mods icon appears when any mod appends UMM
- [ ] ADR settings registered as GamePanel (`GamePanelType` + `gamePanelComponents`); UMM button calls `toggleGamePanel`
- [ ] Settings GamePanel opens at roughly **70vh × 80vw** (size may be adjusted later in development)
- [ ] Settings GamePanel exposes all four root tabs with feature parity to current `main`
- [ ] `UseEuisRegister` / `EuisIsMandatory` / `EuisApps` removed or disabled; mod loads without EUIS
- [ ] `_Frontends/UI/main/` deleted from product shipping path
- [ ] `adr-commons` either inlined into `k45_adr_vuio` **or** retargeted off `_replacements/euis-components` (prefer inline — see [08](08-shared-libs.md))
- [ ] Region map (pan/zoom/overlays/selection) works from VUIO panel
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

None — all product decisions in **Confirmed decisions** were settled (AskQuestion rounds). Revisit only if implementation uncovers a new blocker; then add a subject block here before coding around it.
