# <아트라스 오더 소트 히어로즈>
> 정렬을 통해 영웅들을 강화하고 몰려오는 몬스터를 막아냅니다.

|항목|내용|
|장르|하이브리드 캐주얼|
|세부장르|로그라이크 3-Sort 디펜스|
|엔진|Unity 6000.3.9f1|
|언어|C#|
|개발기간|2026.05.06 ~ 2026.07.16|
|인원|총 13명 (기획8/개발5)|

# 주요 기능
- 전투 시스템
  - 상태 패턴을 사용한 AI 전투
- 
- 데이터 세이브 -> 스테이지 클리어 혹은 보상 획득 후 데이터 직렬화 및 저장.

# 기술 스택
| 엔진 및 렌더링 | Unity6, URP, UIParticle |
| 데이터 로딩 | Addressable, ScriptableObject, Newtonsoft.Json |
| | |

# 아키텍쳐
- ServiceLocater를 기반으로 한 각종 매니저 관리
- BaseCharacter를 상속받아 PlayerCharacter, Monster 구
