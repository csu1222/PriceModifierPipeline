# Phase 2 — Common debug panel

## Contract

`IPriceArchitectureDebugSource.CaptureSnapshot()` → immutable `PriceDebugSnapshot` → `PriceModifierDebugPanel`.
The panel forwards `IPriceArchitectureDebugCommand` calls to the assigned component and refreshes after each command. It also refreshes on enable and supports an explicit `Refresh()` call. There is no Update polling.

The contracts assembly references only Phase 1 Common and has no Unity dependency. Snapshot consistency is `NotEvaluated` until both results exist, then `Pass` for equal prices or `Fail` for unequal prices. Applied flags are preserved as supplied, independently of item type.

## Scene and prefab

Open `Assets/Scenes/00_DebugPanelTest.unity` and enter Play Mode. The scene supplies an explicitly wired `DebugPanelTestRuntime`, Canvas, and Input System EventSystem.

`Assets/03.Prefabs/PriceModifierDebugPanel.prefab` holds UI references only. Each future scene must assign Source Behaviour and Command Behaviour on its instance. Both may reference the same component. Missing or incompatible dependencies log an error, disable the panel, and disable its buttons. External listeners are not removed.

The UI uses existing UGUI Text, Unity's built-in font, and a 1280×720 reference CanvasScaler. TMP Essential Resources were absent, so UGUI Text avoids introducing shared TMP settings. No package installation or project setting changes are required.

`Tools > Price Modifier > Create Debug Panel Test Assets` creates the assets through Unity APIs and refuses to overwrite existing Scene/Prefab assets. It preserves the user's open scenes. The prefab requires an enclosing Canvas; the test scene provides one.

## Fixture behavior

The source is explicitly labeled **UI Test Fixture (no calculation)**. It is not an architecture implementation. Prices are fixed 234/235 test values and do not represent the selected input's calculated price.

- Initial / Reset: Normal, Favored, Long, Lucky; Preview and Commit 234; PASS.
- Preview: alternates fixed 234/235 values. From Reset, the first Preview produces 235 and FAIL.
- Commit: copies the current preview fixture, or uses fixed 234 if no preview exists.
- Next Season: Normal → Favored → Unfavored → Normal.
- Toggle Distance / Event / Item Type: cycle the corresponding two values.
- Any input command clears result availability, producing N/A.
- LocalSpecialty fixture: Season OFF, Distance OFF, Event ON. The fake supplies these flags; the panel does not infer them.

For N/A → PASS, change an input, click Preview, then Commit. Click Preview again for FAIL. Reset restores the initial fixture.

## Verification

Phase 1 tests remain unchanged. The separate Debug test assembly checks consistency, supplied flag preservation (including intentionally unusual LocalSpecialty flags), and the generated scene in Play Mode. The scene test invokes all seven Button UnityEvents, checks rendered values, re-enables the panel to check subscription cleanup, checks text overflow, and verifies safe shutdown after the source is destroyed.

Automated Button UnityEvent invocation does not verify physical pointer hit testing or subjective GameView appearance. Inspect at 1280×720 and 1920×1080 before recording a portfolio GIF.

### Executed on 2026-09-04

- Unity 6000.3.10f1, isolated project copy under `Temp/Phase2Validation`.
- Asset generation: exit code 0; Scene/Prefab saved by Unity APIs and copied with Unity-generated metadata.
- Unity Test Runner: **28 passed, 0 failed** (Phase 1: 20; Phase 2: 7 snapshot cases + 1 PlayMode integration scenario).
- Final compilation: no C# errors. The PlayMode test includes label preferred-height checks and dependency-loss handling.
- Evidence: `Logs/Phase2-assets-build.log`, `Logs/Phase2-tests.log`, `Logs/Phase2-tests.xml` (local ignored logs).
- Batch ScreenCapture produced no image; physical mouse interaction and visual inspection at both target resolutions remain unverified.
- Existing package duplicate-assembly/native-extension messages appear in batch logs; packages were not changed.

When using the creation menu in another project state, save any untitled scene first; Unity cannot create an additive scene alongside an untitled unsaved scene. The builder refuses existing output assets rather than regenerating their identities.

No Direct controllers, Pipeline/Resolver, Weather, architecture selector, performance measurement, or Phase 1 domain changes are included.
