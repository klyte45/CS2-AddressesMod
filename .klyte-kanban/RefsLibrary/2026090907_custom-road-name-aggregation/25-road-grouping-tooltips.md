# Road Grouping tool — tooltips (requirements)

*Added: 2026-09-14 · Expanded: 2026-09-15 · Status: **locked — ready for coding sprint** · Related: [09](09-road-grouping-tool-spec.md), [10](10-road-grouping-overlay-render-system.md), [11](11-road-grouping-tool-roadmap.md), [23](23-adr-net-kind-gate-labels-seeds.md), [27](27-adr-net-kind-playtest-smoke-notes.md) · Code: `AdrRoadGroupingTool`, `AdrRoadGroupingOverlayRenderSystem`, `TempRoadMarkerTooltip` · Game.dll build GUID `d124bec16c0748da9ebf48366d9c1d68` (kcs2m)*

Supersedes the draft stub formerly at `25-road-grouping-tooltips-draft.md`.

## Goal

Add **player-facing mouse tooltips** for the Road Grouping tool so soft-fails and available gestures are explained next to the cursor (vanilla tooltip stack), instead of relying on PSI notifications + log lines alone.

Tooltips cover **errors** and **command hints** in one coherent system that matches vanilla tool UX.

## Why

| Gap today | Player sees |
|-----------|-------------|
| Net-kind reject (Ctrl+toggle / endpoint drag) | Overlay red wash `#FF0000` + PSI `NotificationSystem.Pop` + info log |
| Endpoint drag abort (no path / UI / empty space) | Silent or log-only; no command hint |
| Gestures (select, Ctrl+toggle, end-circle drag, fixed pin, RMB/Esc) | Only tool-options panel copy; no hover/command tooltip |
| Hover kind reject (red wash) | Overlay only — no text |

## Scope

### In

- Context mouse tooltips while `AdrRoadGroupingTool` is active
- Soft-fail reasons (kind mismatch, unclassified / out-of-scope net, drag destination unreachable, atomic drag abort)
- Mode-dependent command / gesture hints (InputHints with remapped-safe keybind display)
- i18n via pending `*_entries.txt` (assembled keys under `K45::ADR.vuio[roadGrouping.…]`)
- **Remove** in-tool PSI pops for kind rejects once Error tooltips ship (keep `LogUtils`)

### Out

- First-run coach marks / teaching overlays
- Custom Gameface React tooltip UI or MouseToolOptions strip as a command cheat-sheet
- SIP / non-tool reuse of reject strings (same keys may be reused later; not required this sprint)
- Richer “Road vs Pathway” mismatch interpolation (v1 keeps generic copy)
- Treating this requirements doc as a sprint task (docs are not board work)

## Vanilla pipeline (locked)

Vanilla mouse tooltips are **not** a frontend React overlay. They are ECS systems at `SystemUpdatePhase.UITooltip` that push widgets into `TooltipUISystem.mouseGroup` via `TooltipSystemBase.AddMouseTooltip`.

| Channel | Vanilla mechanism | ADR approach |
|---------|-------------------|--------------|
| **Available actions** | `InputHintsTooltipSystem` reads `activeTool.actions`; shows `InputHintTooltip` when `ProxyAction.displayOverride` is active and `priority != -1` | Expose ModData Apply / Cancel / Toggle via overridden `toolActions`; own `DisplayNameOverride` instances with mode-dependent locale keys |
| **Errors / warnings** | Tool-specific `TooltipSystemBase` + `StringTooltip` with `TooltipColor.Warning` / `Error` (e.g. `TempRenewableElectricityProductionTooltipSystem`) | New `AdrRoadGroupingTooltipSystem`; in-repo template: `BelzontAdr/Tooltips/TempRoadMarkerTooltip.cs` |

Vanilla tools often swap `applyActionOverride` / `cancelActionOverride` to named `toolActionCollection.GetActionState(...)` aliases. Mods cannot freely invent new collection assets. ADR **keeps** ModData mouse bindings (`K45_ADR_RoadGroupingApply` / `Cancel` / `ToggleMembership`) and instead:

1. Override `toolActions` to yield those three `ProxyAction`s (so `InputHintsTooltipSystem` discovers them).
2. Create and update `DisplayNameOverride` instances (`displayName` = locale id, `active`, `priority` &gt; 0).
3. Let vanilla render keybind + label.

Errors stay on the dedicated ADR tooltip system — **not** PSI.

```text
AdrRoadGroupingTool
  ├─ DisplayNameOverride (mode labels) ──► InputHintsTooltipSystem ──► mouseGroup
  ├─ hover / drag reject state ──► AdrRoadGroupingTooltipSystem (StringTooltip Error) ──► mouseGroup
  └─ overlay colors (unchanged; tooltips explain why / what)
```

Register `AdrRoadGroupingTooltipSystem` in `AddressesCs2Mod` at `SystemUpdatePhase.UITooltip` beside `TempRoadMarkerTooltip`.

## UX — priority (single mouse stack)

When Road Grouping is active:

1. **Error / reject** `StringTooltip` (`TooltipColor.Error`) if current hover/drag has a soft-fail reason — **suppress** conflicting Apply / Toggle InputHints for that invalid target.
2. Else **InputHint** rows for currently available gestures (mode + hover target).
3. Overlay colors unchanged; tooltips explain *why* / *what can be done*.

No idle delay (matches vanilla InputHints: always on while the tool is active).

Double-click **fixed membership pin** is **not** an InputHint (no dedicated ProxyAction). Do not invent a fake hint for it in v1.

## UX — mode → hint matrix

| Mode | Apply hint | Cancel hint | Toggle hint |
|------|------------|-------------|-------------|
| No selection + eligible hover | Select group | Exit tool | off |
| Selected + member hover | off (unless end-handle — see below) | Clear selection | Remove from group |
| Selected + non-member OK | off | Clear selection | Add to group |
| Selected + non-member reject | off | Clear selection | off + **Error** StringTooltip |
| Selected + end-handle hover | Start extend / shrink | Clear selection | off |
| Endpoint-dragging + valid path | Apply path change | Cancel drag | off |
| Endpoint-dragging + invalid | off + **Error** | Cancel drag | off |

## Error / reject strings

### Reuse (already in CSV)

| Assembled key | Intent (en-US already shipped) |
|---------------|--------------------------------|
| `K45::ADR.vuio[roadGrouping.kindMismatch]` | Cannot mix different network types in one group. |
| `K45::ADR.vuio[roadGrouping.kindUnclassified]` | This network type cannot join an ADR road group. |

Keep **generic** mismatch copy for v1 (no host/edge kind interpolation).

### Add in coding sprint (pending `*_entries.txt` only — never edit `i18n.csv`)

File: `BelzontAdr/i18n/roadGrouping_tooltips_entries.txt` (7 columns).

| Assembled key | Mock en-US | Mock pt-BR |
|---------------|------------|------------|
| `K45::ADR.vuio[roadGrouping.reject.dragUnreachable]` | Cannot reach that destination from this end. | Não é possível alcançar esse destino a partir desta extremidade. |
| `K45::ADR.vuio[roadGrouping.hint.select]` | Select road group | Selecionar grupo de vias |
| `K45::ADR.vuio[roadGrouping.hint.toggleAdd]` | Add to group | Adicionar ao grupo |
| `K45::ADR.vuio[roadGrouping.hint.toggleRemove]` | Remove from group | Remover do grupo |
| `K45::ADR.vuio[roadGrouping.hint.dragStart]` | Extend or shrink group | Estender ou encolher grupo |
| `K45::ADR.vuio[roadGrouping.hint.dragApply]` | Apply path change | Aplicar alteração do caminho |
| `K45::ADR.vuio[roadGrouping.hint.cancelSelection]` | Clear selection | Limpar seleção |
| `K45::ADR.vuio[roadGrouping.hint.exitTool]` | Exit tool | Sair da ferramenta |
| `K45::ADR.vuio[roadGrouping.hint.cancelDrag]` | Cancel drag | Cancelar arraste |

Optional later (only if tool surfaces a distinct pin-block reject): `roadGrouping.reject.fixedPinBlocks`.

Do **not** pass `translate` fallbacks for new keys.

## Feedback channels (locked)

| Channel | After tooltips ship |
|---------|---------------------|
| Overlay red / green / orange / yellow | **Keep** |
| PSI `NotifyNetKindReject` / `NotificationSystem.Pop` | **Remove** for in-tool kind rejects |
| `LogUtils` info / diagnostics | **Keep** |
| Tool options panel | **Keep**; not a tooltip substitute |

## Implementation sketch (coding sprint)

1. **Public tool state API** on `AdrRoadGroupingTool` — expose hover join result + drag reject reason so tooltip system and overlay share one source of truth (today `m_DragRejectReason` is private; hover reject is overlay-only).
2. **`AdrRoadGroupingTooltipSystem`** (`TooltipSystemBase`) — gate on `activeTool == AdrRoadGroupingTool`; `AddMouseTooltip` Error `StringTooltip` for kind + drag rejects.
3. **InputHints wiring** — override `toolActions`; create/update `DisplayNameOverride`s each tool update from the mode matrix; suppress Apply/Toggle when Error tooltip is showing for that target.
4. **i18n** — `roadGrouping_tooltips_entries.txt` as above.
5. **PSI demotion** — delete / no-op `NotifyNetKindReject` Pop path; keep log line.
6. **Playtest** — checklist in TT-06.

Key paths:

| Piece | Path |
|-------|------|
| Tool | `BelzontAdr/Tools/AdrRoadGroupingTool.cs` |
| Overlay | `BelzontAdr/Systems/AdrRoadGroupingOverlayRenderSystem.cs` |
| Membership gate | `BelzontAdr/Systems/AdrNameAggregateMembershipSystem.cs` (`EvaluateNetKindJoin`) |
| Template tooltip | `BelzontAdr/Tooltips/TempRoadMarkerTooltip.cs` |
| New tooltip system | `BelzontAdr/Tooltips/AdrRoadGroupingTooltipSystem.cs` |
| Mod registration | `BelzontAdr/AddressesCs2Mod.cs` (`UpdateAt<…>(UITooltip)`) |
| ModData actions | `BelzontAdr/AdrModData.cs` |

## Sprint task catalog (coding only)

Documentation of this file is **not** a sprint task. Board epic: `road-grouping`.

| Order | ID | Board | Title | Size | Depends | Outcome |
|-------|-----|-------|-------|------|---------|---------|
| 1 | TT-01 | **0092** L | Tool state API: public reject / hover reasons | S | — | Public join/drag reject accessors; single evaluation path shared with overlay |
| 2 | TT-02 | **0093** L | `AdrRoadGroupingTooltipSystem` Error StringTooltips | M | TT-01 | Kind + drag rejects as `TooltipColor.Error` mouse tooltips while tool active |
| 3 | TT-03 | **0094** L | InputHints wiring — `toolActions` + DisplayNameOverride matrix | M | TT-01 | Mode-dependent Apply / Cancel / Toggle hints with remapped-safe keybinds; suppress on Error |
| 4 | TT-04 | **0095** L | i18n pending entries for hints + new reject strings | S | — | `roadGrouping_tooltips_entries.txt` (7-col); no CSV edits |
| 5 | TT-05 | **0096** L | Remove PSI kind rejects; keep logs | S | TT-02 | No in-tool `NotificationSystem.Pop` for kind rejects |
| 6 | TT-06 | **0097** L | Playtest checklist | S | TT-02, TT-03, TT-05 | Hover reject, Ctrl soft-fail, drag invalid, mode hints, Esc/RMB |

Suggested pull order:

```text
TT-01 → TT-02 ∥ TT-04 → TT-03 → TT-05 → TT-06
```

## Success criteria (when TT catalog ships)

- [ ] Hovering a kind-rejected non-member shows Error tooltip (no PSI)
- [ ] Ctrl+toggle / drag soft-fail shows Error tooltip (no PSI); logs remain
- [ ] Valid modes show InputHint rows with correct labels + bindings
- [ ] Invalid targets do not show conflicting Apply/Toggle hints
- [ ] Overlay colors unchanged and still match reject/preview semantics
- [ ] Pending i18n entries cover all new assembled keys; no CSV edits by agents

## Relation

- [09](09-road-grouping-tool-spec.md) — tool input map
- [10](10-road-grouping-overlay-render-system.md) — overlay colors
- [11](11-road-grouping-tool-roadmap.md) — T0–T3 shipped scaffold
- [23](23-adr-net-kind-gate-labels-seeds.md) — kind gate semantics
- [27](27-adr-net-kind-playtest-smoke-notes.md) — notes tooltip polish as follow-up
