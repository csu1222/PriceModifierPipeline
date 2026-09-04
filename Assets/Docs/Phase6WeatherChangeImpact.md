# Weather Modifier Change Impact

## 1. Final Verdict

**PASS** — Unity 6000.3.10f1에서 최종 218/218 테스트 PASS, 기존 130/130 회귀 PASS, 신규 72/72 Context 동등성 PASS. 세 Scene의 자동 렌더 검증과 Comparison Scene의 실제 OS 포인터 입력을 확인했다. Standalone 실행은 수행하지 않았으며 이번 Phase의 필수 조건이 아니다.

검증 날짜: 2026-09-04. 이 문서는 Phase 6의 After Evidence이다. `Phase5ArchitectureComparison.md`의 Before Evidence와 측정 정의는 수정하지 않았다.

## 2. New Requirement / Common Rule

Weather는 **Architecture 확장성을 비교하기 위한 데모용 신규 요구사항**이다. 원래 프로젝트에 존재했던 기능이라고 주장하지 않는다.

| Weather | Multiplier |
| --- | ---: |
| Clear | ×1.0 |
| Rain | ×1.1 |
| Storm | ×1.2 |

적용 순서: BasePrice → Season → Distance → Event → Weather → FinalRound.

모든 중간 계산은 decimal이다. 마지막 한 번만 `MidpointRounding.AwayFromZero`로 반올림하고 checked int 변환을 수행한다.

- Normal: Season / Distance / Event / Weather 적용.
- LocalSpecialty: Season / Distance 제외, Event / Weather 적용.
- Clear도 WeatherApplied = true, multiplier = 1.0이다. ON은 계산 단계 참여를 의미한다.
- 기존 5인자 SellPriceInput 호출과 default 입력은 Clear로 동작한다. 잘못된 Weather enum 값은 생성자에서 거부한다.
- NextWeather는 Clear → Rain → Storm → Clear로 순환한다. 다른 입력 변경은 현재 Weather를 유지한다. 입력 변경은 기존 계산 결과를 무효화하며 Reset은 Clear를 복원한다.

## 3. Functional Equivalence

BasePrice 100 × ItemType 2 × Season 3 × Distance 2 × Event 2 × Weather 3 = **72 Context**.

각 Context에서 독립 기대값 = Direct Preview = Direct Commit = Pipeline Preview = Pipeline Commit을 검증했다. 결과는 **72/72 PASS, mismatch 0**이다. 실제 두 MonoBehaviour Runtime의 Preview/Commit을 각각 호출하며 Preview 결과를 Commit으로 복사하지 않는다.

`Phase6WeatherTests`의 oracle은 Production 상수/Resolver/Calculator/표시 lookup을 참조하지 않는 요구사항 기반 decimal 계산이다. `WeatherComparisonExpectations`는 별도의 고정 72개 표시 기대값이며 이 oracle과 대조된다. 기존 24개 `PriceComparisonBaseline`과 Phase 5 테스트는 보존했다.

| Item / Season / Distance / Event | Clear | Rain | Storm |
| --- | ---: | ---: | ---: |
| Normal / Favored / Long / Lucky | 234 | 257 | 281 |
| LocalSpecialty / Favored / Long / Lucky | 150 | 165 | 180 |

Normal Rain: 100 ×1.2 ×1.3 ×1.5 ×1.1 = 257.4 → 257. Normal Storm: 280.8 → 281. LocalSpecialty Rain: 100 ×1.5 ×1.1 = 165.

## 4. Common Changes

공통 요구사항과 UI/관찰 연결 비용은 Architecture의 정책/계산 변경 비용과 분리한다. 아래 파일은 모두 실제 변경되었으며 제외된 작업을 숨기지 않도록 전체 목록에 포함했다. 경로는 repository-relative이다.

**Common Domain: 기존 2파일 수정, 1파일 신규.**

- `Assets/01.Scripts/Common/Domain/SellPriceInput.cs`: Weather 입력과 기본값/유효성 검사.
- `Assets/01.Scripts/Common/Rules/SellPriceRules.cs`: Weather 상수와 단계 순서.
- 신규 `Assets/01.Scripts/Common/Domain/WeatherPriceState.cs`.

**Shared Debug: 기존 5파일 수정.**

- `Assets/01.Scripts/Debug/Contracts/PriceDebugSnapshot.cs`: 입력·배수·적용 여부.
- `Assets/01.Scripts/Debug/Contracts/IPriceArchitectureDebugCommand.cs`: NextWeather.
- `Assets/01.Scripts/Debug/PriceModifierDebugPanel.cs`: Snapshot 표시 및 버튼 구독/해제.
- `Assets/01.Scripts/Debug/DebugPanelTestRuntime.cs`: 명령 계약 적응. 가격 계산 없는 UI fixture를 유지하며 Weather 적용값을 계산하지 않는다.
- `Assets/01.Scripts/Debug/Editor/PriceDebugPanelBuilder.cs`: 신규 생성 시 Weather 버튼 포함.

**두 옵션의 UI 입력/어댑터 연결: 기존 6파일 수정, 각 옵션 3파일.**

- `Assets/01.Scripts/DirectCalculation/DirectPriceRuntime.cs`: NextSeason, ToggleDistance, ToggleEvent, ToggleItemType의 입력 재생성 4곳에서 Weather 유지; NextWeather 1개 추가.
- `Assets/01.Scripts/DirectCalculation/DirectPriceDebugCommand.cs`: NextWeather 전달.
- `Assets/01.Scripts/DirectCalculation/DirectPriceDebugSource.cs`: Snapshot Weather 결과 전달.
- `Assets/01.Scripts/ModifierPipeline/PipelinePriceRuntime.cs`: Direct와 같은 4개 입력 재생성 위치와 NextWeather.
- `Assets/01.Scripts/ModifierPipeline/PipelinePriceDebugCommand.cs`: NextWeather 전달.
- `Assets/01.Scripts/ModifierPipeline/PipelinePriceDebugSource.cs`: Snapshot Weather 결과 전달.

이 6파일은 공통 UI 요구사항을 각 옵션에 연결한 실제 비용이다. Runtime 자체가 Production class라는 점도 아래 넓은 범위 집계에 반영한다. Preview/Commit 메서드 본문은 양쪽 모두 바뀌지 않았다.

**Comparison: 기존 3파일 수정.**

- `Assets/01.Scripts/ArchitectureComparison/ArchitectureComparisonRuntime.cs`: 양쪽 NextWeather 전달, 새로운 기대값 조회.
- `Assets/01.Scripts/ArchitectureComparison/PriceArchitectureComparisonPanel.cs`: Weather 표시와 명령 전달, 기대값 조회 교체.
- `Assets/01.Scripts/ArchitectureComparison/Editor/PriceArchitectureComparisonSceneBuilder.cs`: Weather 버튼 및 7개 버튼 배치.

**공통 지원 신규 3파일.**

- `Assets/01.Scripts/ArchitectureComparison/WeatherComparisonExpectations.cs`: 고정 72개 표시 기대값.
- `Assets/01.Scripts/Debug/Editor/Phase6WeatherPrefabUpdater.cs`: PrefabUtility/SerializedObject/Undo로 기존 공통 Prefab 업데이트.
- `Assets/01.Scripts/ArchitectureComparison/Editor/Phase6WeatherSceneUpdater.cs`: 기존 Comparison Scene 버튼 추가/배치/콜백 연결 및 Dirty/Save 처리.

합계: 공통/Debug/Comparison/입력 연결 **기존 C# 16파일 수정, C# 4파일 신규**. Serialized Asset 2파일 수정은 별도이다. 테스트와 Evidence 문서는 이 숫자에 포함하지 않는다.

## 5. Direct Changes

Architecture-specific 정책·계산·결과 데이터 변경은 기존 **3파일: class 2개 + struct 1개**이다.

| File under Assets/01.Scripts/DirectCalculation | 실제 변경 위치 (After line) | 역할 |
| --- | --- | --- |
| DirectPreviewController.cs | CalculatePreview: Weather switch 41, 곱셈 50, 결과 전달 53 | 독립 Preview 정책/계산 |
| DirectCommitController.cs | CalculateCommit: Weather switch 41, 곱셈 50, 결과 전달 53 | 독립 Commit 정책/계산 |
| DirectPriceResult.cs | Weather 속성 9–10, 생성자 인자 17 및 대입 26–27 | 결과 데이터 확장 |

각 Controller에 기존 경로를 유지한 채 Weather switch 하나와 곱셈 하나를 추가했다. Preview/Commit 통합, 공통 Weather helper, 추가 Resolver는 없다.

Weather 정책 종류가 Preview와 Commit에 중복되어 **New Duplicate Policy = 1**이다. 두 switch를 중복 2개로 집계하지 않는다. LocalSpecialty 판단 밖에 Weather를 추가하여 추가 특산품 분기는 **0**이다.

## 6. Pipeline Changes

Architecture-specific 정책·계산·결과 데이터 변경은 기존 **5파일: class 2개 + struct 3개**이다.

| File under Assets/01.Scripts/ModifierPipeline | 실제 변경 위치 (After line) | 역할 |
| --- | --- | --- |
| SellPriceCalculationContext.cs | Weather 속성 12, 생성자 대입 21 | 입력 전달 |
| SellPriceModifierResolver.cs | Resolve: Weather switch 32, resolved 결과 전달 42 | 단일 정책 선택 |
| ResolvedSellPriceModifiers.cs | Weather 속성 8–9, 생성자 인자 16 및 대입 24–25 | resolved 데이터 확장 |
| PriceCalculator.cs | Calculate: resolved Weather 곱셈 13 | 정책 의미 없는 실행 |
| PipelinePriceResult.cs | Weather 결과 접근자 10–11 | 결과 전달 |

**Calculator 수정 필요.** 기존 개별 배수 필드 구조를 유지했으며 `WeatherApplied`가 true일 때 `WeatherMultiplier`를 곱하는 실행문을 추가했다. Calculator는 Clear/Rain/Storm을 판단하지 않는다. 임의 resolved 배수 1.37과 applied false에 대한 테스트로 정책 선택과 실행 경계를 확인했다.

`PipelinePriceService.cs`는 Before와 byte-identical이다. 공개 Calculate(SellPriceInput) 사용 방식과 Preview/Commit 호출 구조는 유지된다. Generic modifier collection으로 미리 리팩터링하지 않았다.

New Duplicate Policy = **0**, LocalSpecialty 추가 분기 = **0**.

## 7. Before Baseline / After Metrics

기준 HEAD: `2bc99c1022d6c04ce88d586d6f08f3d82257e350`, branch `main`. Phase 5 파일은 작업 시작 시 untracked였으므로 HEAD만으로 비교하지 않고 당시 작업 트리 전체 원본과 SHA-256을 함께 보존했다.

Assets / Packages / ProjectSettings의 **213파일**이 Before snapshot에 포함된다. Phase 5 문서와 Comparison source/Scene도 여기에 포함된다. 과거 문서의 line 표기 대신 실제 Before/After diff와 현재 line을 사용한다.

Phase 5의 정의를 그대로 적용한다: switch 하나는 정책 위치 하나, 조건 하나는 위치 하나, 동일 옵션의 여러 경로에 반복된 의미상 정책 종류를 중복 하나로 센다. 산술, resolved flag 실행, 데이터 전달, 상수/기본값, validation, Debug/Test/Editor는 Policy Location에서 제외한다.

| Metric | Direct Before | Direct After | Pipeline Before | Pipeline After |
| --- | ---: | ---: | ---: | ---: |
| Policy Class Count | 2 | 2 | 1 | 1 |
| Policy Location Count | 8 | 10 | 4 | 5 |
| Duplicate Policy Count | 4 | 5 | 0 | 0 |
| LocalSpecialty Branch Count | 2 | 2 | 1 | 1 |

After 근거: Direct 각 Controller의 Item guard 18, Season switch 21, Distance if 32, Event if 37, Weather switch 41 = 5곳 ×2. Pipeline Resolver의 Item guard 14, Season switch 17, Distance if 26, Event if 29, Weather switch 32 = 5곳. Calculator의 flag 실행은 정책 위치가 아니다.

## 8. Change Impact / Direct vs Pipeline Change Cost

**집계 범위:** 공통 Domain 및 UI/Debug/Comparison을 분리한 정책·계산 변경 기준이다. class는 C# class만 센다. struct를 class로 바꾸어 세지 않으며 아래 별도로 표시한다.

**Modified Location**은 Phase 6의 정책/계산 변경 지점을 나타내는 추가 지표다: 독립 Weather 정책 선택문 1개 또는 Weather 산술 실행문 1개가 한 위치이다. switch case 수, 초기값, DTO 필드/생성자/return 인자 전달은 여기에 더하지 않는다. 기존 Phase 5 **Policy Location** 정의를 확장하거나 산술을 과거 수치에 섞지 않는다. 데이터 전달 위치는 5·6절에 빠짐없이 열거했다.

| Metric | Direct | Pipeline |
| --- | ---: | ---: |
| Architecture-Specific Modified Classes (policy/calculation) | 2 | 2 |
| Architecture-Specific Modified Structs (data transport) | 1 | 3 |
| Architecture-Specific Modified Types / Files | 3 | 5 |
| Architecture-Specific Modified Locations (policy + arithmetic) | 4 = 2 + 2 | 2 = 1 + 1 |
| New Policy Locations (Phase 5 definition) | 2 | 1 |
| New Duplicate Policy | 1 | 0 |
| Preview Path Changed (call structure) | No | No |
| Commit Path Changed (call structure) | No | No |
| Existing Calculator Changed | 별도 Calculator 없음; Controller 산술 2곳 수정 | Yes, PriceCalculator |
| Existing Policy Class Changed | 2 Controllers | 1 Resolver |
| LocalSpecialty Additional Branch | 0 | 0 |
| Common UI/input integration files within option folders | 3 | 3 |
| All changed files within option folders, including UI/input | 6 | 8 |

넓은 Production 집계도 명시한다: DebugSource/DebugCommand를 제외하고 **Runtime class를 포함하면 수정 class는 Direct 3 / Pipeline 3**이다. struct를 포함한 전체 non-Debug Production type은 Direct 4 / Pipeline 6이다. DebugSource/DebugCommand까지 포함한 실제 옵션 폴더 class는 5 / 5이다. 서로 다른 범위의 숫자를 같은 지표처럼 비교하지 않는다.

이 실험에서 Pipeline은 정책 위치와 중복을 줄였지만 수정 class 수를 줄이지 않았고 데이터 전달 타입은 더 많이 수정했다. 이 결과를 숨기지 않는다.

## 9. Files Changed / Change Set Evidence

Before 대비 기존 파일 **27개 수정 = 기존 C# 25개 + Serialized Asset 2개**.

- Common/Debug/Comparison/입력 연결: 기존 C# 16개 — 4절 전체 목록.
- Direct architecture-specific: 기존 C# 3개 — 5절 전체 목록.
- Pipeline architecture-specific: 기존 C# 5개 — 6절 전체 목록.
- Regression test 계약 적응: `Assets/02.Tests/EditMode/SellPriceDomainRuleTests.cs` 1개. Reference의 Weather 단계 및 LocalSpecialty 적용 단계 기대값만 확장; 기존 20개 테스트 식별과 가격 기대값 유지.
- Serialized: `Assets/03.Prefabs/PriceModifierDebugPanel.prefab`, `Assets/Scenes/03_ArchitectureComparison.unity`.

신규 C# **7개**: 4절의 Domain enum/lookup/Editor updater 4개와 다음 테스트 3개.

- `Assets/02.Tests/Comparison/EditMode/Phase6WeatherTests.cs`
- `Assets/02.Tests/Comparison/EditMode/Phase6WeatherSceneTests.cs`
- `Assets/02.Tests/Pipeline/EditMode/Phase6WeatherDebugSceneTests.cs`

신규 문서: `Assets/Docs/Phase6WeatherChangeImpact.md`. 신규 C# 7개 및 문서 1개의 `.meta`는 Unity가 생성한다. 기존 `.meta`는 모두 byte-identical이다.

`01_DirectCalculation.unity`, `02_ModifierPipeline.unity`, `00_DebugPanelTest.unity`는 직접 저장/수정하지 않았다. 공유 Prefab의 새 버튼과 참조를 상속해 동작하며 기존 Scene 파일의 해시는 그대로이다. `.unity`/`.prefab` YAML 직접 편집은 수행하지 않았다.

로컬 실행 증거는 Git ignored `Logs/Phase6/`에 있다:

- `Before/`: 213파일 원본 복사.
- `baseline-sha256.json`, `after-sha256.json`, `measurement.json`: 파일 해시와 변경/추가 목록 및 테스트 식별 대조.
- `git-status-before.txt`, `head-before.txt`, `git-status-after.txt`.
- `changes-vs-before.diff`: 각 기존 파일에 대해 실행한 `git diff --no-index` 결과. Phase 5 untracked 파일도 비교한다.
- `Phase5ExecutionBefore/`: 회귀 실행 전에 보존한 Phase 5 XML/log/스크린샷. 기존 테스트의 고정 로그 경로 재사용으로 과거 실행 증거가 사라지지 않도록 복사했다.
- `prefab-update-elevated.log`, `scene-update.log`, `tests.xml`, `tests.log`, `interactive.log`, `manual-validation.md`, 해상도별 PNG 36개.

## 10. Tests

| Suite | Passed / Total |
| --- | ---: |
| 기존 Phase 1–4 | 102 / 102 |
| 기존 Phase 5 | 28 / 28 |
| Phase 6 72-context matrix | 72 / 72 |
| Phase 6 Domain/rounding/calculator/input tests | 10 / 10 |
| Phase 6 Scene/resolution tests | 6 / 6 |
| Total | 218 / 218 |

최종 XML의 failed / skipped / inconclusive는 모두 0이다. Before XML의 130개 fullname을 최종 XML과 대조하여 하나도 삭제/누락되지 않았음을 확인했다. 기존 24-context 테스트를 72개로 교체하지 않았다.

검증 범위: enum/기본값/invalid enum, Weather 상수와 순서 계약, 257/281/165, 최종 반올림 전 Weather 적용(1 ×1.2 ×1.3 ×1.5 ×1.1 = 2.574 →3), midpoint 5×1.1=5.5→6, zero, overflow, resolved flag 실행, Weather 보존/순환/무효화/Reset/양쪽 desync 감지.

실행 명령: Unity.exe `-batchmode -projectPath <repository> -runTests -testPlatform EditMode -testResults <repository>/Logs/Phase6/tests.xml -logFile <repository>/Logs/Phase6/tests.log`.

최초 실행은 214/218 PASS였다. Weather 버튼 이벤트가 OnEnable 외에 Execute에도 추가되던 구현 오류로 신규 Debug Scene 테스트 4개가 실패했다. 중복 등록을 제거한 후 전체 218개를 다시 실행하여 PASS했다. 최초 결과는 `tests-first.xml`/`tests-first.log`로 보존한다. 최종 C# compile error는 0이다.

## 11. Runtime Result

Direct / Pipeline / Comparison 세 Scene에서 각각 1280×720 및 1920×1080을 검증했다. 각 Scene·해상도 조합에서 Normal/LocalSpecialty × Clear/Rain/Storm을 실행하여 **36개 렌더 상태**를 확인했다.

- Direct/Pipeline: 독립 Preview/Commit 기대 가격, Weather ON과 배수, 입력 변경 후 결과 N/A 및 Weather OFF, 버튼 재활성화 후 단일 순환, Reset Clear 확인.
- Comparison: 동기화, 네 결과 일치, 특산품 Season OFF / Distance OFF / Event ON / Weather ON 확인.
- Canvas 픽셀 크기, 텍스트 preferredHeight, 화면 내 label bounds를 자동 검증했다.
- PNG 중 1280×720 Comparison Rain, 1920×1080 Comparison specialty Storm, Direct 1280×720, Pipeline 1920×1080 specialty Storm을 육안 확인했다. 텍스트와 버튼이 겹치거나 잘리지 않았다.

자동 렌더 테스트는 일시적으로 ScreenSpaceCamera + RenderTexture를 사용한다. 저장된 Scene은 ScreenSpaceOverlay이며 아래 실제 GameView 검증은 Overlay 그대로 수행했다.

## 12. Manual Validation

Computer Use의 실제 OS 마우스 입력으로 Unity Editor의 `03_ArchitectureComparison` GameView를 조작했다. 테스트용 `onClick.Invoke()`를 실제 포인터 검증이라고 보고하지 않는다. 사람의 손으로 클릭한 테스트라고 주장하지도 않는다.

**1920×1080 Full HD:** Next Season → Favored, Toggle Distance → Long, Toggle Event → Lucky, Run Both →234/234 양쪽 확인. Next Weather →Rain 후 Run Both →257/257, 다음 Storm →281/281. Toggle Item Type →LocalSpecialty 후 Run Both →180/180; Season/Distance OFF, Event/Weather ON 확인. Reset도 동일 해상도에서 확인했다.

**1280×720:** Weather Storm→Clear→Rain 순환, LocalSpecialty Rain에서 Run Both →165/165 및 정책 상태 확인. Reset →Normal/Normal/Short/None/Clear, 결과 N/A 확인. Next Season, Toggle Distance, Toggle Event, Toggle Item Type도 실제 버튼으로 각각 확인했다.

따라서 요청한 7개 명령을 두 GameView 해상도에서 모두 실제 포인터로 조작했다. 양쪽 해상도에서 전체 패널/버튼 표시를 육안 확인했다. 물리 창은 1537×817 screenshot 크기였고 GameView는 각각 0.61×(Full HD), 0.92×(720p)로 축소 표시되었다. native 1:1 화면 검증이라고 주장하지 않는다.

Phase 5의 실제 포인터 미검증 항목을 이번 Weather 확장 Scene에서 해소했다. 과거 Phase 5 문서를 소급 수정하지 않았다. PlayMode를 종료했고 Full HD 설정을 복원했다. Standalone player는 미실행이다.

## 13. Responsibility Check

- Resolver: Weather 의미와 배수 선택 소유.
- Calculator: resolved flag/배수 실행, decimal 및 최종 반올림 소유. Weather enum 의존 없음.
- Direct: 기존 두 Controller의 독립 정책/계산 소유 유지.
- Runtime: 입력/결과 상태 변경 및 무효화 소유. Preview/Commit 호출 본문 유지.
- Panel: Snapshot/결과 포맷과 명령 전달. Storm→1.2 같은 정책 판단이나 가격 계산 없음.
- Comparison: 양쪽 입력 동기화와 결과 비교. 고정 기대값 lookup은 관찰 계층에만 존재한다.

## 14. Regression Check

기존 130개 테스트가 최종 XML에 모두 남아 있고 PASS한다. Clear의 24개 기존 가격 Context와 기존 Phase 5 표시 lookup도 유지한다. 기존 서비스, 정책 예외, 호출 구조, Package/assembly 경계는 유지했다.

Before 대비 213파일 중 27파일만 승인 범위 내에서 변경되었고 **186파일은 byte-identical**, 누락 0이다. Phase 5 문서, 기존 lookup, PipelinePriceService, 기존 모든 `.meta`, 00/01/02 Scene, Packages 및 기존 ProjectSettings는 보존한다.

## 15. Scope Check

새 Architecture, IPriceModifier 계층, generic rule engine, reflection discovery, ScriptableObject 정책, Addressables, 성능 benchmark, Direct/Pipeline 리팩터링을 도입하지 않았다. 신규 테스트는 기존 assembly에 위치하므로 asmdef 수정이나 Package 추가도 없다.

## 16. Git Status

branch `main`, HEAD는 시작과 동일하다. staging / commit / push를 수행하지 않았다. Phase 5의 원래 untracked 작업과 Phase 6 추가 파일을 혼동하지 않도록 Before snapshot 비교를 사용한다.

최종 기존 tracked 수정은 23파일이며, Phase 5에서 이미 untracked였던 3개 Comparison script와 Comparison Scene의 Phase 6 수정은 별도 Before diff에 나타난다. 정확한 untracked 전체 경로는 `git-status-after.txt`에 기록한다.

`git diff --check`는 Unity가 생성한 Prefab의 빈 `m_Name: ` 항목 3곳에서 trailing whitespace를 보고한다. YAML 직접 편집 금지에 따라 이를 손으로 정리하지 않았다. C# 변경의 whitespace 검증과 신규 C# 검토는 별도로 수행했다.

## 17. Risks / Trade-off

- Pipeline은 Weather 선택을 한 위치에 모았지만 고정된 multiplier 필드 구조이므로 Calculator와 context/resolved/result 타입 수정이 필요했다. OCP를 완전히 충족한다고 주장하지 않는다.
- Direct는 더 적은 데이터 타입을 수정하지만 Preview/Commit의 Weather 선택과 산술을 각각 관리해야 한다. 이 실험은 실제 작업 시간이나 성능을 측정하지 않는다.
- class, struct, 정책 위치, 계산 실행 위치, UI 변경 파일 수는 서로 다른 단위이다. 하나만 선택해 전체 Architecture의 우열로 일반화하지 않는다.
- 72개 Matrix는 BasePrice 100에 한정된다. 별도 boundary/rounding 테스트가 있지만 모든 int/decimal 입력에 대한 완전 증명은 아니다. 표시 lookup도 BasePrice 100만 지원한다.
- 생성자 optional 인자는 기존 소스 호출 호환성을 위한 것이다. 사전 컴파일 외부 바이너리 ABI 호환을 보장하지 않는다. IPriceArchitectureDebugCommand 구현체에는 NextWeather 구현이 필요하며 repository의 구현체를 모두 갱신했다.
- 기존 Unity package의 duplicate assembly / 미설치 platform native-extension 진단이 남는다. Package 변경으로 해결하지 않았으며 C# compile error와 구분한다.
- 최초 sandbox Unity 실행은 사용자 캐시 DB 접근 실패로 시작하지 못했다. 승인된 실행으로 재시도해 Editor 업데이트/테스트를 완료했다.
- Unity가 실행 중 자동 생성한, Before에 없던 SceneTemplateSettings.json은 로그에 사본을 남긴 뒤 제거했다. 최종 ProjectSettings 변경은 없다.
- `Logs/Phase6` 실행 증거는 로컬 ignored 파일이다. 포트폴리오 외부 전달 시 문서와 필요한 XML/PNG/diff를 함께 별도로 보존해야 한다.

## 18. Portfolio Evidence

1. “동일한 Weather 가격 규칙을 추가했을 때, 공통 UI 변경을 제외한 정책·계산 class는 Direct와 Pipeline 모두 2개를 수정했으며, 독립 정책·계산 변경 위치는 각각 4곳과 2곳이었습니다. 결과 전달 struct 수정은 각각 1개와 3개였습니다.”
2. “Weather 정책은 Direct의 Preview·Commit 두 위치에 추가됐고 Pipeline에서는 Resolver 한 위치에 추가됐습니다. 의미상 신규 중복 정책은 Direct 1개, Pipeline 0개였습니다.”
3. “72개 Context에서 독립 기대값과 Direct/Pipeline Preview·Commit 결과가 모두 일치했고, 기존 130개를 포함한 총 218개 테스트가 통과했습니다.”
4. “Pipeline에서도 기존 Calculator 수정이 필요했지만 Preview·Commit 호출 구조는 유지했습니다. Comparison Scene의 7개 명령은 1280×720 및 1920×1080 GameView에서 실제 OS 포인터 입력으로 확인했습니다.”

## 19. Next Phase Readiness

최종 포트폴리오 Bullet 작성 준비 완료. Before/After 정책 지표, Common과 Architecture-specific 변경 분리, 수정 타입/위치, 실제 72개 동등성 및 회귀/포인터/해상도 결과를 근거로 사용할 수 있다. 성능 우위, 변경 class 수 감소, Standalone PASS 등 측정하지 않았거나 사실이 아닌 주장은 사용하지 않는다.
