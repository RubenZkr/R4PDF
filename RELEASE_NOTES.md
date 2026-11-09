# Release Notes

## 1.2.0 - 2026-11-09

1.1.5 was not published; its `keepWithNext` ships with this release.

### Highlights
- Long words no longer run over the edge of their column or paragraph. A word wider than a line on its own (a file name, an identifier, a URL) is broken inside, after a separator such as `_`, `-` or `/` when there is one. Paragraphs and table cells share one wrapper (`TextWrapper`), and table cells now honour newlines.
- Per-cell styles on table rows: `cellStyles` holds a `backgroundColor`, `textColor` and `isBold` per cell over the style of the row. With `badge: true` the background is a rounded label around the text instead of a filled cell, to show a status.
- Anchors and links: every element and table row can carry an `anchor` and a `link`. `PdfGenerator.GenerateWithLayout` returns the PDF with the page and area (in points, from the top-left corner) of every anchor, so a document that is merged with others can link into it or point a table of contents at a heading in the middle of it. A `link` of `"#name"` jumps to the page of that anchor, any other value is opened as a URL.
- New `canvas` element: a drawing area in the flow of the page whose elements are positioned relative to its top-left corner (lines with `x1`/`y1`/`x2`/`y2` too) and do not move the layout cursor. Only the canvas takes up room: its `height`, or without one the bottom of its lowest element. With auto-pagination it moves to the next page as a whole. Meant for diagrams.

### Fixes
- A rectangle with an `x`/`y` was positioned twice from its own coordinates; it now uses the position the page renderer resolved, which is what makes it work inside a canvas.

### Internal
- Elements copy themselves with `PdfElement.Clone()` (a member-wise copy; tables, rows, paragraphs and canvases copy their lists). The four hand-written copies in `PdfGenerator`, `PageRenderer`, `TableRenderer` and `ParagraphRenderer` are gone; they were the cause of the `DataFontSize`, `NoBorders` and `IsBold` bugs fixed in 1.1.3 and 1.1.5, and a new property is now copied without extra code.

### Tests
- Added `LayoutFeatureTests`: breaking long words in tables and paragraphs, cell styles surviving pagination, bold cell styles, anchors on continuation pages, links to anchors and the layout of a canvas.

### Upgrade Notes
- Backward compatible. Text that used to overflow a narrow column now wraps, so such a row can get taller.

## 1.1.5 - 2026-11-06

### Highlights
- New `keepWithNext` on every element: with auto-pagination, an element (typically a heading) moves to the next page when it does not fit together with the start of the element after it (the first line of a paragraph, or the header and first row of a table). A chain of elements that all keep with next moves as a whole; on a page that holds nothing yet the element is rendered regardless. Templates for earlier versions ignore the property.
- Fixed `IsBold` on table rows being dropped when `TableElement` is cloned during auto-pagination (`PdfGenerator.CloneElement` and `PageRenderer.CloneElement`), so bold rows rendered in the regular face.

### Tests
- Added tests for `keepWithNext`: a heading that fits without its table moves along with it, without the flag only the table moves, and on an empty page the element is rendered anyway.
- Added an integration test that renders a bold table row with auto-pagination enabled and checks the bold face is used.

## 1.1.4 - 2026-04-10

### Highlights
- Rebuild release to ensure the distributed NuGet binaries include the `DataFontSize`/`NoBorders` clone preservation fix used during auto-pagination.

## 1.1.3 - 2026-04-10

### Highlights
- Fixed `DataFontSize` and `NoBorders` not being preserved when `TableElement` is cloned during auto-pagination (`PdfGenerator.CloneElement` and `PageRenderer.CloneElement`).

### Tests
- Added propagation test verifying `DataFontSize` is preserved through auto-pagination cloning.

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
