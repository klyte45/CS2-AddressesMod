# [0021] improve-vehicle-identifier-visual-design

**Developed by:** 

## User Story

> Acting as **a city builder**, I want **vehicle plate identifier slots to be visually distinct by type and centered**, so that I **I can immediately see which slot is Local, Car Number or Regional without confusion from space-separated patterns**.

---

## Background

The vehicle plate editor shows identifier slots side by side with only spaces between them. When a pattern contains spaces, it becomes ambiguous. Each slot type (Regional=0, Local=1, CarNumber=2) should have a color background so its type is immediately obvious. Centering the layout also improves readability.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Each slot in the vehicle plate editor has a colored background matching its type (Regional, Local, CarNumber)
- [ ] The vehicle plate letters editor container layout centers its items
- [ ] Color coding is applied via CSS variables so it fits game theme
- [ ] Preview rows still show correctly alongside the redesigned slots

---

## Implementation Notes

1. In VehiclePlateControllerComponent.tsx, add a CSS className to each .letterItem div that corresponds to its source type: 'slotLocal', 'slotCarNumber', 'slotRegional'
2. In vehiclePlateController.scss, add background-color rules for .slotLocal, .slotCarNumber, .slotRegional using accent/info/warning colors from the game theme
3. Add justify-content: center to the .vehiclePlateLettersEditorContainer
4. Use visually distinct colors: e.g. var(--accentColorLight) for local, var(--warningColor) for car number, var(--positiveColor) for regional

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


