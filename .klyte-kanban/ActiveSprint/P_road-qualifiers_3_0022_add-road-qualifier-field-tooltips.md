**Start time:** 2026-04-03 22:39 -0300
# [0022] add-road-qualifier-field-tooltips

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a city builder**, I want **each road qualifier field to show a tooltip explaining what the field controls**, so that I **I can learn what each condition does without reading documentation**.

---

## Background

The RoadPrefixCmp.tsx shows many numeric inputs and tri-state checkboxes for road conditions. Users need context about what each field means — especially the new fields added in Sprint 001 (car lanes, width). The euis-components Cs2FormLine component should be extended with a tooltip prop that is rendered as a data-tooltip attribute (CS2 native tooltip system).

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Cs2FormLine in euis-components submodule accepts an optional tooltip prop (string)
- [ ] When tooltip prop is provided, the wrapper div renders data-tooltip attribute
- [ ] The submodule commit is made before the main project commit
- [ ] All new and existing road qualifier field rows in RoadPrefixCmp.tsx have descriptive tooltip text
- [ ] i18n.csv has new tooltip keys for each road qualifier field and all 11 locale files are updated

---

## Implementation Notes

1. In _Frontends/UI/_replacements/euis-components/src/components/Cs2FormLine.tsx: add 'tooltip?: string' to Props, add data-tooltip={tooltip} to the wrapper div
2. Commit the submodule (_Frontends/UI/_replacements) before committing the main project
3. In RoadPrefixCmp.tsx pass tooltip={translate('roadPrefixSettings.tooltip.FIELDNAME')} to each Cs2FormLine
4. Add tooltip keys to i18n.csv for: patternFormat, minimumSpeed, maximumSpeed, requireFullBridgeState, requireAnyElevatedState, minimumCarLanes, maximumCarLanes, minimumWidthM, maximumWidthM, the flags section header
5. Update all 11 locale files with translations

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


