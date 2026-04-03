# [0014] add-name-file-hot-reload-file-watcher

**Developed by:** 

## User Story

> Acting as **a player editing name files in a text editor**, I want **library name files to reload automatically when saved from an external editor**, so that I **see changes in-game immediately without clicking the reload button**.

---

## Background

AdrNameFilesManager only reloads files when explicitly triggered (UI button or mod load). A FileSystemWatcher on the NamesetsFolder could automatically call ReloadNameFiles when .txt files change, enabling a live-edit workflow.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] A FileSystemWatcher is set up on NamesetsFolder monitoring *.txt create/change/delete/rename events
- [ ] An edit to a .txt name file in an external editor is reflected in the library list within 2 seconds
- [ ] The watcher is disposed when the mod unloads (OnDispose)
- [ ] No crash or exception if the folder is deleted and recreated

---

## Implementation Notes

1. Use System.IO.FileSystemWatcher with a debounce to avoid multiple rapid events
2. Dispatch the reload through the main thread queue (actionsToGoOnUpdate pattern from AdrMainSystem)

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


