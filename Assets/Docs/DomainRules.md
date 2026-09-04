# Price Modifier Pipeline — Domain Rules

## Experiment Question
판매가 규칙이 늘어날 때 Preview와 Commit 계산 일관성을 유지하면서 확장 가능한 구조를 어떻게 만들 것인가?

## Independent Variable
판매가 보정 규칙을 선택하고 계산 경로에 전달하는 Architecture:
Controller Direct Calculation vs Modifier Pipeline.

## Controlled Rules
두 Option은 BasePrice, Season, Distance, Event, Item Type, LocalSpecialty 정책,
Modifier Order, Rounding, 동일한 입력과 Preview / Commit 기대 결과를 공유한다.
Phase 1에서는 이 통제변인만 고정하며 Architecture 비교는 시작하지 않는다.

## Rules
기본 테스트 BasePrice는 100이다. BasePrice는 0 이상의 int이며 생성자에서 음수를 거부한다.

| Rule | State | Multiplier |
| --- | --- | --- |
| Season | Normal | 1.0 |
| Season | Favored | 1.2 |
| Season | Unfavored | 0.8 |
| Distance | Short | 1.0 |
| Distance | Long | 1.3 |
| Event | None | 1.0 |
| Event | Lucky | 1.5 |

수치의 코드 Single Source는 Common/Rules/SellPriceRules.cs이다.
Season, Distance, Event는 외부 시스템 연동 없는 입력 데이터다.
Calendar, 좌표/Route 거리 산출, 랜덤 이벤트 생성은 구현하지 않는다.

### Item Type / LocalSpecialty
- Normal: Season, Distance, Event를 모두 적용한다.
- LocalSpecialty: Season과 Distance는 적용 대상에서 제외하고 Event만 적용한다.
- 제외는 ×1.0 적용과 다르다. 해당 단계 자체를 실행하지 않는다.
- 실제 적용 대상 선택은 이후 각 Architecture가 담당한다. Common에는 선택기를 두지 않는다.

## Modifier Order
BasePrice → Season → Distance → Event → FinalRound.

LocalSpecialty는 BasePrice → Event → FinalRound이다.
순서는 SellPriceRules.ModifierOrder에도 명시한다. 이 값은 계약을 설명하며 실행기가 아니다.
현재 곱셈만으로 결과가 같더라도 향후 각 Option은 이 실행 순서를 지켜야 한다.

## Rounding
RawPrice는 decimal로 계산한다. 중간 반올림은 하지 않는다.
모든 적용 대상 계산 후 Math.Round(rawPrice, 0, MidpointRounding.AwayFromZero)를
정확히 한 번 호출하여 int FinalPrice로 변환한다.
최종 결과가 int 범위를 넘으면 OverflowException으로 실패한다. Clamp나 wrap은 하지 않는다.

예: Normal / Favored / Long / Lucky, Base 100 → 234.
LocalSpecialty의 동일 Context → 150.
Base 1의 Normal / Favored / Long / Lucky → Raw 2.34 → 2 (단계별 반올림 시 3).
Base 3의 Normal / Normal / Short / Lucky → Raw 4.5 → 5.

## Architecture-Neutral Input
SellPriceInput은 BasePrice, ItemType, Season, Distance, Event의 get-only 속성을 가진 readonly struct이다.
생성 시 음수 BasePrice와 정의되지 않은 enum 값을 ArgumentOutOfRangeException으로 거부한다.
default(SellPriceInput)은 BasePrice 0 / Normal / Normal / Short / None인 유효 입력이다.
Unity Inspector 직렬화 DTO가 아니며 필요한 UI 변환은 이후 Phase에서 담당한다.
Common assembly는 UnityEngine / UnityEditor에 의존하지 않는다.

## Expected Results
아래 조합의 BasePrice는 모두 100이다.

| Item Type | Season | Distance | Event | FinalPrice |
| --- | --- | --- | --- | --- |
| Normal | Normal | Short | None | 100 |
| Normal | Favored | Short | None | 120 |
| Normal | Unfavored | Short | None | 80 |
| Normal | Normal | Long | None | 130 |
| Normal | Normal | Short | Lucky | 150 |
| Normal | Favored | Long | Lucky | 234 |
| LocalSpecialty | Favored | Long | None | 100 |
| LocalSpecialty | Favored | Long | Lucky | 150 |

## Verification
Assets/02.Tests/EditMode/SellPriceDomainRuleTests.cs에 테스트 전용 private Reference 계산을 둔다.
이 함수는 Production 계산기로 사용하지 않는다.
필수 8개 기대값, 제외된 단계, 실행 순서, 최종 1회 반올림, midpoint, 0,
음수/잘못된 enum 입력, int 상한 및 overflow를 검증한다.
Unity Test Runner의 EditMode에서 PriceModifierPipeline.EditMode.Tests를 실행한다.
Reference 테스트의 통과가 미래 Direct/Pipeline 구현의 정확성을 보장하지는 않는다.
각 Option의 Preview/Commit 경로는 구현 후 이 기대값으로 별도 검증해야 한다.

## Phase 1 Scope
공통 데이터, 규칙 정의, 테스트 전용 Reference, 문서만 구현한다.
Direct/Pipeline Controller, Resolver, Pipeline Context, IPriceModifier,
Common Debug Panel, Validator, Weather Modifier는 구현하지 않는다.
Scene/Prefab, ProjectSettings, Package 구성은 변경하지 않는다.
