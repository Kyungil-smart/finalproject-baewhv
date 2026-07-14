# <아트라스 오더 소트 히어로즈>
> 정렬을 통해 영웅들을 강화하고 몰려오는 몬스터를 막아냅니다.
<img width="292" height="650" alt="image" src="https://github.com/user-attachments/assets/6e9a54a5-dc5a-44b7-897d-638603e0d7c2" />

|항목|내용|
|---|---|
|장르|하이브리드 캐주얼|
|세부장르|로그라이크 3-Sort 디펜스|
|엔진|Unity 6000.3.9f1|
|언어|C#|
|개발기간|2026.05.06 ~ 2026.07.16|
|인원|총 13명 (기획8/개발5)|

# 주요 기능
- 전투 시스템
  - 상태 패턴을 사용한 AI 전투.
  - Navimesh+ 를 이용한 길찾기 로직.
- 3Sort 시스템
  - 레일의 기물을 옮겨 캐릭터에 배치해 스테이터스를 높이는 Sort
  - 레일 내 연속한 3개의 오브젝트를 체크하여 Sort를 강화하는 Combo
- 보상 시스템
  - 레벨업 시 보상을 선택해 해당 전투에서 캐릭터 강화.
  - 스테이지 클리어 혹은 이벤트 노드를 통해 캐릭터 강화.
- 데이터 세이브
  - 스테이지 클리어 혹은 보상 획득 후 데이터 직렬화 및 저장.
- 내러티브
  - 게임의 몰입감 향상을 위한 스토리 페이지.

# 기술 스택
| 분류 | 사용 기술 |
| 엔진 및 그래픽 | Unity6, URP, UIParticle |
| 연출 | DOTween |
| 리소스 관리 | Addressable |
| 데이터 저장 | ScriptableObject, Newtonsoft.Json |

# 아키텍쳐
|이름|설명|관련 자료|
|---|---|---|
| ServiceLocater | ServiceLocater를 기반으로 한 각종 매니저 관리. |[클래스 다이어그램](https://github.com/Kyungil-smart/finalproject-baewhv/wiki/Manager%EA%B5%AC%EC%84%B1)|
| BaseManager | 서비스 로케이터에 등록 가능한 매니저클래스. 씬 타입을 지정해 항시유지, 세션유지, 씬 유지 선택 ||
| SceneController | DDOL 기능을 활용하여 SessionScene을 만들고 해당 씬에만 존재하는 매니저 배치 ||
| GameManager | 스테이지 선택 및 전투 스테이지에 배치되어 주요 규칙을 상태 패턴으로 관리  ||
| PlayerManager | 데이터 드리븐을 통해 캐릭터 리소스, 스테이터스, 스킬을 초기화하고 UI와 연동. 생성 후 맵에 배치.||
| MonsterManager | 데이터 드리븐을 통해 몬스터 풀링 리소스를 제작. 웨이브 시작 시 웨이브 규칙에 맞는 몬스터 배치. ||
| BaseCharacter | BaseCharacter 를 상속받아 PlayerCharacter, Monster 구현. ||
| ObjectPooling | 몬스터 및 이펙트 풀링. ||

# 프로젝트 구조
```
Assets/
├─_Shared/ -> 리소스 서브모듈 레포지토리
├─Data/    -> SheetToJson으로 직렬화한 데이터 모음
├─Prefabs/ ->
│ ├─Manager/ -> BaseManager 컴포넌트를 포함한 프리펩
│ ├─MonsterPrefab/ -> 몬스터
│ ├─Object/ -> Sort에 사용되는 오브젝트 UI
│ ├─PlayerPrefab/ -> 플레이어
│ ├─SO/ -> 스크립터블 오브젝트
│ ├─Stage/ -> 스테이지를 구성한 프리펩
├─Scenes/ -> 씬 구성 참조


│ ├ └ ─
```

# 씬 구성
| 씬 이름 | 역할 |
|---|---|
|TitleScene| 타이틀 씬|
|ModeScene| 스토리 혹은 아카이브씬을 선택하는 씬|
|StageSelectScene| 현재 진행중인 스테이지를 확인하고 스테이지로 진입할 수 있는 선택씬|
|StageScene| 전투 스테이지 |

