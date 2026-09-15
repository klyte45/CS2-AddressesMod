# Road Grouping tool — tooltips (draft)

*Added: 2026-09-14 · Status: **draft stub** — expand later · Related: [09](09-road-grouping-tool-spec.md), [10](10-road-grouping-overlay-render-system.md), [23](23-adr-net-kind-gate-labels-seeds.md) · Code: `AdrRoadGroupingTool`, `AdrRoadGroupingOverlayRenderSystem`*

## Goal

Add **player-facing tooltips** for the Road Grouping tool so soft-fails and gestures are explained in-world / near-cursor, instead of relying on PSI notifications + log lines alone.

This doc is a **planning stub**. Do not treat as locked UX or a Kanban catalog yet.

## Why now

| Gap today | Player sees |
|-----------|-------------|
| Net-kind reject (Ctrl+toggle / endpoint drag) | Overlay red wash `#FF0000` + PSI notification + info log |
| Endpoint drag abort (no path / UI / empty space) | Silent or log-only; no command hint |
| Gestures (select, Ctrl+toggle, end-circle drag, fixed pin, RMB/Esc) | Only tool-options panel copy; no hover/command tooltip |

Tooltips should cover **errors** and **commands** in one system.

## Scope (draft)

### In

- Context tooltips while `AdrRoadGroupingTool` is active
- Soft-fail reasons (kind mismatch, unclassified / out-of-scope net, drag destination unreachable, atomic drag abort)
- Command / gesture cheat-sheet (mode-dependent: no selection vs ADR selected vs endpoint-dragging)
- i18n via pending `*_entries.txt` (assembled keys under `K45::ADR.vuio[roadGrouping.…]`)
- Prefer replacing or demoting PSI pops for routine kind rejects once tooltips are reliable

### Out (for later detail)

- Full vanilla tooltip pipeline research (which Game UI tooltip APIs to hook)
- Exact layout, delay, fade, keybind icons
- Board tasks / sprint slice ID
- Teaching first-run coach marks

## Tooltip kinds (sketch)

| Kind | When | Content (intent) |
|------|------|------------------|
| **Idle / mode help** | Tool active; no urgent error | Short command list for current mode |
| **Hover reject** | ADR selected; hover non-member that fails `EvaluateNetKindJoin` (red wash) | Why it cannot join (kind mismatch vs unsupported net) |
| **Action reject** | Ctrl+toggle or drag release soft-fail | Same reason as hover; confirm action did nothing |
| **Drag help** | Endpoint-dragging | Release to apply; invalid drop = no change; fixed pin reminder if pinned |

## Command lines to surface (from current input map)

Mode **no selection**:

- LMB on aggregated road → select group
- RMB / Esc → exit tool

Mode **ADR selected**:

- Ctrl+LMB segment → toggle membership
- LMB drag end-circle → extend / shrink / pivot
- Double-click member → fixed membership pin
- RMB / Esc → clear selection (stay in tool)

Mode **endpoint-dragging**:

- Release on valid path → apply adds/removes atomically
- Invalid / unsupported destination → no change (red hover)

## Feedback channels (to reconcile later)

| Channel | Role today | Proposed later |
|---------|------------|----------------|
| Overlay color | Belonging / preview / red reject | Keep; tooltip explains *why* |
| PSI `NotificationSystem.Pop` | Kind reject | Optional; tooltip may supersede for in-tool rejects |
| `LogUtils` info | Diagnostics | Keep for modders; not player UX |
| Tool options panel | Name / highway / cards | Keep; not a substitute for world tooltips |

## Open questions (for detail pass)

1. Hook: game tool tooltip, custom Gameface overlay, or mouse-tool-options strip?
2. Show command tooltip always vs only after first idle second?
3. One string per reject (`kindMismatch` / `kindUnclassified`) vs richer “Road vs Pathway” copy?
4. Bind key names from `ProxyAction` / ModData so remaps stay accurate?
5. Should SIP / non-tool surfaces reuse the same reject strings?

## Next steps

1. Research vanilla / WE tooltip patterns via kcs2m frontend MCP when implementing.
2. Expand this doc with locked UX + key list + mock copy.
3. Add board task(s) only when explicitly requested (likely after NET-02 playtest or with tool polish slice).
