# Release Notes

## 1.1.2 - 2026-04-10

### Highlights
- Fixed table data font-size application so `TableTheme.DataFontSize` is used during table row rendering.
- Scaled table data row height with data font size to prevent oversized rows with small text.

### Tests
- Added PDF generation test with custom table data font size.
- Added propagation test for table data font size from theme to rendered table element.

## 1.1.1 - 2026-04-08

### Highlights
- Merge-fix release after integrating `develop` into `feature/autopager`.
- Preserved auto-pagination improvements and renderer updates during conflict resolution.

### Notes
- No intentional breaking API changes compared to 1.1.0.

## 1.1.0 - 2026-04-08

### Highlights
- Added opt-in automatic pagination for overflowing body content.
- Added continuation page generation when content exceeds page bounds.
- Added paragraph splitting across pages when enabled.
- Added table row continuation across pages when enabled.
- Added continuation controls for repeating header/footer on generated pages.

### Fluent API
- Added `WithAutoPagination(bool enabled = true)`.
- Added `WithAutoPagination(Action<AutoPaginationBuilder>)`.
- Added `SettingsBuilder.AutoPagination(Action<AutoPaginationBuilder>)`.
- Added new `AutoPaginationBuilder` options:
  - `Enabled(bool)`
  - `RepeatHeaderOnContinuation(bool)`
  - `RepeatFooterOnContinuation(bool)`
  - `SplitParagraphs(bool)`
  - `SplitTables(bool)`

### JSON/Model Updates
- Added `settings.autoPagination` support in templates:
  - `enabled`
  - `repeatHeaderOnContinuation`
  - `repeatFooterOnContinuation`
  - `splitParagraphs`
  - `splitTables`

### Reliability
- Added safeguards to prevent infinite continuation loops during pagination.

### Tests
- Added/updated tests for fluent configuration and automatic continuation behavior.
- Full test suite passing at release cut.

### Upgrade Notes
- Behavior is backward compatible by default. Automatic pagination is disabled unless explicitly enabled.
