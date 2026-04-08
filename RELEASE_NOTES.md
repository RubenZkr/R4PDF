# Release Notes

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
