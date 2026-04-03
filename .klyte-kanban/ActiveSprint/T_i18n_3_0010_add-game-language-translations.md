**End time:** 2026-04-03 20:00 -0300
**Start time:** 2026-04-03 19:59 -0300
# [0010] Add basic machine translation support all game languages

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a non-English-speaking player**, I want **the Addresses mod UI to be available in my language**, so that I **use the mod comfortably without needing to read English**.

---

## Background

i18n.csv currently only has en-US and pt-BR columns. The game have other languages available, and players who use those languages may have trouble using the mod if they don't understand English. Adding machine translation support for all game languages would make the mod more accessible to a wider audience. The pattern of the project for more languages is to add a new file at same level as i18n.csv with the name pattern `LOCALE.csv`, where `LOCALE` is the locale code of the language registered in the game. For example, `fr-FR.csv` for French (France). This file would have the same structure as i18n.csv but with the translated strings for that language.

Since it's a mod for a game, it's acceptable to use machine translation as a starting point. The community can then contribute with corrections and improvements to the translations over time.

ATTENTION: Only compile the project using the solution file, as explained on the kanban README! This project was compiling before, so if it stopped it was something you have done!
---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] All game languages supported by Cities: Skylines 2 have a corresponding translation file in the mod with machine-translated strings based on the English and Portuguese sources.

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


