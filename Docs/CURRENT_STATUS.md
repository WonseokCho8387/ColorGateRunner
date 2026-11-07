# Current Status

## Iteration 84 — Bottom Navigation Cell Alignment (2026-11-07 KST)

- 기준: `e4563ae`(Iteration 83, 푸시 전). Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음, 활성 플랫폼
  WebGL. Push 없음. 패키지·ProjectSettings 변경 없음.
- 하단 메뉴 버튼·아이콘·글자를 화면 20% 균등 칸 대신 배경 이미지(`BottomNavigationDock.png`)의 실제 칸 위치
  (`FrontendSceneBuilder.NavigationCellBounds`)에 맞췄다. 선택 커서(`NavigationSelection.png`)는 육각 몸체가 칸
  테두리를 감싸도록 크기와 위치를 계산하고, 라벨 띠 뒤에 그려 아래 장식이 글자를 가리지 않게 했다.
- 자동 검증: **EditMode 637/637; PlayMode 309 passed + 1 intentional ignore (310)**. Builder 2회. Product 저장과
  Campaign PlayerPrefs 전후 동일. 게임 규칙·생성 변경이 없어 시뮬레이션은 다시 돌리지 않았다(Iteration 83 해시 유지).
- 무료 웹판 로컬 캡처로 HOME 칸 커서와 다섯 칸의 아이콘·글자 정렬을 확인했다(수정 전후 비교).

## Iteration 83 — Online Campaign Ranking (2026-11-07 KST)

- 기준: `0cfefdf`(Iteration 82, 푸시 전) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음. 사용자 승인으로 네트워크와 패키지 2개를 추가했다.
- Unity Cloud 프로젝트 `ColorGateRunner`(`6771fae3-70aa-4800-b75d-0b4980a7eacd`, 조직 `wonseok-cho`)에 연결했다
  (ProjectSettings `cloudProjectId`). 리더보드 `campaign-total`: 높은 점수 위, Best Score, 리셋·버킷·티어 없음.
- 패키지: `com.unity.services.authentication` `3.8.1`, `com.unity.services.leaderboards` `2.3.4`
  (`com.unity.services.core` `1.18.0` 기존).
- RANK 탭(`RankPageView`): 열 때 익명 로그인, 이름이 없으면 `Guest` 지정(서비스가 `#NNNNN`을 붙임), 스테이지
  최고 점수 합계를 제출(0이면 제출 안 함)하고 상위 10명과 내 순위를 보여 준다. 실패하면 `OFFLINE`과 연결 안내.
  목록 뒤에는 단색 배경판(`RankListPlate`). 합계는 스테이지마다 완벽 클리어 최대 점수로 잘라 더한다(기본 범위 검사). 플레이테스트 빌드와 배치 테스트
  실행은 랭킹을 붙이지 않는다(`AppServiceGraph.Ranking` null).
- 자동 검증: **EditMode 636/636; PlayMode 309 passed + 1 intentional ignore (310)**(활성 플랫폼 WebGL). Builder 2회.
  Campaign 시뮬레이션과 Step 10 산출물 8개 해시 Iteration 80 기준과 **동일**. Product 저장과 Campaign PlayerPrefs
  전후 동일.
- 실제 서버 확인(무료 웹판, 로컬 브라우저): 익명 로그인 `200`, 이름 설정 `200`(`Guest#58552`), 순위 조회 `200`,
  내 점수 없음 `404` → 화면 `ONLINE`·`NO SCORES YET`. 점수 제출은 브라우저 창이 가려져 게임이 느려져서 실제로는
  확인하지 못했다(가짜 서비스 테스트로만 검증).
- 결제(IAP) 패키지가 Unity 서비스 초기화와 함께 사용 통계 요청(`prd.insights.analytics.unity3d.com`)을 보낸다.
  확인 시 응답 상태 `0`(전달 실패로 보임). 별도 결정 필요.

## Iteration 82 — Offline Stage Score (2026-11-06 KST)

- 기준: `8ce9f5e`(Iteration 81, 푸시 전) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음. 온라인 랭킹 1단계: 네트워크 없이 점수만 계산·저장한다.
- 점수(`Core/StageScorePolicy`): 난이도 배수 보통 ×1, 어려움 ×2, 매우 어려움 ×3. 이번 판 크라운 CLEAN `100`,
  COMBO `200`, FULL COMBO `300`(×배수), 최고 콤보 × `10` × 배수, 최소 탭 수를 넘은 탭 1개당 `−20`, 0 미만 없음.
  Comeback은 점수 없음.
- 최소 탭 수(`StageSession.MinimumTapCount`): 판정된 게이트 색을 순서대로 이으며 색 순환 거리를 센다(3색에서
  뒤 색은 2탭). Booster로 지난 게이트는 세지 않는다. 실제 탭은 `TapCount`.
- 결과 화면: `TIME … · BEST …` 한 줄과 `SCORE n (BEST m) · +k TAPS` 한 줄. 스테이지 최고 점수는
  `StageRecord.BestScore`/`LocalStageProgressData.BestScore`(옛 저장은 0)에 높은 값만 남는다. 음수는 저장 검증 실패.
- 자동 검증: **EditMode 632/632; PlayMode 309 passed + 1 intentional ignore (310)**(활성 플랫폼 WebGL). Campaign
  시뮬레이션과 Step 10 산출물 8개 해시 Iteration 80 기준과 **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.

## Iteration 81 — Guide Holds Without Lost Taps (2026-11-06 KST)

- 기준: `4cf8fce`(Iteration 80, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음.
- 색 맞추기 멈춤(Stage 1, 13 초록): 맞출 때까지 멈추고, 맞으면 그 게이트를 지날 때까지 탭을 막아(`Passing`)
  자동으로 통과한다. 이전에는 멈춤이 풀린 뒤 게이트까지 남은 약 `0.15s` 동안 계속 누른 탭이 색을 바꿔 실패했다.
- 기믹 설명(위장 7, 얼음 11, Echo 18, 깜빡임 24): 지난 게이트와 다음 게이트의 중간(`Midway`, 간격의 절반)에서
  `3s` 멈추고 탭을 막은 뒤 시간이 지나면 닫히며 직접 플레이한다. 숨김(21)은 색이 꺼지기 전에 `3s` 멈춘다.
  위장은 색 공개 뒤, 깜빡임은 색 고정 뒤에 멈추는 조건은 그대로다.
- 자동 검증: **EditMode 625/625; PlayMode 309 passed + 1 intentional ignore (310)**(활성 플랫폼 WebGL). Campaign
  시뮬레이션과 Step 10 산출물 8개 해시 Iteration 80 기준과 **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.

## Iteration 80 — Campaign Reorder, Booster Rock and Wind (2026-11-06 KST)

- 기준: `e5c6137`(Iteration 79, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음.
- 캠페인 순서(`StageCatalogAsset.CampaignOrder`, 26개 그대로): 1-3 기본, 4 Shield(`stage-06`), 5 Booster
  (`stage-07`), 6 아이템(`stage-08`, 3색→2색), 7-12 위장·안개·얼음 입문/연습, 13 초록 입문(`stage-04`), 14 3색
  도전(`stage-05`), 15-17 위장·안개·얼음 마스터리, 18-26 그대로. 12번까지 2색. Stage ID·시드는 내용을 따라가고
  표시 번호만 바뀐다. 번호로 판단하던 생성 규칙(예외 색, 초록 입문, 2·3번 구간)은 ID 기준이 됐다. 옮겨진 25개의
  게이트 색·타이밍은 이전과 같고, 아이템 스테이지는 색만 바뀌었다(타이밍 같음). 13·14번은 시작 아이템을 허용한다.
- 저장: `LocalCampaignProgressData.CampaignLayoutVersion`(현재 `1`). 오래된 배치의 저장은 시작할 때 한 번 현재
  순서에서 처음 안 깬 스테이지로 진행 위치를 다시 잡는다. 기록은 Stage ID 그대로. 레거시 PlayerPrefs 가져오기는
  번호 N을 `stage-NN`으로 매핑한다. 보상 마일스톤과 트랙 곡선·로비 테마는 표시 번호(위치) 기준 그대로다.
- 음악: 곡은 스테이지를 따라 이름만 옮겼다(재렌더와 바이트 동일 확인). Booster 스테이지(5)는 새 하드록
  `afterburner`, 아이템 스테이지(6)는 원래 Booster 자리의 시티팝 `neon_boulevard`. 모든 Booster 질주 중 음악은
  `60%`, 3초 반복 바람 소리(`Audio/Sfx/BoosterWind.wav`)가 `0.2s`에 커지고 질주 후 `1s`에 사라진다.
- 자동 검증: **EditMode 625/625; PlayMode 309 passed + 1 intentional ignore (310)**(활성 플랫폼 WebGL). Builder
  2회. Step 10 산출물 5개 해시 **동일**. Campaign 시뮬레이션은 순서가 바뀌어 **새 기준**:
  `MechanicCampaign-SimulationResults.csv` `895ACC31CEC089BDD3DF5AEA1520059974265877A15E7A3EBD43CCECBC504F6F`,
  `.json` `76B957F67CBB8AC6D559D30096C28B34CF62E907D47F06C80D2DAF261F5412F6`,
  `Summary.md` `56637538D14CDB4391A1F3431DA9725610CC89B402A3480940133A940A2FAE10`.
  Product 저장 파일과 Campaign PlayerPrefs 전후 동일(테스트는 실제 저장을 쓰지 않는다).
- 새 무료 웹판 브라우저 계측(소리 0 gain): 새 저장으로 1-5 자동 클리어. 5번에서 바람 반복이 GO에 시작해 약
  `5.5s` 뒤(질주 약 `4.4s` + 페이드) 멈추고, 음악은 한 번 시작해 골인 뒤까지 재생됐다. 4·5번 안내 카드 정상.
- Editor의 기존 개발 저장은 다음 실행 때 한 번 새 순서로 진행 위치가 옮겨진다(의도된 마이그레이션).

## Iteration 79 — Web Stage Music Clock (2026-11-06 KST)

- 기준: `14015cb`(Iteration 78, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음.
- 웹판 스테이지 음악이 뭉개지고 골인 전에 끝나던 문제를 고쳤다. WebGL의 `AudioSource.time`은 재생 속도 변경과
  되감기가 겹치면 실제의 약 2배를 보고했고, 동기화가 이를 보고 매 프레임 되감았다(Stage 2 약 40초에 730번 재시작).
  WebGL에서는 `StageMusicPlayer`가 오디오 장치 시계(`AudioSettings.dspTime`)로 재생 위치를 직접 센다. Android와
  Editor는 그대로 `AudioSource.time`을 쓴다.
- 자동 검증: **EditMode 621/621; PlayMode 308 passed + 1 intentional ignore (309)**(활성 플랫폼 WebGL). Campaign
  시뮬레이션과 Step 10 산출물 해시 **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.
- 새 무료 웹판 브라우저 계측(소리는 0 gain): 새 저장으로 Stage 1 가이드 판 클리어까지 음악 재시작 6번(시작, 가이드
  멈춤 뒤 재개 3번 포함), 재생 속도 `0.92`–`1.0`, 결과 화면까지 음악 유지.

## Iteration 78 — Mechanic Guide Runs (2026-11-06 KST)

- 기준: `bb26bbe`(Iteration 77, 푸시 전) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음.
- 각 기믹의 첫 Stage를 깨기 전에는 가이드 런이 된다: 4 첫 초록 게이트, 9 Camouflage 색 공개 직후, 15 첫 Ice
  게이트, 18 첫 Echo 게이트, 24 Flicker 색 고정 직후에서 멈추고 색을 맞출 때까지 기다린다. 21 Hidden은 색이
  꺼지기 전에 잠시 멈춰 기억할 색을 보여 준다. 6 Shield, 7 Booster, 12 Fog, Echo 저장/사용은 멈추지 않는 알림.
  Stage 1 가이드는 같은 실행기로 옮겼고 동작은 같다. 스테이지 수·저장 구조·판정은 바뀌지 않는다.
- 자동 검증: **EditMode 621/621; PlayMode 307 passed + 1 intentional ignore (308)**(활성 플랫폼 WebGL). Campaign 시뮬레이션과 Step 10 산출물
  해시 **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.

## Iteration 77 — Tutorial (2026-11-05 KST)

- 기준: `387a306`(Iteration 76, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음, 활성 플랫폼 WebGL. Push 없음.
- Stage 1 가이드 런: Stage 1을 깨기 전에는 첫 3개 게이트 `0.15s` 앞에서 화면 이동을 멈추고 색을 맞출 때까지
  기다린다(같은 색이면 안내 후 자동 재개). 멈춘 동안 StageSession은 진행하지 않아 시간·판정·기록·시뮬레이션은
  그대로다. 이후 등급과 Goal을 안내하고, 실패 시 `YOU / GATE` 색을 보여 준다. 첫 클리어 결과에서 CROWNS 카드.
- 새 기믹 안내 카드: 각 기믹의 첫 Stage(4 Green, 6 Shield, 7 Booster, 8 아이템, 9, 12, 15, 18, 21, 24)를
  클리어·시작한 적 없을 때 PreRun에서 한 번. 로비 `?` 버튼은 4장짜리 HOW TO PLAY. 저장 구조 변경 없음.
- 자동 검증: **EditMode 617/617; PlayMode 299 passed + 1 intentional ignore (300)**(활성 플랫폼 WebGL). Builder 2회, Campaign 시뮬레이션과
  Step 10 산출물 해시 **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.

## Iteration 76 — Android App Icon and League Clear Coins (2026-11-05 KST)

- 기준: `a957e4d`(Iteration 75, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음. Push 없음.
- 앱 아이콘: `Tools/Generate-AppIcon.ps1`이 Victory 엠블럼(게이트 포털)으로 적응형 아이콘 배경·전경(432px)과
  레거시 아이콘(512px)을 만들고(`Assets/Game/Art/AppIcon`), `Tools > Color Gate Runner > Apply App Icons`
  (`Editor/AppIconBuilder`)가 Android 적응형·원형·레거시 슬롯에 넣는다(기본 아이콘은 비움). 사용자 승인 범위로
  ProjectSettings는 아이콘 항목만 바뀌었다. 기기 표시 이름은 바꾸지 않았다.
- 경제: 상점 없이 캠페인에서 얻는 양은 코인 6,500(첫 클리어 3,500 + 마일스톤 3,000), Shield 9, Booster
  8이다. 캠페인 뒤에는 수입이 없던 문제를 막기 위해 리그 클리어마다 100코인을 준다
  (`StageClearRewardPolicy.LeagueClearCoins`, 클리어 횟수별 거래 ID로 한 번만). 결과 화면에 LEAGUE COINS로
  보인다. Continue Ticket은 여전히 플레이로 얻을 수 없다(버튼은 0개일 때 숨김).
- 자동 검증: **EditMode `612/612` passed; PlayMode `293` passed, `1` intentional opt-in ignore**(활성 플랫폼 WebGL). Campaign 시뮬레이션과 Step 10 산출물
  해시 **동일**. Android 테스트 APK build succeeded (targetSdk 36; adaptive, round and legacy icons present at every density). Product 저장 파일과 Campaign PlayerPrefs 전후 동일.

## Iteration 75 — Free Web Edition for itch.io (2026-11-05 KST)

- 기준: `1b47939`(Iteration 66, push됨) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음. Push 없음.
- 새 빌드 `Tools/Color Gate Runner/Free Edition/Build WebGL Free Edition`(`CGR_WEB_FREE`, 개발 모드 아님,
  Gzip + 해제 대체, 세로 템플릿)이 전체 캠페인을 정식 저장으로 플레이하는 무료 웹판을 만든다. 빌드 뒤
  `Builds/WebFree/ColorGateRunner-Web-<버전>.zip`으로 묶으며 `*_DoNotShip` 폴더를 빼고 itch.io 제한(파일
  1000개, 파일당 200MB, 합계 500MB)을 확인한다. Playtest 빌드도 같은 공용 WebGL 빌드 경로를 쓴다.
- 무료판에서는 Stage 시작이 Heart를 쓰지 않고(저장된 Heart는 그대로) 로비에 ∞로 보인다. Shop 탭은 빈칸,
  Shop 페이지와 스와이프 이동은 막고, 코인 부족 안내는 "EARN COINS BY CLEARING STAGES."로 바뀐다.
  코인·아이템·이어하기는 게임 안 재화로 그대로다. 다른 판(Android, Editor, Playtest)은 바뀌지 않는다.
- 웹 템플릿에 `autoSyncPersistentDataPath: true`를 켜 Product 저장이 새로고침 뒤에도 남는다.
- 자동 검증: **EditMode `610/610` passed; PlayMode `293` passed, `1` intentional opt-in ignore**(활성
  플랫폼 WebGL). 무료판 빌드 성공: ZIP 75MB, 6개 파일, 가장 큰 파일 62MB. 로컬 브라우저(소리는 0 gain으로
  막고 신호만 측정): 로비 ∞·Shop 빈칸, 새로고침 뒤 계정 선택 유지(저장 지속), Stage 1 실행 중 음악 신호
  RMS 0.10, 콘솔 오류 없음. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.
- 발견한 기존 결함(범위 밖): 활성 플랫폼이 Android이면 설정 화면에 진동 행이 생겨 TERMS/PRIVACY/SUPPORT가
  Apply/Cancel 버튼 아래로 밀린다. 실제 Android 기기에서도 생기므로 Android 출시 전에 고쳐야 한다.
- Fresh-context checker: 기준 5개 PASS. 지적 중 ZIP 충돌을 긴 빌드 전에 확인하도록 고쳤다.
- 저장소 위생(범위 밖): `ColorGateRunner/.utmp/`와 `ColorGateRunner/Data/Plugins/lib_burst_generated.wasm`은
  빌드 중간 파일인데 git에 추적되어 빌드할 때마다 바뀐다.

## Iteration 66 — Neon Chrome for Frontend Frames and Icons (2026-11-05 KST)

- 기준: `6e7f615`(Iteration 65) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음.
  Push 없음.
- 금색은 재화 전용이라는 Iteration 61 규칙에 맞춰, Frontend 틀 5장(TopResourceBar,
  BottomNavigationDock, NavigationSelection, LobbyStageCard, PageHeader)과 Theme01 아이콘 14개의 금색
  장식을 청록 크롬으로 바꿨다. IconCoin은 재화라서 금색을 유지하고 IconContinue는 금색이 없어 그대로다.
  IconPlay의 초록 채움은 주요 행동 색인 Magenta로 바꿨다.
- `Tools/Recolor-NeonChrome.mjs`가 `6e7f615`의 원본을 git에서 읽어 금색 띠 픽셀만 회색을 거쳐
  청록으로 섞는다. 모양·명암·알파·크기·PNG 부가 청크는 그대로이고 같은 결과를 다시 만든다.
- 자동 검증: **EditMode `600/600` passed; PlayMode `292` passed, `1` intentional opt-in ignore**. Builder 각 2회(Scene은 ID 재배치, 재질은 공백만
  바뀌어 복원). 그래픽 캡처 1080×1920, 720×1280. Product 저장 파일과 Campaign PlayerPrefs 전후 동일.
  근거: `Artifacts/Validation/Iteration66/`, `Artifacts/Validation/VisualQC/`.
- Fresh-context checker: 범위·재현성·테스트 PASS. 지적(어두운 청동·옅은 황갈색 잔여, 금색과 청록 사이
  올리브 줄무늬, 두 번 실행 시 결과 변화, PNG 부가 청크 손실, 느슨한 테스트 기준)을 한 번에 고쳤다.

## Iteration 65 — Lobby Illustrations (2026-11-05 KST)

- 기준: `c156e9a`(Iteration 74) 위에서 진행했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음.
  Push 없음.
- 세 로비 테마(Color Courtyard, Neon Garden, Sky Festival)의 Background·가운데 상징물·Ambient 9장을
  사용자가 `LOBBY_ILLUSTRATION_PROMPTS.md`로 ChatGPT에서 만든 그림으로 교체했다. 파일 이름과 경로,
  가져오기 설정은 그대로다. Background는 가운데 9:16(864×1536)을 잘라 941×1672로 맞췄고, 상징물
  (1254×1254)과 Ambient(1024×1536)는 받은 그대로 넣었다. 화면은 비율을 유지해 맞추므로 코드 변경은 없다.
- 첫 상징물 2장(Color Courtyard, Neon Garden)은 빛 주변에 회백색 얼룩과 체크무늬 구멍이 박혀 있어
  사용자가 다시 만들었다. 세 상징물 모두 다시 만든 그림이다.
- 자동 검증: **EditMode `580/580` passed; PlayMode `292` passed, `1` intentional opt-in ignore**. Frontend
  Builder 포함 Builder 각 2회(Scene은 ID 재배치, 재질은 공백만 바뀌어 복원). 그래픽 캡처 1080×1920,
  720×1280. Product 저장 파일과 Campaign PlayerPrefs 전후 동일. 근거: `Artifacts/Validation/Iteration65/`,
  `Artifacts/Validation/VisualQC/LobbyHome-*.png`.
- 사람 판단: 사용자가 세 테마 캡처를 보고 승인했다.

## Iteration 74 — Per-Stage Music Themes (2026-11-05 KST)

- 기준: `14f75d1`(Iteration 73) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock
  없음. Push 없음.
- 26곡이 모두 다른 테마다. 챕터 첫 Stage 8곡(1, 6, 9, 12, 15, 18, 21, 24)은 Iteration 72 곡을 그대로
  두었고 다시 합성해도 바이트 단위로 같다. 하드록 11곡(2, 3, 5, 8, 10, 13, 16, 19, 22, 25, 26)은 키·음계·
  진행·리프·드럼 그루브(punk, gallop, halftime, shuffle, blast, tribal)·기타 음색과 2마디 훅 멜로디가 각각
  다르다. 각 챕터 가운데의 7곡(4, 7, 11, 14, 17, 20, 23)은 완급 조절용 90년대 시티팝이다(FM 일렉트릭
  피아노 메이저7·9 코드, 클린 커팅 기타, 핑거 베이스, 하프타임 그루브).
- 바뀐 파일은 합성 스크립트(`Tools/Music/synth_stage_music.py`)와 WAV 18개뿐이다. `.meta`,
  `StageMusicCatalog.asset`, 런타임 코드는 그대로이므로 게이트 지도와 Booster 동기화는 변하지 않는다.
- 자동 검증: **EditMode `580/580` passed; PlayMode `292` passed, `1` intentional opt-in ignore**. Builder 각 2회(Scene은 ID 재배치, 재질은 공백만 바뀌어
  복원). Campaign 시뮬레이션과 Step 10 산출물 8개 해시 **동일**. Product 저장 파일과 Campaign PlayerPrefs
  전후 동일. 새 곡 18개 모두 클리핑 없음. 게이트 박자 일치는 15곡이 전부, Stage 16 `45/46`, Stage 25 `46/48`, Stage 26
  `51/52`(Iteration 72 Stage 26은 `50/52`). 근거: `Artifacts/Validation/Iteration74/`.
- Fresh-context checker: 기준 4개 PASS. 지적 1건 수정: 시티팝 베이스의 7음 기호가 dom9 코드에서 9음을
  골랐다(Stage 7, 11, 20). 고친 뒤 세 곡만 바뀐 것을 확인하고 관련 테스트를 다시 통과했다. 추가한 동일
  음원 검사는 곡 파일 중복만 잡고 테마가 비슷한지는 판단하지 못한다(사람이 듣고 판단).

## Iteration 73 — Sound Effects and Lobby Music (2026-11-05 KST)

- 기준: `f1e6531`(로비 일러스트 프롬프트 문서) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 효과음 17종을 `Tools/Music/synth_ui_sfx.py`로 합성했다(`Assets/Game/Audio/Sfx`). Frontend와
  Campaign의 모든 Button·Toggle에 Builder가 `UiButtonSound`를 붙이고, 이름과 표면으로 탭·확인·
  뒤로·위험·구매·탭 전환 소리를 고른다. 토글은 켜짐/꺼짐, 누를 수 없는 버튼은 거절음을 낸다.
- 게임 효과음: 카운트다운 3·2·1과 GO, 게이트 판정 PERFECT/GREAT/GOOD, 실패, 클리어, 새 크라운.
- Frontend에서 잔잔한 로비 루프(84 BPM, 91.4초, `synth_lobby_music.py`)가 페이드 인으로 재생되고,
  Stage 실행 시 페이드 아웃, 로드 실패 시 다시 재생된다. Frontend 카메라에 AudioListener를 추가했다.
- 설정의 SFX 행을 보이게 했고 저장된 SFX 볼륨을 적용한다. 클리어 결과 화면이 뜨면 Stage 음악이
  2초 동안 사라진다. 일시정지 중 바꾼 음악 볼륨이 바로 적용되고, 깨진 음악 지도는 소리 없이 진행된다.
- 자동 검증: **EditMode `579/579` passed; PlayMode `292` passed, `1` intentional opt-in ignore**. Builder 각 2회(Scene 변경은 소리 컴포넌트와
  오디오 오브젝트 추가뿐, 재질은 공백만 바뀌어 복원). Campaign 시뮬레이션과 Step 10 산출물 8개 해시
  **동일**. Product 저장 파일과 Campaign PlayerPrefs 전후 동일. 근거: `Artifacts/Validation/Iteration73/`.
- Fresh-context checker 1차 FAIL: 클릭 처리 중 스스로 비활성화되는 버튼(구매 확인, 뒤로, 결과 건너뛰기)이
  거절음을 냈다. 누를 때의 상태로 판단하도록 고치고 Playtest 실행 경로에 로비 음악 페이드를 추가한 뒤
  관련·전체 테스트를 다시 통과했다.

## Iteration 72 — Stage Music (2026-11-04 KST)

- 기준: `d9328b2` (Iteration 71) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- Campaign Stage 26개에 하드록 배경음악을 넣었다. 8개 챕터 테마(기본, 아이템, Camouflage, Fog,
  Ice, Echo, Hidden, Flicker)를 `Tools/Music/synth_stage_music.py`가 표준 라이브러리만으로
  결정적으로 합성한다. 각 곡은 그 Stage의 공칭 게이트 도착 시각에 맞춰 게이트마다 한 마디이고,
  게이트 도착이 마디 첫 박이다. 음원은 `Assets/Game/Audio/Music/StageNN.wav`(Streaming,
  Vorbis 0.5, preload 끔)이고 `Assets/Game/Audio/StageMusicCatalog.asset`이 곡과 게이트 거리·
  시각 지도를 묶는다.
- 재생 위치는 주자 거리로 정한다. Booster처럼 공칭보다 빠르면 곡이 빨리감기처럼 빨라지고
  (pitch 최대 `2.0`), Booster 뒤에는 따라잡을 때까지 빠르게 재생된다. 실패하면 곡이 느려지며
  사라지고, Continue는 다시 시작한 거리의 위치에서 이어지며, Retry와 로비는 정지한다. Pause는
  곡을 멈춘다. 마지막 게이트 뒤에는 원래 속도로 마무리 화음까지 재생한다.
- 설정의 MUSIC 행을 보이게 했다(저장된 Music 볼륨이 곡 볼륨에 적용된다). SFX 행은 효과음이
  없으므로 계속 숨긴다. 저장 형식 변경 없음.
- 자동 검증: **EditMode `563/563` passed; PlayMode `288` passed, `1` intentional opt-in ignore**. Builder 각 2회(Frontend는 fileID 재배치만
  생겨 복원했고, SampleScene에는 `StageMusic` 오브젝트만 추가됐다). Campaign 시뮬레이션과
  Step 10 산출물 8개를 다시 생성했고 해시가 기준과 **동일**하다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다. 근거:
  `Artifacts/Validation/Iteration72/`.
- Fresh-context checker: 7개 기준 모두 PASS, 차단 이슈 없음. 지적에 따라 스트리밍 곡의 위치 지정을
  `Play()` 뒤로 옮겼고 관련 EditMode `14/14`, PlayMode `50/50`을 다시 통과했다.

## Iteration 71 — Booster/Echo PERFECT and Combo Crown Targets (2026-11-04 KST)

- 기준: `64c0816` (Iteration 70) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 결정: Booster 통과와 Echo 통과는 항상 **PERFECT**이고 콤보를 +1 한다. Shield 흡수,
  Continue 보호, GOOD, 실패는 계속 콤보를 끊는다.
- 콤보 크라운 목표는 Stage 난이도를 따른다: Normal `10`, Hard `20`(Stage 11·14·17·23·26),
  VeryHard `30`(Stage 20). 결과 화면과 Collection 행의 크라운 이름이 Stage 목표("20 COMBO")를
  표시하고, Collection 요약은 `COMBO n`으로 합산한다. 저장 형식 변경과 기존 크라운 재평가는 없다.
- 자동 검증: **EditMode `558/558` passed; PlayMode `286` passed, `1` intentional opt-in ignore**. Builder 각 2회(Scene은 fileID 재배치만
  생겨 복원했고 내용은 동일). Campaign 시뮬레이션과 Step 10 산출물 8개를 다시 생성했고 해시가
  기준과 **동일**하다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다. 근거:
  `Artifacts/Validation/Iteration71/`.

## Iteration 70 — Gate Judgment Grades (2026-11-04 KST)

- 기준: `b55d181` (Iteration 69) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 결정: 맞게 통과한 게이트를 마지막 색 전환 시점으로 등급을 나눈다. 전환 없이 맞는 색을
  유지한 통과와 도착 `0.25`초 안의 전환은 **PERFECT**, `0.50`초 안은 **GREAT**, 그보다 이르면
  **GOOD**. PERFECT·GREAT만 콤보를 +1 하고, GOOD과 등급이 없는 게이트(Echo, Booster, Shield
  흡수, Continue 보호, 실패)는 콤보를 끊는다. FULL COMBO는 콤보 히트(PERFECT·GREAT) 1회 이상과
  끊김 0회를 요구한다. 판정 결과(통과/실패)·속도·보상·저장 형식 변경 없음.
- 판정 연출: 러너 앞 불투명 PERFECT 판을 없애고, 상단 HUD와 일시정지 버튼 바로 아래 띠
  (`GateJudgmentFlash`)에 큰 반투명 글자, 등급색 테두리, Additive 잔상과 가로 빛줄기를 띄운다.
  튀어 들어오는 크기와 색으로 등급을 구분하고(PERFECT 금색·마젠타 잔상, GREAT 시안, GOOD
  라벤더) `0.6`초에 사라진다. 일시정지 중에는 멈춘다. 새 셰이더 `ColorGateRunner/UI/AdditiveGlow`와
  생성 머티리얼 `JudgmentGlow.mat`을 추가했다(패키지·ProjectSettings 변경 없음).
- 가독성 감사는 `GateJudgmentFlash`만 예외로 두고 나머지 글자는 그대로 검사한다.
- 자동 검증: **EditMode 553/553 passed; PlayMode `285` passed, `1` intentional opt-in ignore**.
  Builder 각 2회. Campaign 시뮬레이션과 Step 10 산출물 8개를 다시 생성했고 해시가 기준과
  **동일**하다. D3D11 Lobby·Campaign 캡처 2/2(`04E-Combo-Perfect`, `04F-Judgment-Great`,
  `04G-Judgment-Good`).
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다
  (`TestResults.xml`과 Unity 세션 카운터만 변경).
- 독립 read-only checker: 기준 7개 PASS, blocker 없음. should-fix(셰이더 클립 키워드 가드,
  감사 주석 위치, `StopFlash` null 가드)를 한 번의 수정으로 반영했다. 근거:
  `Artifacts/Validation/Iteration70/`.

## Iteration 69 — UI Readability Pass (2026-11-04 KST)

- 기준: `666eef0` (Iteration 68B) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 피드백: 가독성이 떨어지고 텍스트와 UI가 겹친다. 사용자 결정(권장안 승인): 모든 글자는
  불투명 바탕 위에 두고 상태는 바탕 투명도가 아니라 색으로 구분한다. 최소 글자 크기는 Frontend
  540 기준 `14`(Campaign 1080 기준 `28`). 분위기는 기존 네온 팔레트와 아트로 유지한다.
- Theme01 패널·버튼 스프라이트 7종의 안쪽을 불투명하게 바꿨다(모서리 투명은 유지). Journey·
  Collection·크라운·Settings의 반투명 색을 불투명 톤(`PlateMuted`, `TextMuted`)으로 바꿨다.
- Builder가 배경 아트 위에 떠 있던 글자 뒤에 불투명 판을 깐다(Home 테마 이름, Journey 진행,
  Shop 상태, 계정 선택 카드, 하단 탭 이름 띠). Home 스테이지 카드 글자를 카드 유리창 안으로
  옮기고, 스크롤 영역이 하단 탭 아래로 들어가지 않게 했다. `COLOR GATE RUNNER` 제목은 Lobby
  상단 바에 가려져 있었으므로 계정 선택 화면에만 둔다.
- 게임 화면: PERFECT는 함께 켜지고 꺼지는 불투명 판 위에 뜬다. STAGE CLEAR·TAP TO CONTINUE에도
  판을 깔고, 일시정지 버튼의 겹친 `II` 글자를 아이콘 하나로 바꿨다. 판정·속도·보상·저장 변경 없음.
- 새 PlayMode 가독성 감사(`UiReadabilityAudit`)가 Frontend 페이지와 Campaign 상태의 보이는
  글자를 1080×1920 고정 프레임에서 검사한다(불투명 판, 겹침, 최소 크기, 잘림). Lobby 캡처는
  Editor 창 크기와 무관한 1080×1920 렌더 텍스처로 바꿨다(이전 캡처는 326×580이었다).
- 자동 검증: **EditMode 549/549 passed; PlayMode `284` passed, `1` intentional opt-in ignore**.
  Builder 각 2회. Campaign 시뮬레이션과 Step 10 해시 8개가 기준과 **동일**하다. D3D11 Lobby·
  Campaign 캡처 2/2.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다
  (`TestResults.xml`만 변경).
- 독립 read-only checker: blocker 없음. should-fix(감사가 가려진 글자를 건너뛰던 구멍, 밝은
  무스프라이트 판, 테두리 여백, PERFECT 판 회귀 테스트, PLAY 버튼 크기)를 한 번의 수정으로
  반영했다. 근거: `Artifacts/Validation/Iteration69/`.

## Iteration 68B — Collection Stage Records and Replay (2026-11-04 KST)

- 기준: `4c7a4a6` (Iteration 68A) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 결정(권장안 A 승인): Lobby **Collection** 페이지를 `COMING SOON`에서 스테이지 기록
  페이지로 바꾼다. Journey, Rank, Shop, Home은 변경 없음.
- 위쪽 요약 카드: `CROWNS n / 78`, CLEAN · 10 COMBO · FULL COMBO 각 `x / 26`,
  `COMEBACK CLEARS n`. 아래에 카탈로그 스테이지마다 한 줄(제목, 크라운 3칸, 최고 시간,
  최고 콤보)을 세로 스크롤로 보여준다. 이미 저장된 기록만 읽으며 저장 필드·schema·쓰기·
  Coin 보상은 없다. "첫 풀콤보" 같은 날짜 기록은 저장돼 있지 않아 표시하지 않는다.
- 클리어했고 해금 범위 안의 스테이지만 REPLAY를 보여준다. Home PLAY와 같은 실행 경로로
  해당 stage ID의 Authored 실행을 예약하고 그 스테이지 PreRun을 연다(일반 Heart 시작).
  미클리어는 `NOT CLEARED YET`, 잠금은 `LOCKED`. Playtest에서는 REPLAY를 숨기고 거부한다.
- 자동 검증: **EditMode 549/549 passed; PlayMode `282` passed, `1` intentional opt-in ignore**. Frontend Builder 2회. Campaign 시뮬레이션과
  Step 10 해시 8개가 기준과 **동일**하다. D3D11 Lobby 캡처(`LobbyCollection-1080x1920.png`) 포함.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다
  (`TestResults.xml`만 변경).
- 독립 read-only checker: blocker 없음. should-fix 2건(요약의 콤보 목표 하드코딩, 행 수
  불일치 시 Lobby 전체 실패)과 minor 1건(해금 범위 밖 클리어 기록의 REPLAY)을 한 번의
  수정으로 반영하고 회귀 테스트를 추가했다. 근거: `Artifacts/Validation/Iteration68B/`.

## Iteration 68A — Stage Crowns and Campaign Replay (2026-11-03 KST)

- 기준: `4ba41e5` (Iteration 67) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 결정(권장값 승인): 스테이지 크라운 3종 — CLEAN(Continue·Shield 발동 없는 클리어),
  10 COMBO(그 런의 최대 콤보 10 이상), FULL COMBO(CLEAN이면서 Perfect 1회 이상, 콤보 끊김
  없음; Booster·Echo 통과는 유지). 크라운은 클리어한 런에서만 얻고 누적되며 지워지지 않는다.
  Coin 보상 없음. 7전8기(Comeback)는 마지막 클리어 이후 8번째 이상 승인된 시작에서의
  클리어로, 같은 비트 필드에 기록하고 클리어 화면에 `COMEBACK CLEAR · ATTEMPT n`으로만
  보여준다(Collection 표시는 68B).
- Product 저장의 스테이지 기록에 선택 필드 `BestMaxCombo`, `CrownFlags`,
  `AttemptsSinceClear`를 schema 3 그대로 추가했다. 시도 횟수는 Heart/아이템 승인과 같은
  시작 트랜잭션에서 stage ID별로 세며, 같은 attempt ID 재시도는 다시 세지 않는다.
  레거시 PlayerPrefs 형식은 바뀌지 않으며 크라운을 저장하지 않는다.
- Campaign 클리어 화면은 큰 `MAX COMBO`, 크라운 3칸(이번 저장으로 처음 얻은 크라운은 NEW),
  REPLAY(같은 스테이지 PreRun)를 보여준다. REPLAY와 LOBBY는 아래 줄에 나란히 놓인다.
  판정, 속도, 보상, 경제, Heart 정책은 변경 없음.
- 자동 검증: **EditMode 544/544 passed; PlayMode `280` passed, `1` intentional opt-in
  ignore**. Builder 각 2회. Campaign 시뮬레이션 Summary/JSON/CSV와 Step 10 다섯 해시가
  기준과 **동일**하다. D3D11 Step9C 캡처 2/2.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다
  (`TestResults.xml`만 변경).
- 독립 read-only checker: blocker 없음. should-fix 4건(Product 측 크라운 축소 방지, 저장되지
  않은 크라운·Comeback·BEST 표시)을 한 번의 수정으로 반영하고 회귀 테스트를 추가했다.
  근거: `Artifacts/Validation/Iteration68/`.

## Iteration 67 — Perfect Switch Combo and Gate Timeline Export (2026-11-03 KST)

- 기준: `50cc5d5` (Iteration 64C) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 결정: 레이스 순위는 깨끗한 클리어 시간이 스테이지마다 고정이라 실력과 무관하므로
  표시하지 않는다. 대신 게이트 도착 `0.25`초 안의 마지막 색 전환을 **Perfect Switch**로
  세는 콤보를 HUD(`n COMBO`, `PERFECT` 0.45초)와 클리어 결과(`MAX COMBO n`)에 보여준다.
  탭 없이 통과·Echo·Booster는 콤보 유지, 이른 전환·Shield·Continue 보호·실패는 초기화.
  속도, 판정, 보상, 저장은 변경 없음. 라이벌은 분위기 연출로만 남는다.
- `Tools/Color Gate Runner/Export Gate Timeline For Music`이 스테이지별 깨끗한 무아이템
  런의 게이트 도착 시각을 `Artifacts/GateTimeline/StageNN.csv`(간격·참고 BPM 포함)와
  `.mid`(120 BPM 실시간, 색별 음높이)로 내보낸다. 시각 0은 GO다.
- 자동 검증: **EditMode 527/527 passed; PlayMode `276` passed, `1` intentional opt-in ignore**. Builder 각 2회.
  Campaign 시뮬레이션 Summary/JSON/CSV와 Step 10 다섯 해시가 기준과 **동일**하다.
  D3D11 Step9C 캡처에 `04E-Combo-Perfect`를 추가했다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
- 독립 read-only checker: 기준 1–6 PASS, blocker 없음. should-fix 2건(실패 시 PERFECT 잔상, MIDI 근접 노트 시각 밀림)을 한 번의 수정으로 보완하고 회귀 테스트를 추가. 근거: `Artifacts/Validation/Iteration67/`.

## Iteration 64C — Curved Rival Lanes and Zoned City (2026-11-03 KST)

- 기준: `308625a` (Iteration 64B) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- 사용자 피드백: 라이벌 트랙이 플레이어와 같은 커브일 필요가 없고, "나 외에도 누군가
  달리고 있다"는 느낌이 목적이다. `RivalTrackLayout`이 Stage 시드로 라이벌마다
  다른 좌우 흔들림(경로 중심에서 8.5–16.5)과 높낮이(0–4.5)를 만든다. 출발선에서는
  ±9·높이 0에서 40m에 걸쳐 섞여 들어간다. 차선 메시, 러너, 측면 게이트, 러너 진행
  방향이 이 형태를 따른다.
- 빌딩은 트랙에 맞춰 배치한다. 라이벌 단계에서 각 빌딩은 플레이어 도로와 라이벌
  차선 사이의 틈(틈이 충분할 때 45%)이나 라이벌 차선 바깥에 서며, 빌딩 깊이 전체에서
  차선과 겹치지 않는다. Stage 1 배치는 이전과 같다.
- 라이벌 진행 계산, 순위, 판정, 보상, 저장, 카메라, Stage 데이터, Builder, Scene은
  변경 없음.
- 자동 검증: **EditMode 514/514 passed; PlayMode `272` passed, `1` intentional opt-in ignore** (신규 EditMode 4,
  PlayMode 라이벌 차선 테스트 갱신). Builder 각 2회(변경 없음 확인 후 Scene 복원).
  D3D11 Step9C 캡처 `2/2`에서 서로 다른 커브의 측면 차선과 차선 사이/바깥 빌딩을
  확인했다. Core 변경이 없어 시뮬레이션은 재실행하지 않았다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
  근거: `Artifacts/Validation/Iteration64C/`.

## Iteration 64B — Rival Race Presentation (2026-11-03 KST)

- 기준: `0c36ecc` (Iteration 64A) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- Campaign Stage 2부터 `RivalRaceView`가 같은 Spline을 ±9로 오프셋한 측면 차선 2개,
  기본 실루엣의 라이벌 러너(Magenta / Signal 셸), 차선당 게이트 3개(통과 Signal,
  실패 Danger 점멸)를 표시한다. HUD에 `1ST / 3` 순위 칩, 클리어 결과에
  `RACE <순위> PLACE` 줄을 추가했다. 라이벌 단계에서는 빌딩 최소 거리를 16으로 넓힌다.
  카메라, 판정, 보상, 저장, Stage 데이터는 변경 없음. Stage 1은 라이벌이 없다.
- 자동 검증: **EditMode 510 passed; PlayMode 272 passed + 1 existing ignored**
  (신규 EditMode 1, PlayMode 4). Builder 각 2회. Campaign 시뮬레이션 520행의
  Summary/JSON/CSV와 Step 10의 다섯 해시가 기준과 **바이트 단위로 동일**하다.
  D3D11 Step9C 캡처 `2/2`에서 측면 차선·게이트·순위 칩을 확인했다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
- 독립 read-only checker: 기준 1–6 PASS, 7 테스트 공백 지적(재시작·단계 선택 숨김·Stage 1 빌딩 거리 테스트 추가로 보완), blocker 없음. 근거: `Artifacts/Validation/Iteration64/`.

## Iteration 64A — Rival Race Core Simulation (2026-11-03 KST)

- 기준: `dfa10cf` (Iteration 63) 위에서 구현했다. Unity `6000.5.1f1`, 최종 검증 시
  Editor 종료/lock 없음. Push 없음.
- `Core/RivalRaceSimulation`을 추가했다. 표시 전용 Ghost 라이벌 2명이 Stage 2부터
  스테이지의 원시 게이트 간격과 비부스터 기본 속도를 따라 각자의 거리로 달린다.
  시드에서 파생한 게이트 성공 여부와 속도 계수는 결정적이고 프레임률과 무관하며,
  실패 시 0.9초 동안 45% 속도로 휘청인다. 플레이어 순위는 완주 시간, 그다음 거리로
  계산한다. 아직 어떤 Scene이나 런타임도 이 클래스를 호출하지 않는다(64B에서 연결).
- 자동 검증: **EditMode 509 passed; PlayMode 268 passed + 1 existing ignored**
  (신규 EditMode 7). Product 저장 파일과 Campaign PlayerPrefs는 전후 동일하다.

## Iteration 63 — Settings and Placeholder Page Refresh (2026-11-03 KST)

- 기준: `f63c82c` (Iteration 62) 위에서 구현했다. primary 프로젝트, Unity
  `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음. Push 없음.
- 공용 Settings 패널(Frontend·Pause)을 네온 Modal, AUDIO/GAMEPLAY/ABOUT 구역,
  Magenta 스위치 토글, 주요/보조 텍스트 버튼으로 다시 구성했다.
  `SettingsAvailability`가 작동하지 않는 행을 숨긴다: 알림(전송 없음)과 Music/SFX
  (오디오 자산 없음)는 숨김, 진동은 Android/iOS에서만 표시, 릴리스 빌드는 주소 없는
  링크를 숨긴다. 숨긴 행도 불러온 값을 유지하므로 Apply가 저장 값을 보존한다.
  ABOUT은 빌드 버전과 Chakra Petch SIL OFL 1.1 표기를 보여준다. Rank는
  `OFFLINE` 상태 카드, Collection은 `COMING SOON` 카드로 정직하게 안내한다.
  저장 스키마·키·규칙은 변경 없음.
- 최종 자동 검증: **EditMode 502 passed; PlayMode 268 passed + 1 existing ignored**
  (신규 EditMode 5, PlayMode Rank 문구 1건 갱신). 집중 11/11. Builder 각 2회 이상.
- 커밋하지 않은 임시 Editor 캡처 스크립트로 Settings 렌더 2장(데스크톱/휴대기기
  가용성)을 확인하고 링크 행 높이를 고쳤다. Rank 페이지 렌더와 실제 Frontend 흐름의
  화면 확인은 대화형 Editor가 필요해 **NOT VERIFIED**다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
  변경은 Unity TestResults.xml, 세션 기록, `PT_Run` EditorPrefs뿐이다.
- 독립 read-only checker: 기준 1, 3, 5 PASS·2, 4, 6, 7 참고 사항, blocker 없음(릴리스 링크 상태 문구 간격은 수정함). 근거: `Artifacts/Validation/Iteration63/`.

## Iteration 62 — Neon Night City Gameplay Environment (2026-11-03 KST)

- 기준: `d963031` (Iteration 61) 위에서 구현했다. primary 프로젝트, Unity
  `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음. Push 없음.
- Campaign 배경만 밤 도시로 바꿨다. `NeonNightSky` 스카이박스(그라디언트·지평선
  마젠타 글로·두 겹 스카이라인·창문 점), `NeonCityFacade`(UV 없이 그리는 결정적
  창문 격자, 거리 Fog 지원), Trilight 밤 환경광, 달빛 조명 색, 반사 0.6, 선형 거리
  Fog 60–230, 빌딩 글로 Cyan/Magenta 교대. 도로·게이트·러너·Fog 기믹·Bloom·규칙·
  Stage 데이터·저장은 변경 없음.
- 최종 자동 검증: **EditMode 497 passed; PlayMode 268 passed + 1 existing ignored**
  (신규 EditMode 3). 집중 9/9. 세 Builder 각 2회 이상 후 전체 회귀 통과, Missing
  Script/Reference 없음. Package·ProjectSettings 변경 없음.
- D3D11 비헤드리스 batchmode Step9C 캡처 `2/2`(19장)로 밤 화면을 직접 확인했다.
  첫 캡처에서 러너 셸이 어두워지고 날개 윤곽이 사라져 환경광·반사를 한 번 올렸다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
  변경은 Unity TestResults.xml, Unity 세션 기록, Performance Test Framework
  `PT_Run` EditorPrefs뿐이다.
- 독립 read-only checker: 기준 1–7 PASS(5–7은 참고 사항 포함), blocker 없음. 근거:
  `Artifacts/Validation/Iteration62/`.

## Iteration 61 — Neon Night UI Design System Foundation (2026-11-03 KST)

- 기준: `d527300` (itch.io Playtest v0.1 후보 커밋) 위에서 구현했다. primary 프로젝트,
  Unity `6000.5.1f1`, 최종 검증 시 Editor 종료/lock 없음. Push 없음.
- 사이버펑크·밤·네온 방향의 첫 단계다. `NeonUiPalette` 토큰, OFL Chakra Petch
  SemiBold 공통 글꼴, 결정적으로 생성한 모서리 깎은 네온 9-slice 표면 7종,
  UI 텍스트/비스킨 채움/런타임 상태 색의 토큰화, Home 카드 행·탭 선택 프레임·
  버튼 라벨 여백·보상 행 스킨을 적용했다. 게임 판정색·규칙·데이터·저장은 변경 없음.
- 최종 자동 검증: **EditMode 494 passed; PlayMode 268 passed + 1 existing ignored**
  (신규 EditMode 6). 집중 7/7. 세 Builder 각 2회(최종 패스 포함) 후 전체 회귀 통과,
  Missing Script/Reference 없음. Package·ProjectSettings 변경 없음.
- D3D11 비헤드리스 batchmode 캡처: Campaign Step9C `1/1`(19장). Frontend 페이지
  캡처는 `WaitForEndOfFrame` 제약으로 batchmode에서 실행 불가해 **NOT VERIFIED**다.
- Product 저장 파일과 `ColorGateRunner.*` Campaign PlayerPrefs는 전후 동일하다.
  변경은 Unity TestResults.xml·세션/테스트 실행 기록뿐이다.
- 독립 read-only checker: 기준 1–7 PASS, blocker 없음. 근거:
  `Artifacts/Validation/Iteration61/`.

## itch.io Playtest v0.1 (2026-10-06 KST, `d527300`로 커밋됨)

보고서에 따라 구현을 진행하라는 요청으로 만든 **로컬 검토 후보**였으며 2026-11-02에
사용자 요청으로 커밋했다. 아래
Authoritative Baseline의 승인된 HEAD를 대체하거나 게시 준비 완료를 의미하지 않는다.

- 작업 HEAD: `e48448d81a28f2f4338a4f2db79e6f74ca90391a`; primary 프로젝트,
  Unity `6000.5.1f1`; 최종 검증 시 Editor 종료/lock 없음. 커밋·push·업로드 없음.
- `CGR_PLAYTEST`를 해당 WebGL Player 빌드에만 적용한다. Boot/Frontend/Campaign을
  재사용하며 경로는 `01 → 02 → 03 → 06 → 09 → 21 → 18 → 24`다.
  계정/경제/보유 아이템 선택을 우회하고 한 번의 Retry와 명시적 8단계 종료를 제공한다.
- 진행·임시 Product 그래프는 메모리에만 존재한다. 기존 Stage 정의/시드/속도/판정과
  Stage 6 제공 Shield는 유지한다. Stage 1 안내, 실제 색 불일치 설명, 입력+일치+클리어
  기준의 온보딩 관찰 상태를 추가했다.
- 최종 자동 검증: **EditMode 488 passed; PlayMode 268 passed + 1 existing ignored**.
  집중 검증 EditMode 8 / PlayMode 3 passed. Builder 각 2회와 빌드 후 전체 회귀 통과.
  Campaign 520행 재실행의 세 해시는 아래 승인 기준과 동일하다. Step 10은 관련
  Core/Experiment/입력 시뮬레이션 변경이 없어 재실행하지 않았다.
- 재질 재검증이 기존 렌더 순서 3010을 3000으로 되돌리는 디스크 변경을 발견했다.
  additive 재질의 `_QueueOffset=10`을 Builder에 명시해 기존 3010을 유지한다.
  URP shader validation 테스트 및 실제 WebGL 데이터의 재질 추출로 확인했다.
  이전 후보에도 실제 패킹된 재질 순서는 3010이었다.
- 배포 후보: `Artifacts/Release/ColorGateRunner-Playtest-v0.1-rc2.zip`.
  ZIP **55,768,566 bytes**, 추출 **56,497,772 bytes / 6 files**, 최대 파일
  **42,101,094 bytes**. SHA-256:
  `637400d268db60be9be9a45ebc0c420dab0adc60e489f0e48794aaf1dd391cf5`.
  루트 index.html/상대·대소문자 경로/ZIP 무결성/크기 제한 검사 통과.
- Product 저장·백업은 사전 파일 해시와 수정 시각이 동일하고 Campaign PlayerPrefs도
  정확히 일치한다. Package 변경 없음. 최종 ProjectSettings는 승인 해시와 일치한다
  (작업 시작 시 허용된 package-managed WebGL define 차이는 사라짐).
  빌드가 생성한 URP global settings/burst 파일은 근거 폴더에 보관 후 작업 전 상태로
  복원했으며, generated YAML의 줄 끝 공백만 정규화했다.
- Chromium WebGL 2.0 로컬 브라우저에서 진입/색 전환/실패/Retry/Pause/Resume 확인.
  첫 후보에서 390×844·844×390 레이아웃과 로컬 iframe/전체 화면 크기 전환도 관찰했다.
  실제 itch.io, 실물 모바일, 브라우저 전체 경로 클리어, 오디오, 포커스 이탈은
  **NOT VERIFIED**. FSR upscaling 미지원과 persistentDataPath 동기화 경고가 남는다.
- **UGS SDK·동의 UI·외부 이벤트 전송은 아직 구현/연결되지 않았다.** 동의 기반 UGS
  방향은 승인됐으나 프로젝트 ID/환경과 개인정보 문의·게시 URL이 미정이다.
  현재 화면은 수집 미연결을 명시한다. 초심자 관찰/전체 브라우저 경로/UGS 실제 수신/
  배포 페이지 자료/제한 itch.io 검증은 외부 초대 전 남은 작업이다.
- 명세: `ITCH_IO_IMPLEMENTATION_SPEC.md`; 페이지·개인정보 초안:
  `ITCH_IO_PLAYTEST_PAGE.md`; 상세 검증: `TEST_HISTORY.md`의 같은 후보 항목.
  실행 근거: `Artifacts/Validation/ItchPreparation-20261004/` 및
  `Artifacts/Validation/ItchPreparation-20261006/`. 마지막 폴더의
  `final-source.json`, `packed-materials.json`, `browser-evidence.json`을 사용한다.
- 독립 checker는 한 차례 보완 후 **로컬 구현 후보 PASS**를 반환했다. 전체 브라우저
  완주·서비스/기기·주관적 UX 검토 및 외부 게시 승인은 이 판정에 포함되지 않는다.

## Authoritative Baseline

This section is the single current baseline for routine Codex iterations.
Historical counts and hashes later in this file remain evidence for their own
iterations but do not override this section.

### Repository

- Snapshot date: `2026-11-03` KST
- Branch: `main`
- Implementation base: the Iteration 68A commit `feat: add stage crowns and campaign replay`,
  whose parent is `4ba41e51bacafcd9712b75547dba3ee88f33cc73`
  (`feat: add perfect switch combo and gate timeline export`).
- Iteration 63 commit: `dfa10cfb69f61ac373c227cef6e79d6776ebcb95`
  (`feat: refresh settings and placeholder pages`).
- Iteration 62 base parent: `d9630311d18785aa87f49e483029538d969f49aa`
  (`feat: establish neon night UI design system`).
- Previous base: `d527300040de2a7b208f068a5d12939ae6ad3d92`
  (`feat: add itch.io playtest build candidate`).
- Earlier implementation base: `d937b3a242b04df6d211de7dfe92abfca82175e3`
  (`feat: refine lobby shell and page visuals`)
- The handoff-documentation commit is intentionally newer than the
  implementation base; it changes only this file and `Docs/GPT_HANDOFF.md`.
- Inspection began with no untracked files. Git reported
  `ColorGateRunner/ProjectSettings/ProjectSettings.asset` as modified because
  of working-copy normalization, but its Git blob and working bytes were
  identical and its approved SHA-256 remained exact.
- Push status: not pushed

### Current product state

Iteration 23 supersedes the active Iteration 22 gameplay/economy baseline
below where their contracts differ.

- Current completed iteration: **Iteration 68A — Stage Crowns and Campaign
  Replay**. Clears earn CLEAN, 10 COMBO and FULL COMBO crowns and a Comeback
  flag; the Campaign clear result offers REPLAY.
- Iteration 67 — Perfect Switch Combo and Gate
  Timeline Export. The HUD counts Perfect Switches as a combo and the clear
  result shows the run's max combo; rivals no longer show a rank.
- Iteration 64 — Rival Race (64A Core, 64B presentation, 64C curved lanes and
  zoned city). Campaign Stage 2+ shows two presentation-only Ghost rivals.
- Iteration 63 — Settings and Placeholder Page Refresh.
  Settings shows only controls the build can honor; Rank states that online
  ranking is unavailable.
- Iteration 62 — Neon Night City Gameplay Environment.
  Campaign renders a night skybox, Trilight night ambient, linear distance fog
  and lit-window city facades; gameplay rules and road/gate/runner art are
  unchanged.
- Iteration 61 — Neon Night UI Design System Foundation.
  Shared UI chrome now uses `NeonUiPalette`, Chakra Petch SemiBold and the
  chamfered neon 9-slice surface set; lobby illustrations, top resource rail
  and bottom dock art are unchanged until Iterations 63–64.
- Previous iteration: **Iteration 60 — Lobby Shell and Page Visual Refinement**.
- The persistent Lobby shell now uses authored transparent top-resource,
  bottom-dock and selected-tab art. Home keeps only the live Guest, Coin,
  Heart, theme, current Stage, mechanic, progress and Play hierarchy visible;
  legacy inventory and local milestone counters remain bound under an inactive
  read-model root so existing state ownership is unchanged. Stage copy wraps
  and truncates within the authored lower card at both `1080 x 1920` and
  `720 x 1280`.
- Shop and Journey use dedicated portrait environments plus a shared authored
  header treatment. Journey retains its automatic read-only chapter,
  milestone and League behavior. Rank and Collection use distinct illustrated
  environments with only truthful `COMING SOON` copy; no leaderboard,
  collection, store or reward behavior is simulated.
- Campaign Red, Blue and Green share one imported Cyber Orb with 22 fixed
  Builder-bound visual parts. Red deploys a broad shoulder/ram power kit,
  Blue a compact guard/crest stable kit and Green a wide blade/tail wing kit.
  A `0.24s` transition first folds shared parts inward, then deploys the
  exclusive target kit with restrained overshoot. The logical color,
  emissive material and HUD still change in the input call; a new tap retargets
  from the current pose with no queue. Yellow, Purple and Cyan intentionally
  retain the Blue/base geometry. Player root, collider, Rigidbody, Spline
  pose, steering and judgment ownership are unchanged.
- Theme 1 now uses image assets for the five bottom destinations, Profile,
  Coin, Heart, Shield and Booster. The former runtime rectangle navigation
  glyphs and HUD Shield text symbol are gone. Existing action icons plus the
  seven reusable panel/button/card surfaces were regenerated at higher source
  resolution while all player copy remains live text. Rank and Collection are
  included as illustrated, truthful Coming Soon destinations; the Developer
  Console remains Editor-only.
- Campaign Clear now holds its victory beat for `2.35s`, then crossfades to
  the reward page over `0.25s`. A tap is ignored during only the first `0.1s`;
  afterward it skips through the same crossfade rather than hard-cutting.
  The Builder-owned celebration uses 36 fixed sprite streaks as three
  staggered 12-spark radial bursts, three fixed flash cores and two restrained
  emblem echoes. No runtime object is instantiated and the committed reward
  transaction remains unchanged.
- Journey now contains three fixed chapters: Color Courtyard, Neon Garden and
  Sky Festival. They unlock at automatic Lobby milestones `0 / 6 / 12`, expose
  one `IN USE` state, and let the user persist any unlocked Lobby theme through
  the existing atomic Product save transaction. Legacy schema-3 saves without
  a selection resolve the latest unlocked chapter without a load-time write.
- The Home hero was opened up around the layered theme artwork. All three
  chapters own complete Background / Midground / Ambient art. The visible
  foreground is limited to top resources, theme/progress, the current Stage
  summary and its Play action; inventory and milestone read models remain
  bound under an inactive compatibility root.
- Journey inserts a League terminal immediately after the last live even-Stage
  milestone. It stays locked until every Stage in the current Catalog is
  cleared, then Home enters deterministic endless replay. Each profile sees a
  stable shuffle bag containing every authored Stage once per cycle with no
  immediate repeat across a cycle boundary. Adding a new uncleared Catalog
  Stage automatically returns that profile to authored progression.
- Frontend now uses a persistent five-tab portrait navigation shell inspired
  by the approved information hierarchy while retaining original Theme 1 neon
  surfaces and transparent destination images. Its horizontal order is Shop,
  Rank, Home, Journey and Collection. Tabs and one-page swipes navigate the same pager;
  vertical Shop and Journey scrolling does not page. Back from every non-Home
  destination returns Home without opening the app-exit confirmation.
- Shop is a vertical catalog preview over the shared Profile / Coin / Heart /
  Settings shell. It displays all 11 existing local catalog definitions as
  five Featured Circuit bundles followed by six Coin Vault packs. Original
  pouch, box, chest and overflowing-cart art communicates package volume, and
  semantic reward glyphs expose exact quantities. Every action is disabled and
  labeled `STORE OFFLINE`; no price, order, receipt or grant is simulated.
- Journey is a read-only vertical projection of the existing automatic Lobby
  milestone policy. It shows 18 Stage 2-36 rewards as Collected, Current,
  Locked or Coming Soon; with the current 26-Stage Catalog, Stages 28-36 are
  truthful Coming Soon entries. It has no Claim action and writes no save.
- Rank and Collection open as truthful `COMING SOON` pages rather than disabled
  tabs or simulated systems.
- Campaign Clear now opens with an original victory emblem and a fixed-pool
  firework celebration, then reveals only rewards actually committed by the
  clear transaction. Campaign failure first presents Continue cost and the
  number of gates that remain after the failed gate is retired, routes Give Up
  through an explicit consumed-Heart confirmation and an extensible
  consequence queue, then offers Try Again or Lobby. Leaving does not spend a
  second Heart; Experiment retains its prior diagnostic result flow.
- Successful Campaign attempts still refund their consumed finite Heart in the
  atomic Product clear transaction, but the reward page does not label or
  animate that invisible Stage-start spend as `HEART RETURNED`.
- The project now routes development through an explicit environment preflight,
  risk-sized deterministic validation and fresh-context QC. The generic
  `quality-graph` Skill is versioned under `Tools/AgentSkills`; this repository
  supplies only its Unity/project adapter under `Tools/QualityGraph`.
- The development Experiment panel now exposes `SPLINE TRACK LAB`. It runs a
  separate horizontal S-curve with distance-to-Spline runner, camera, gate and
  Goal poses plus a generated dark-alloy/Cyan road mesh. Restart and Exit remain
  Lab-local and non-persistent.
- All 26 active Campaign Stages use one scalar-distance Spline route rather
  than the legacy recycled straight-track placement. The same evaluated pose
  owns runner, chase camera, pooled gates, Goal, Fog curtain and preplaced Ice
  runway. Stages 1–7 introduce restrained horizontal curvature, Stages 8–13
  broaden it and begin gentle elevation, and Stages 14–23 increase both within
  a stable world-up/no-roll portrait camera contract.
- The Campaign chase camera follows the current Spline rotation with a `0.3s`
  half-life and bounded `24°` yaw / `16°` pitch lag. It never banks or rolls,
  snaps at Stage start, Retry and Continue, and freezes with Pause or failure.
- The runner root, Rigidbody and judgment pose remain exact Spline consumers.
  A presentation-only visual child anticipates the upcoming tangent with a
  bounded `10°` yaw and `6°` lean so the vehicle visibly steers through turns
  without creating a second movement authority.
- Campaign road presentation uses original rough, non-emissive asphalt maps
  while retaining the Cyan emissive edge. Ice uses a fixed 50-slot pool of
  runtime Spline-sampled ribbon meshes with original smooth, non-emissive ice
  maps, so active intervals follow the same horizontal and vertical route pose.
- Campaign Fog presents ten fixed Blender-authored low-poly banks across a
  closer speed-adaptive 18–32 unit Spline band. A fixed 24-wisp / 96-rain
  camera-following particle budget and a Fog-scoped darker color-adjustment
  volume share the authored `0.5s` fade-in, Stage-owned `5 / 6 / 7s` hold and
  `0.5s` fade-out lifecycle. No volumetric rendering is introduced.
- Every pooled gate owns one inactive, collider-free Echo membrane using the
  shared protection-field hex shader. Only a visible unresolved Echo Provider
  exposes it in its assigned color; crossing hides it and transfers that exact
  hue to the player Echo shell.
- Campaign track presentation is one generated seven-unit two-submesh road for
  the full attempt. The legacy six-segment straight pool remains available only
  to existing Experiment paths and is inactive during Campaign play.
- The 24 imported Theme 1 city body/glow pairs form a deterministic fixed pool.
  They follow the Spline outside a 7.5-unit minimum lateral clearance, recycle
  behind the runner without allocation and continue through the Goal buffer.
- Unity Editor exposes `Tools > Color Gate Runner > Developer Console`.
  It can open any authored Stage as a non-persistent cheat launch, unlock
  Campaign through a selected Stage, reset Campaign separately from Economy,
  and set/reset Coins, Shields, Boosters, Continue Tickets, Hearts and timed
  unlimited Hearts. Edit Mode writes the same local Product save before Play;
  Play Mode uses the active AppRoot session and refreshes visible Product UI.
  The tool is Editor-only and is absent from player builds.
- Unity IAP `5.4.2` and its Unity Services Core `1.18.0` dependency are
  installed. Android billing mode is Google Play and the Android application
  ID is `com.wscho.colorgaterunner`.
- The package baseline does not yet initialize a store, fetch provider prices,
  process orders, validate receipts or grant paid rewards. The Shop displays
  only the local catalog preview and a truthful disconnected state.
  Firebase exists only as a console project; no Firebase Unity SDK or mobile
  configuration file is present in the repository.
- Runtime flow: `Boot(0) -> Frontend(1) -> SampleScene/Campaign(2)`.
- Frontend contains first-run Account Choice and the consolidated Campaign
  Lobby. It does not contain an AppRoot and reuses the Boot-created persistent
  AppRoot.
- `LocalProfileData.AccountChoiceCompleted` owns onboarding completion.
  Returning users enter Lobby directly; schema-1 saves without the field
  intentionally see Account Choice once.
- Google integration remains absent, its release action is hidden, and no
  linked state is simulated.
- Frontend and Gameplay Pause share Notifications, Music, SFX, Vibration, and
  configured-link Settings. Master Volume remains hidden and applies through
  `AudioListener.volume`; Music and SFX persist without a current content-
  specific target. Vibration gates the existing Booster haptic call.
- Frontend reads Campaign progression through the schema-2 Product save and
  queues one non-persistent stable-ID launch request. Production entry opens
  the existing PreRun/item selection without briefly showing Campaign Lobby;
  direct/development/fallback entry retains that Lobby.
- Campaign records the non-persistent Frontend entry origin only after it
  consumes that launch request. PreRun Back returns that path directly to the
  Frontend Lobby; direct/development entry still returns to Campaign Lobby.
- Every Settings toggle uses one `StateLabel`, showing exactly one of `ON` or
  `OFF`. Frontend and Pause share the same immediate synchronization path.
- Gameplay Pause freezes attempt time, movement, input, judgment, recycling,
  mechanic timing, camera feedback, and registered attempt VFX. Resume,
  existing Retry, Settings, and confirmed Frontend Lobby return are available.
- Product save schema 3 owns Profile, Settings, stable-ID Campaign records,
  Economy inventory, Hearts, timed unlimited Hearts, Continue Tickets,
  transaction IDs, Starter entitlement and Lobby milestone presentation.
- On the first production launch after upgrade, legacy device-wide Campaign
  PlayerPrefs are imported once. Legacy keys remain for rollback safety, but
  Product save becomes the runtime write authority.
- First clear grants `100 / 200 / 500` Coins for Normal / Hard / Very Hard
  difficulty. Stages 11, 14, 17, 23 and 26 are Hard; Stage 20 is Very Hard; the other
  authored Stages are Normal. Every even first-cleared Stage up
  to Stage 36 applies one automatic Lobby milestone and an idempotent reward.
  The starter reward policy grants 200 Coins, or 300 at every third milestone,
  plus alternating Shield/Booster inventory.
- Frontend Lobby uses three clear zones: persistent Profile / Coin / Heart /
  Settings controls at the top, the current Lobby theme and upgrade progress
  as the dominant center, and one compact Stage card with one primary `PLAY`
  action at the bottom. Shield/Booster stock and pending-milestone counters are
  not player-visible; their existing bindings live under inactive
  `LobbyProgressReadModelRoot`. Duplicate `LOBBY` and account labels are
  removed. Hearts show capacity, next-recharge time, or timed-unlimited
  remaining time from Product state. Frontend-origin result actions return
  there; direct/development entry keeps Campaign Lobby fallback.
- The same top wallet and five-tab bottom navigation remain present on Home and
  Shop. The Home theme, progression and Stage card are hidden while Shop is
  active; Shop content uses a fixed portrait ScrollRect and resets to its top
  when opened.
- Theme 1 `COLOR COURTYARD`, Theme 2 `NEON GARDEN` and Theme 3 `SKY FESTIVAL`
  each use complete Builder-owned Background / Midground / Ambient portrait
  art. Six automatic Lobby milestones drive each chapter's progression state.
  The presentation never blocks UI input and does not change the top wallet,
  Stage card or `PLAY` navigation.
- Theme 1 Campaign presentation now uses Blender-authored FBX art for the
  spherical cyber vehicle, modular Left / Right / Top gate, neon track, city
  backdrop and Goal portal. Editable `.blend` source and Runner / Gate /
  Environment UV/PBR map families are checked in. A fixed six-view,
  five-fragment-per-view deterministic pool shows a `0.3s` gate break without
  physics allocation or gameplay authority. The existing URP profile enables
  restrained four-iteration Bloom and the Builder enables camera
  post-processing. Gate/Track pool sizes, collision, judgment, Stage data and
  balance are unchanged.
- The Theme 1 runner now reads from the chase camera as a spherical cyber
  vehicle with a smooth non-emissive glass-like hull that follows the current
  runner color, plus an HDR current-color rear panel, wide bumper, side fins,
  twin exhausts, twin chevrons and a rear light bar. Generated gameplay colors
  use HDR emission while the glass hull and Track dark alloy remain
  non-emissive. The
  existing four-iteration Bloom profile uses threshold `0.8`, intensity
  `0.85` and scatter `0.58`; no render package or ProjectSettings changed.
- The gameplay color-order HUD is a bottom-center horizontal strip. Current
  color is centered, next is emphasized to the right and previous is reduced
  to the left; two-color stages use a balanced current/next pair and additional
  colors reuse the same fixed slots without covering left-hand curves.
- Gates and the bottom color-order HUD now share six distinct transparent,
  non-elemental Theme 1 image emblems. The former Unicode shape glyphs are no
  longer used by normal runtime presentation. Echo retains its concise
  mechanic marker, while Hidden and Flicker have no explanatory marker or text.
- Frontend and Campaign now share an original Theme 1 UI skin: seven genuine-
  alpha 9-slice panel/button/chip sprites, eleven semantic resource/action/
  Profile icons and five navigation images. Builder-created panels retain
  their existing hierarchy and text; buttons gain primary, secondary and danger surfaces plus distinct hover,
  press, selected and disabled states. Coin/Heart, Play, Settings, Back,
  Pause, Shield, Booster, Continue and Retry cues are non-blocking Images.
  Navigation, Product state, economy values and gameplay authority are
  unchanged.
- Theme 1 FBX instances now discard the importer-supplied `X -90°` root
  rotation and use normalized local position, rotation and scale. The Track
  therefore remains approximately 7 units wide, below 3 units high and 40
  units long along gameplay Z instead of standing 40 units vertically. The
  same correction keeps the runner, gates, Goal portal and city in their
  authored Unity-axis orientation. Collision, Track recycling and Stage
  coordinates remain owned by their existing non-art parents.
- Each pooled 40-unit Track segment now owns matching start/end anchors at
  local Z `-20 / +20`. Placement aligns the start anchor, rather than the
  imported-art parent pivot, to the requested world start. Reset and recycling
  therefore keep all six fixed segments end-to-start continuous beyond the
  authored Stage distance without pool growth, overlap or a disappearing road.
- Shield and Echo share one original transparent spherical protection-field
  presentation with a procedural hex grid, Fresnel rim, restrained motion and
  Bloom-reactive emission. Normal Shield is fixed Cyan/Electric Blue; held Echo
  uses only its exact stored runner hue through a property block. Consumption
  emits one short outward pulse in that same exact field hue before the surface
  disappears. The URP shader uses an explicit shader-model target, stable
  branch-free hex-cell math and instancing/stereo setup; EditMode and the
  Campaign Builder reject compiler errors instead of allowing a pink error
  fallback. An attempt
  that starts with a selected or stage-provided normal Shield generates no Echo
  Provider gates for that entire attempt in Campaign or Experiment, including
  after Shield consumption, Continue or Retry with the retained selection.
- Booster presentation is a runner-aligned 3D Warp corridor. Its non-emitting
  controller follows the Player, while Cyan and Gold stretched particles emit
  through enlarged forward box volumes, including the upper portrait frame,
  and remain in World Space so depth and parallax read along the travel
  direction instead of sticking to screen center. Both layers prewarm. The
  two-ended lifetime fade and hard 160-particle emitted capacity remain. The
  normal chase camera is closer at `(0, 7, -8.2)` / `18°`; Booster blends to
  `(0, 5, -6.5)` / `13°` while preserving its existing FOV contract. It reuses
  the existing
  Booster state, distance, FOV, warning and pause ownership and allocates no
  runtime emitter.
- Campaign contains 26 stable-ID Stages. Stages 1–5 remain the color/rhythm
  foundation; Stage 6 provides Shield and Stage 7 provides Booster while
  selection stays locked; Stage 8 is the first clean three-color Stage with
  selectable items. Stages 9–20 teach Camouflage, Fog, Ice and Echo in
  three-Stage intro/practice/mastery blocks using 2/2/3 colors. Stages 21–23
  teach Hidden recall and Stages 24–26 teach Flicker timing with the same
  2/2/3-color block shape and an isolated modifier per block.
- Hidden Stages 21–23 now place `7 / 9 / 11` deterministic occurrences across
  the full run. Their `1.30 / 1.15 / 1.00s` hide leads and shared `0.18s`
  emblem fade/contract transition make the intro readable while increasing
  repetition and later-run memory pressure. Judgment and simulation authority
  remain unchanged.
- Flicker Stages 24–26 place `6 / 8 / 10` deterministic occurrences with
  `1.35 / 1.17 / 0.99s` switch periods. A gate
  may begin changing only while farther than 24 scalar path units from the
  runner. During its `0.24s` dual-color window, the next neon color travels
  through the existing emissive frame meshes from chase-view screen-left
  bottom-to-top, across the top left-to-right, then down screen-right. No overlay geometry or
  LineRenderer is used. The center emblem simultaneously replaces the old
  sprite with the next through a complementary noise dissolve, so it never
  performs a disappear-then-reappear sequence. Afterward the new color is
  committed and held through judgment, and gate-break fragments use that
  committed color rather than the authored initial color. Shield and Booster
  are selectable; deterministic Flicker teaching targets remain after the
  finite Booster travel distance.
- Existing saves that cleared Stage 23 and still store Stage 23 as their
  highest unlocked Stage resolve Stage 24 as effectively unlocked after this
  catalog expansion. The earlier Stage 20→21 repair remains intact. Loading
  does not rewrite the Product save or legacy PlayerPrefs; a later ordinary
  progression write persists the new stable ID.
- From Stage 8 onward, selecting Shield or Booster consumes one owned unit of
  each selected item in one atomic Product save when `START` succeeds, before
  Countdown. At zero stock the item button offers one Shield or Booster for
  `900` Coins through a blocking confirmation modal. A successful purchase is
  atomic, transaction-ID idempotent, grants exactly one item and selects it;
  `START` then consumes it through the existing attempt authorization. Low
  balance shows the exact shortage, while save failure spends and grants
  nothing. Insufficient current stock or start-save failure keeps PreRun open
  with truthful status and publishes no partial spend. Retry is a new attempt
  and consumes again; Back before Start and duplicate Start do not consume.
  Stage 6/7 provided training items remain free and do not consume inventory.
  Without a ready Product session, selectable items and quick buy are
  unavailable while no-item and provided-item starts remain valid.
- Coin Continues cost `900`, `1,900`, `2,900`, then `4,900` Coins for every
  later successful Coin use in the same attempt. There is no attempt-wide
  Continue-count cap; a finite Stage and available authorization sources
  remain the practical bounds. One completed
  rewarded-ad Continue is available per attempt when a real provider reports
  availability; Coin use does not consume that right and ad-first does not
  raise the first Coin price. Coin spending is atomic and idempotent before
  Core resumes. Insufficient funds, save failure, failed/cancelled ads and
  stale callbacks keep the failure state frozen. Retry creates a fresh policy.
  The current release provider is unavailable, so the ad action is
  intentionally hidden and never simulated as successful. Failure UI shows
  the current Coin and Heart wallet. Insufficient Coin selection opens a
  truthful popup while keeping the failed run frozen; the popup does not
  pretend the still-deferred Shop exists.
- A normal Stage start consumes one Heart atomically with selected start items.
  Hearts cap at 5 and recover one every 30 minutes, including offline;
  backwards clock movement cannot accelerate recovery. Unlimited Hearts
  suppress consumption until UTC expiry and purchased durations stack. A
  stored Heart actually consumed for an attempt is refunded atomically when
  that attempt clears, so a successful run has zero net Heart cost while
  failure, quit, or restart retains the spend.
- Initial Stage start retains `3, 2, 1, GO`. Continue instead shows `READY` for
  `0.5s`, resumes gameplay when `GO` appears and removes the non-blocking `GO`
  flash after `0.25s`. Active gate modifiers are already rendered and consumed
  Shield/Booster state is already absent before READY. Campaign Clear shows Replay (Iteration 68A),
  separates first-clear and Lobby milestone rewards, and opens the unlocked
  next Stage directly in PreRun. Final or still-locked next Stages hide that
  action.
- Campaign Stages 12–14 no longer neutralize all but two Fog gates. When the
  first Fog gate becomes the next judgment, one world-space storm bank appears
  at 0.9 seconds of current travel distance ahead of the player. It fades from
  alpha 0 to 1 over 0.5 seconds, stays fully opaque for an authored 5 / 6 / 7
  seconds across the intro / practice / mastery curve, then fades to 0 over
  0.5 seconds. It cannot retrigger in that attempt. Pause and Continue
  countdown freeze it; Continue preserves its state and Retry resets it.
  Experiment Lab retains its historical nearest-two Fog comparison contract.
- Campaign Stages 15–17 preplace every authored Ice approach through one fixed
  50-panel runway pool before Countdown. Ice panels span the deterministic
  approach interval into each Ice gate, while the six recycled normal track
  segments keep their normal material. Campaign Ice multiplies non-Booster
  speed by an authored `2.0`; Booster remains authoritative while active.
  Continue preserves the same runway and Retry rebuilds it deterministically.
  Experiment Lab intentionally retains its historical whole-track `1.45`
  comparison behavior.
- Authored Ice gates now prefer one forward tap. Stage 15 and 16 require one
  tap on every Ice gate; Stage 17 requires one tap on 14 of 15 Ice gates and
  two taps on one deterministic gate (`6.67%`). Ice gates never require zero
  or more than two authored taps. Retry reproduces the same quota and colors.
- The local catalog owns six Coin packs and five bundles. Its Frontend Shop
  preview renders all 11 definitions without a provider price. Grants are atomic
  and order-ID idempotent; Starter is locally account-limited. Continue
  Tickets are offered before real ads and Coins. Store connection, receipt
  validation, purchasing, Firebase integration and a real rewarded-ad provider
  remain deferred.

### Automated validation

- Iteration 68A on `2026-11-03` KST (parent `4ba41e5`): EditMode `544/544`;
  PlayMode `280` passed, `1` intentional opt-in ignore. Builders `2/2`. Campaign and
  Step 10 artifacts match the baseline hashes below.
- Iteration 67 on `2026-11-03` KST (parent `50cc5d5`): EditMode `527/527`;
  PlayMode `276` passed, `1` intentional opt-in ignore. Builders `2/2`. Campaign and Step 10 artifacts match the
  baseline hashes below.
- Iteration 64C on `2026-11-03` KST (parent `308625a`): EditMode `514/514`;
  PlayMode `272` passed, `1` intentional opt-in ignore. Builders `2/2`. Core unchanged, so simulation hashes carry over.
- Iteration 64B on `2026-11-03` KST (parent `0c36ecc`): EditMode `510/510`;
  PlayMode `272` passed, `1` intentional opt-in ignore. Builders `2/2`.
  Campaign and Step 10 simulation artifacts match the baseline hashes below.
- Iteration 64A on `2026-11-03` KST (parent `dfa10cf`): EditMode `509/509`;
  PlayMode `268` passed, `1` intentional opt-in ignore. No Builder or Scene change.
- Iteration 63 on `2026-11-03` KST (parent `f63c82c`): EditMode `502/502`;
  PlayMode `268` passed, `0` failed, `1` intentional opt-in visual ignore.
  Builders `2/2`.
- Iteration 62 on `2026-11-03` KST (parent `d963031`): EditMode `497/497`;
  PlayMode `268` passed, `0` failed, `1` intentional opt-in visual ignore.
  Builders `2/2`. D3D11 Campaign capture `2/2`.
- Iteration 61 on `2026-11-03` KST (parent `d527300`): EditMode `494/494`;
  PlayMode `268` passed, `0` failed, `1` intentional opt-in visual ignore
  (`269` discovered). Builders `2/2`. D3D11 Campaign capture `1/1`.
  Values below this line are the Iteration 60 record.
- Fresh headless verification on `2026-09-12` KST at implementation HEAD
  `d937b3a242b04df6d211de7dfe92abfca82175e3`:
  - EditMode: `480/480` passed, `0` failed, `0` skipped
  - PlayMode: `265/265` available tests passed, `0` failed; the opt-in
    `LobbyPagesVisualCapture_WritesPortraitEvidence` test was intentionally
    ignored in headless mode (`266` total discovered)
- Post-Builder PlayMode: same `265` passes plus one intentional graphics-test
  ignore
- D3D11 visual QC: Frontend `1/1` with 14 captures at `1080 x 1920`, including
  a separate Home capture at `720 x 1280`. The inspected set covers all five
  Lobby destinations, three Shop depths, chapter/League variants and the
  compact Home layout. Iteration 59 Campaign `1/1` with 19 captures remains
  the latest gameplay visual evidence.
- Fresh-context read-only Quality Graph checker: `PASS` with no blocking
  finding after diff, Builder, full-suite, protected-state and rendered-image
  review
- Frontend Scene Builder: two consecutive final passes completed; Boot and
  Frontend generated references, authored shell/page sprites, five-tab
  navigation, Shop/Journey ScrollRects, unique roots, EventSystem and Build
  Settings passed
- Campaign Builder: Iteration 59 completed two consecutive passes successfully.
  The active Campaign Spline path, generated full-route road, deterministic
  24-slot city
  pool, ten-bank Fog band, capped weather particles and scoped tone volume,
  50-slot Spline Ice mesh pool and six Echo membranes passed alongside the
  Spline Lab, one valid 22-part modular runner form, bounded camera/runner
  steering, adaptive color strip, quick
  Continue, quick-buy/VFX contracts, fixed gate/legacy track pools and Build Settings
- Stage Catalog Builder: the latest two consecutive Iteration 50 passes completed;
  revision 16 Resource contains 26 valid stages
- Missing Script / Missing Reference / duplicate generated object failures:
  none

### Build Settings

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/Frontend.unity`
3. `Assets/Scenes/SampleScene.unity`

All three entries are expected to be enabled and unique.

### Android and WebGL player artifacts

- The Test Build menu is implemented for Android and WebGL and rebuilds the
  generated Catalog/Campaign/Frontend/Boot content before creating a Player.
- Existing Android artifact:
  `Builds/Test/Android/ColorGateRunner.apk`, `68,774,305` bytes, written
  `2026-08-28T10:26:16Z`, SHA-256
  `885E3CC8FE7693590AA3CF8DC543A488E3D53F48A804C786EFE73CB6109BFA91`.
- Existing WebGL artifact: `Builds/Test/WebGL`, six files totaling
  `155,566,553` bytes, newest file `2026-08-28T10:29:18Z`; `index.html`
  SHA-256
  `2B3ED6A0BA943D4FDF32606ED4ABAE1CBB252F5DCBF4A322F96A9B3204B78988`.
- These artifacts predate Iterations 46–60 and do **not** represent the current
  implementation HEAD. A current Android/WebGL build, Android installation and
  HTTP-served WebGL smoke test have not yet been performed.

### Developer save snapshot

- Schema 3, revision 742, profile/Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, highest unlocked Stage ID `stage-05`
  with four records, 45,900 Coins, 8 Shields, 8 Boosters, 4 Hearts and 0 Continue
  Tickets; selected Lobby theme is empty.
- SHA-256:
  `936ADE5FDE839A4F140001310FE82A5CAB70532D498350CEAADF36AAF130C75F`.
- This user-authored play state was written at `2026-09-10T08:38:11Z`.
  The `2026-09-12` snapshot verification preserved it byte- and timestamp-exact.
  Developer Console mutations still
  occur only after an explicit Apply, Reset, or Unlock action.

### Campaign simulation baseline

- Rows: `520`
- Summary SHA-256:
  `0C07357C04BDDDDBEFD35D621966967B33D0F68099F0D65ED8FFCAE7BF2347CA`
- JSON SHA-256:
  `E2B18725FE63BD7C93109C0EE9CC08777CC2BA814B05A4B38E61F9C6ADFB807B`
- CSV SHA-256:
  `5502E9A93461A49D28BD3040666733778A768F59B611FC61B7FDCBDB39F7AB84`
- Two complete runs are byte-identical. Continue-use metrics are deterministic
  and bounded by each finite Stage. Continuity and pool-violation counters are
  zero.
- The 60 new rows cover Stages 24–26 across every standard profile and start-
  item combination. Existing Stage 1–23 rows retain their prior behavior.
- Step 10 was rerun twice. Its 80-row, 64,016-run artifacts remain byte-exact
  to the baseline below.

### Step 10 simulation baseline

- Report rows: `80`
- Simulation runs: `64,016`
- CSV SHA-256:
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`
- JSON SHA-256:
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`
- Summary SHA-256:
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`
- Comparison SHA-256:
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`
- Shortlist SHA-256:
  `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`

### Persistence safety baseline and historical evidence

- Campaign PlayerPrefs rollback snapshots contain Highest Unlocked plus every
  present authored Stage Record key and must be restored exactly after tests.
- Actual Editor snapshot at Iteration 12 completion:
  - existing Campaign entries: `12`
  - canonical SHA-256:
    `B57BDC93138A3E1C376272C33FB042274E821B8BB9C0EC12A33B9480010C73CD`
- Historical Iteration 18 evidence: the Editor Product save was schema 2,
  revision 23, profile/Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, with Highest Stage 13, 13 Stage
  records, 2,600 Coins, 4 Shields and 4 Boosters. Its SHA-256 is
  `39FB23837EE260E8F1B3CF7E1EF7EBE6389DC689A59CC3E68B6164516E180A18`
  and `LastWriteUtc` is `2026-08-11T03:21:03.1535325Z`. The hash and values
  were identical after Iteration 18 validation. Iteration 18 retained schema
  2 and did not migrate or rewrite the then-current Product save. It does not
  override the schema-3 Developer save snapshot above.
- Values deleted before Iteration 11 are unknown and must not be guessed.

### Package and ProjectSettings baseline

- `Packages/manifest.json` SHA-256:
  `5E44864DB10C6A0A47806035C801D2271F6F48B0BF8BF6F699B88C71D6758C96`
- `Packages/packages-lock.json` SHA-256:
  `FF4B3AC486B926719707FF2447580CEB1CE33D7C0851BABA229A3CD73C48CBBE`
- `ProjectSettings/ProjectSettings.asset` SHA-256:
  `BFF9843B9363FF1C20B113108D623026E777311CAEF6372B07C92690049717BC`
- Approved package delta: Unity IAP `5.4.2`, Unity Services Core `1.18.0`,
  Unity Splines `2.9.0` with its Settings Manager dependency, and
  `Assets/Resources/BillingMode.json` with Google Play. The incidental
  Navigation, Rider, Visual Studio and Visual Scripting upgrades made during
  installation were removed and remain at their prior approved versions.
- Approved ProjectSettings delta: Android application ID
  `com.wscho.colorgaterunner`. Company name and non-Android identifiers are
  unchanged so the existing Editor save location is preserved.
- `ProjectSettings/ProjectSettings.asset` must retain the approved portrait,
  custom WebGL template, Input Actions preload, and all other semantic
  baselines.
- Package-managed WebGL defines `APP_UI_EDITOR_ONLY` and
  `SENTIS_ANALYTICS_ENABLED` may vary by environment in presence, combination,
  or order. They are never staged in a feature commit. Any other scripting
  define or ProjectSettings semantic difference is a baseline failure.
- Routine iterations must not redefine these values. Any intended Package or
  ProjectSettings change requires explicit approval and a new documented
  baseline.

### Snapshot audit notes

- No current gameplay or product defect is confirmed by the fresh automated
  suite. Historical Clone, Stage 26 gate generation, Flicker direction/timing,
  current-color gate break, Continue pool exhaustion and Lobby overflow issues
  are fixed evidence, not open bugs.
- `Docs/FRONTEND_FLOW.md` still has an old top-level Status paragraph that says
  Journey, extra themes, Rank and Collection are pending/disabled. Its later
  Iteration 54–60 sections and the actual Builder/code show the authoritative
  state: Journey and all three themes are implemented; Rank and Collection are
  navigable illustrated Coming Soon pages.
- `Docs/TEST_PLAN.md` retains historical active-map text that refers to older
  Catalog revisions, Stage ranges and PlayerPrefs key bounds. Its later
  Iteration-specific acceptance history and this Authoritative Baseline match
  Catalog revision 16 and Stages 1–26. This snapshot does not edit that file
  because the approved documentation scope is limited to Current Status and
  GPT Handoff.
- `Docs/GAME_DESIGN.md`, `Docs/DECISIONS.md` and `Docs/ITERATIONS.md` contain
  intentionally historical contracts and rejected/superseded ideas. Their
  historical sections must not be interpreted as current implementation when
  they conflict with this baseline.

## Latest Iteration Result and History

## Iteration 54 validation result

- Added the five-page horizontal Lobby pager, truthful Rank/Collection Coming
  Soon pages, package-volume Shop art and the read-only Journey milestone path.
- Frontend Builder completed two consecutive final passes. Full EditMode passed
  `473/473`; full PlayMode passed `259` cases with one opt-in visual capture
  intentionally ignored and no failures.
- A graphics-capable Direct3D 11 run at `1080 x 1920` passed `1/1` and produced
  eight captures covering Home, three Shop depths, Rank, two Journey positions
  and Collection. Original package art is cleanly cropped, reward glyphs and
  quantities are visible, and Stages 28-36 identify unavailable content as
  `COMING SOON`.
- The fresh-context independent checker returned `PASS` with no finding after
  reviewing pager arbitration, Shop/Journey projection rules, tests, Builder
  evidence and rendered captures. No checker repair was required.
- Missing Script/reference, unique generated root, one EventSystem and Build
  Settings checks passed through the Builder. Package and ProjectSettings have
  no diff. Product save timestamp predates validation and remained unchanged.
- Campaign and Step 10 simulations and Player builds were omitted because no
  deterministic gameplay, balance, timing, generation or judgment input
  changed and the user owns Android/WebGL builds.

## Iteration 52A validation result

- Campaign Clear still atomically refunds the finite Heart consumed by the
  successful attempt, but no reward row or active result text contains
  `HEART RETURNED`. Stage 8's first-clear example now displays only Clear
  Coins, Milestone Coins and Shield.
- Focused Product/Frontend PlayMode passed `1/1`. Full EditMode passed
  `465/465`; full PlayMode passed `257/257`.
- One graphics-capable Q2 run at `1080 x 1920` on Direct3D 11 passed `1/1`.
  The clear reward capture shows the expected three earned reward rows with no
  Heart refill copy and no layout gap that blocks Next Stage or Lobby.
- No Scene, Builder, package, ProjectSettings, economy rule, save schema or
  deterministic gameplay input changed. The fresh Product save hash above
  predates automated validation and remained unchanged through both suites.

## Iteration 52 validation result

- Added a pure Campaign result state machine and an extensible failure
  consequence queue. Campaign success now separates celebration from the
  committed reward list; Campaign failure separates Continue, Give Up
  confirmation, future consequences and final Retry/Lobby choice. Experiment
  continues to use its existing result behavior.
- The Builder-owned result presentation uses one original victory emblem, a
  fixed 16-spark celebration pool and five fixed reward rows. It performs no
  runtime instantiation. Theme 1 danger/action button skins and semantic icons
  remain attached throughout the failure sequence.
- Heart ownership remains unchanged: a finite Heart is consumed at Stage
  start, refunded only by a successful clear and never charged again by Give
  Up, final Retry or Lobby selection.
- Campaign Builder passes 5 and 6 both completed successfully. Full EditMode
  passed `465/465`; full PlayMode passed `257/257`.
- Two graphics-capable Q2 runs at `1080 x 1920` on Direct3D 11 each passed
  `1/1`. Their Continue Offer PNGs were byte-identical at SHA-256
  `114848C2ACBF940BA11F656DB0EB8B8B46A756CBD8E87A588E2EEB6F322F6F1F`;
  their Final Choice PNGs were byte-identical at SHA-256
  `6C7C8F549CFD46435E5984897DFC0C61160FF9029B06770ECD98B943B90F17D4`.
  Both captures contain the expected danger card and action icon pixels, and
  the two logs contain no RenderTexture creation warning.
- A read-only fresh-context checker independently inspected the final diff,
  tests and unique A/B artifacts and returned `PASS` with no severity finding.
- Package files, ProjectSettings and the user-authored Product save retain
  their authoritative SHA-256 values. Campaign and Step 10 simulations were
  not rerun because result presentation does not change content, timing,
  movement, generation or judgment.
- Automated evidence cannot certify celebration satisfaction, reveal pacing,
  portrait-device readability, touch comfort or WebGL/mobile presentation.

## Iteration 51 validation result

- Added a reusable `quality-graph` Skill, a thin Color Gate Runner adapter and
  preflight entry points for inspect, primary-project batch validation,
  graphics-aware visual QC and finalization. `Tools/Validate.ps1` now refuses
  unsafe primary-project batch execution before starting Unity.
- Environment evidence distinguishes project copy, Editor/process lock state,
  validation backend, graphics API, resolution and Player artifact shape.
  Protected-path checks reject relative, absolute and case-variant access while
  Unity is open; Player evidence rejects arbitrary files.
- The preflight self-test passed `13/13`. The generic Skill passed its packaged
  validator and a non-Unity fixture verified that project adapters do not leak
  Unity-specific wording or lock rules.
- A fresh-context checker initially rejected five gaps in path protection,
  Player evidence, generic fallback behavior, visual metadata and policy
  wording. One targeted repair addressed all five; the same independent route
  then returned `PASS` with no remaining findings.
- With the primary Unity Editor closed and no lock present, full EditMode passed
  `457/457` and full PlayMode passed `255/255`. No Builder or deterministic
  simulation rerun was required because no game code, Scene, generated content,
  Stage data, timing, judgment or simulation input changed.
- Batch execution briefly reserialized two generated Warp materials; those
  incidental changes were restored. Package and ProjectSettings hashes remain
  exact, and the user-authored Product save above remained byte- and
  timestamp-identical. No Android or WebGL Player was built, so device visual
  quality remains human evidence rather than an automated claim.

## Iteration 50 validation result

- Chase-view Flicker flow now rises on the visible screen-left pillar, crosses
  the top from left to right and descends on the visible screen-right pillar.
  The imported FBX left/right names remain serialized details and no Scene
  reference or model is replaced.
- Campaign Stage 24–26 switch periods are `1.35 / 1.17 / 0.99s`, exactly 1.5
  times Iteration 49. The `0.24s` dual-color transition and 24-unit safety lock
  are unchanged.
- Shield and Booster are both selectable in the Campaign Flicker block.
  Visibility planning applies Booster speed only to the authored spacing that
  overlaps its travel distance, then normal maximum speed afterward. The
  deterministic `6 / 8 / 10` Flicker targets occur after the Booster reach and
  reproduce after Retry. Experiment Hidden/Flicker item policy is unchanged.
- Stage Catalog Builder completed two final revision-16 passes. Focused
  Campaign EditMode passed `15/15`, focused Flicker PlayMode passed `13/13`,
  full EditMode passed `457/457` and full PlayMode passed `255/255`.
- Two complete 520-row Campaign simulations were byte-identical with the
  current Summary / JSON / CSV hashes above. Step 10 was omitted because
  Experiment inputs are unchanged. Android and WebGL builds were not produced.
- The user completed the current Editor play check and confirmed the corrected
  visible side order, longer Flicker timing and restored Booster access.
  Portrait mobile and WebGL device review remain separate human checks.

## Iteration 49 validation result

- Flicker Campaign switch periods are now `0.90 / 0.78 / 0.66s` for Stages
  24–26 and the shared dual-color transition is `0.24s`. The 24-unit lock,
  Shield availability and intentional Booster lock remain unchanged.
- Each imported frame part resolves its shader-axis sign in gate space, so the
  visible replacement direction remains left-up, top-right and right-down even
  when FBX transforms or a curved route rotate the gate.
- Stage 26 startup no longer asks the fixed 50-panel Ice runway to validate a
  non-Ice 52-gate Stage. Ice-bearing stages retain the exact pool-capacity
  validation; non-Ice stages reset and skip runway construction.
- Focused Campaign EditMode passed `11/11`, focused Flicker PlayMode passed
  `13/13`, and Stage 24–26 startup PlayMode passed `3/3` after reproducing and
  fixing the Stage 26 exception. Full EditMode passed `457/457`; full
  post-Builder PlayMode passed `255/255`.
- Stage Catalog and Campaign Builders completed two passes. Two complete
  520-row Campaign simulations were byte-identical with the current Summary /
  JSON / CSV hashes above. Step 10 was omitted because Experiment timing,
  profiles and inputs are unchanged.
- Android and WebGL player builds were not produced; the user owns those
  on-demand test builds.

## Iteration 48 validation result

- The separate Flicker U-path LineRenderer and trace shader/material are
  removed. One pooled `FlickerGateFrameView` drives the three existing neon
  mesh renderers with an opaque emissive dissolve shader using each mesh's
  actual local bounds. Segment progress remains left-up, top-right and
  right-down while geometry, collider and Core timing stay unchanged.
- The emblem dissolve shader no longer uses the HLSL-reserved `point` token.
  Both Flicker shaders passed a graphics-capable D3D validation guard, fixing
  the magenta error-material fallback that the earlier headless suite missed.
- Campaign and Experiment gate-break paths now resolve their material from
  `StageGateView.AssignedColor`. A completed Flicker change therefore breaks
  in the committed current color rather than the authored initial color.
- Campaign Builder completed two passes. Graphics EditMode Flicker shader
  checks passed `2/2`, focused Flicker PlayMode passed `13/13`, full EditMode
  passed `457/457`, and full post-Builder PlayMode passed `252/252`. Missing
  Script/reference, generated-root, EventSystem and Build Settings validation
  passed through the Builder and full suites.
- Campaign and Step 10 simulations were not rerun because no deterministic
  content, timing, balance, generation or judgment input changed. Android and
  WebGL player builds remain user-owned and were not run.
- Human portrait/mobile/WebGL review remains required for dissolve direction,
  noise edge, Bloom intensity and the perceived continuity at `0.12s`.

## Iteration 47 validation result

- Flicker's Core timing, 24-unit path-distance lock, `0.12s` transition and
  dual-color judgment remain unchanged. This iteration changes only pooled
  Presentation and its Builder-owned Scene structure.
- The former inner quad wipe is removed. One collider-free pooled LineRenderer
  now reveals the next HDR neon color at constant path distance along the left
  pillar, top beam and right pillar. The committed frame material changes only
  when the existing Core transition completes.
- Every pooled gate now owns a second emblem SpriteRenderer. A shared
  procedural-noise dissolve shader uses complementary masks for outgoing and
  incoming sprites plus a next-color emissive edge; both sprites coexist for
  the transition and the secondary sprite is disabled on completion, Retry and
  pooled reuse.
- Campaign Builder completed two consecutive passes. Focused Flicker PlayMode
  passed `12/12`, full EditMode passed `455/455`, and post-Builder full
  PlayMode passed `251/251`. Shader compilation, required references,
  collider-free path, unique generated root and Build Settings passed.
- Campaign and Step 10 simulations were not rerun because no deterministic
  content, timing, balance, generation or judgment input changed. Their
  approved hashes remain authoritative. Android and WebGL player builds remain
  user-owned and were not run.
- Human portrait/mobile/WebGL review remains required for the perceived path
  alignment, Bloom width, dissolve readability and whether `0.12s` provides
  enough frames for the richer presentation at 30 and 60 fps.

## Iteration 46 validation result

- Catalog revision 13 adds stable Stages 24–26 as a Flicker-only learning
  block. They use 44/48/52 gates, 2/2/3 colors, increasing speed and cadence
  pressure, deterministic 6/8/10 Flicker occurrences, Shield availability and
  no Booster selection. Stage 26 is Hard.
- `FlickerGateRuntimeState` is the single deterministic authority for each
  gate's current color, pending color, transition progress and distance lock.
  At or below 24 scalar path units a new phase cannot begin; a transition that
  already began outside the boundary completes before the gate locks.
- Every transition lasts `0.12s`. During that window both the previous and next
  colors pass for the player and a held Echo. The same window drives a
  collider-free next-color neon wipe from left to right inside the gate.
- Flicker no longer presents an explanatory marker. Existing gate frame and
  emblem presentation remain authoritative outside the transition wipe.
- Stage Catalog Builder and Campaign Builder each completed two consecutive
  passes. Focused Flicker EditMode passed `22/22`, focused Flicker PlayMode
  passed `11/11`, full EditMode passed `455/455`, and full PlayMode passed
  `250/250`. Missing Script, Missing Reference, duplicate generated root and
  Build Settings checks passed through the standard suites.
- Two 520-row Campaign simulations were byte-identical with Summary / JSON /
  CSV hashes `BDCAAFE...A4EB4`, `748AF7A9...85F7A` and
  `627B5B4A...8C73`. Two Step 10 runs retained all five approved hashes across
  80 rows and 64,016 runs.
- The Editor Product save and Campaign PlayerPrefs were not mutated. No
  package, approved ProjectSettings semantic, Android build or WebGL build was
  changed. Player builds were intentionally left to the user's requested test
  build menu workflow.
- Human review remains required for the comfort of the 24-unit safety boundary,
  the readability and direction of the `0.12s` wipe under Bloom, and the
  authored Stage 24–26 pressure on mobile and WebGL.

## Iteration 45B validation result

- Starting HEAD `97358cd` matched Iteration 45A apart from one allowed
  package-managed WebGL define. Completion commit name:
  `feat: add hidden power-down flicker`.
- Hidden retains its authored observation and `0.18s` erase duration, but the
  monotonic fade is replaced by a deterministic three-dip power-loss envelope.
  Frame base color, HDR emission and emblem blink together while emblem scale
  continues to contract monotonically to `0.72`.
- Completion still applies the exact neutral shared material. Selection,
  memorized target, geometry, collider, judgment and Retry behavior are
  unchanged.
- Focused Hidden EditMode passed `15/15`; focused Hidden PlayMode passed `8/8`.
  Full EditMode passed `451/451` and full PlayMode passed `250/250`. No Builder,
  simulation or test build ran because Scene structure, deterministic content,
  timing, balance, generation and judgment did not change.
- Package files, tracked ProjectSettings and build outputs have no approved
  change. Device review must still judge whether the brief outage reads as a
  failing neon sign without making the target harder to memorize.

## Iteration 45A validation result

- Starting HEAD `7e28d98` was clean apart from one allowed package-managed
  WebGL define. Completion commit name: `fix: fully hide hidden gate color`.
- Hidden previously blended `_BaseColor` and `_Color` toward neutral while
  retaining the assigned emissive material. Its target-colored emission could
  therefore remain visible after the emblem disappeared.
- At transition completion Hidden now applies the same neutral material used
  by a hidden Camouflage gate. The memorized `AssignedColor`, geometry,
  collider and ordinary judgment remain unchanged; Retry restores the assigned
  color material before the next observation.
- Focused Hidden PlayMode passed `7/7`. Full EditMode passed `450/450` and full
  PlayMode passed `249/249`. No Builder, deterministic Stage data, Campaign or
  Step 10 input changed, and no test build was produced.
- Package files, tracked ProjectSettings, generated materials and build output
  have no approved change. Stage 21–23 visual confirmation remains human
  review because automation cannot establish whether all residual color is
  absent on the target display.

## Iteration 45 validation result

- Starting HEAD `db8d212` was clean. Completion commit name:
  `feat: improve hidden readability and color emblems`.
- Catalog revision 12 keeps 23 stable Stages and redistributes Hidden across
  the whole of Stages 21–23 with `7 / 9 / 11` occurrences. Stage-local leads
  are `1.30 / 1.15 / 1.00s` and the shared transition is `0.18s`; ordinary
  target-color judgment, seeds, gate counts, speed, cadence, rewards and save
  contracts remain unchanged.
- Six original transparent Theme 1 emblem sprites replace Unicode shapes on
  pooled gates and the bottom color-order HUD. Hidden now communicates only by
  fading/contracting that emblem and neutralizing the frame; no `HIDDEN` text
  or instructional label remains. Elemental gate theming stays deferred.
- Android Gradle alignment now implements its post-generation callback on
  every Editor start target, rather than compiling the callback only while
  Android is already active. The real combined Android-plus-WebGL command
  therefore succeeds when launched from WebGL.
- Focused Hidden EditMode passed `10/10`; focused Hidden PlayMode passed `7/7`
  plus the live Campaign approach case `1/1`. WebGL-start build-tool EditMode
  passed `7/7`. Campaign Builder completed consecutive passes; final full
  EditMode passed `450/450` and final post-Builder PlayMode passed `249/249`.
- Two 460-row Campaign simulations retained Summary
  `4E3B9ACE68ABDFF9540F9A26D7085C664C1658DAC133D73AE7E87BE3A5A1D07B`,
  JSON `3C3233C5CA3DA50B1272ADE1FE451D718CAA4E148598C81220881530B942A68D`
  and CSV `9CFB8DE0CFC54483E518F91290F998FEF57CBC5E0722CC4A0B2BDC744EF43611`.
  Two Step 10 runs retained all five authoritative hashes.
- The real combined Development build produced a fresh 68,774,305-byte APK
  and a 155,566,553-byte WebGL folder with index, data, framework, loader and
  WASM. Its canvas remains `540 x 960` and exact `9:16`.
- Package files, tracked ProjectSettings and the developer Product save retain
  their approved baselines. Installation, touch/browser behavior, emblem
  distinction and Hidden feel remain human review.

## Iteration 44 validation result

- Starting HEAD `0318ba1` was clean. Completion commit name:
  `feat: add hidden stages and refresh test builds`.
- Catalog revision 11 adds stable Stages 21–23 as Hidden intro, practice and
  mastery with 2 / 2 / 3 active colors. Stage 23 is Hard. Hidden uses the
  existing deterministic modifier, pooled presentation and ordinary judgment;
  no new runtime authority or save schema was introduced.
- Catalog-expansion repair makes Stage 21 effectively available to saves that
  cleared Stage 20 without mutating them on load. Focused runtime coverage
  verifies Campaign Hidden hides only its target while retaining the `HIDDEN`
  marker and ordinary judgment.
- Test Build commands now refresh Stage Catalog, Campaign, Frontend and Boot
  immediately before packaging and delete the previous platform output first.
  Android additionally aligns Kotlin stdlib dependencies in generated Gradle
  projects, including upgrading a cached legacy insertion safely.
- Campaign Builder completed two consecutive passes. Full EditMode passed
  `449/449`; final PlayMode passed `248/248`. Two Campaign simulations produced
  460 byte-identical rows with the authoritative hashes above; two Step 10
  simulations retained all five 80-row / 64,016-run hashes.
- A real menu-driven Android Development build produced a 64,957,676-byte APK.
  A real menu-driven WebGL Development build produced a 156,006,568-byte folder
  with `index.html`, data, framework, loader and WASM; its canvas is `540 x 960`
  with an exact `9:16` frame. Installation, browser interaction and device
  performance remain human acceptance work.
- Package files and tracked ProjectSettings have no diff. Build output and
  Unity-generated temporary changes remain untracked/ignored and unstaged.

## Iteration 43 validation result

- Starting HEAD `f507692` was clean after the allowed package-managed define
  churn and unrelated Editor material refreshes were restored. Completion
  commit name: `feat: refine fog weather and curve readability`.
- Fog now combines ten closer fixed banks, bounded world-space wisps and rain,
  and a scoped darker tone under the existing one-shot lifecycle. Camera lag
  expands to `24° / 16°` with a `0.3s` half-life; runner visual steering remains
  independent of the exact Spline root. The color sequence is bottom-centered
  and horizontal with current / next / previous emphasis.
- The affected Graybox PlayMode group passed `156/156`. Campaign Builder
  completed two consecutive passes. Full EditMode passed `444/444`; final
  post-Builder PlayMode passed `246/246`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package files are byte-exact and ProjectSettings is
  at the tracked semantic baseline with no staged change. The developer save
  remained byte- and timestamp-exact during validation.
- Human portrait/device play still owns Fog/rain density, dark-tone comfort,
  camera lag strength, runner steering feel, bottom-strip intuition, overdraw
  and target-mobile performance.

## Iteration 42 validation result

- Starting HEAD `ec50719` was clean. Completion commit name:
  `feat: polish campaign mechanic surfaces`.
- Campaign road and Ice now use separate original mapped non-emissive
  materials; each active fixed Ice slot samples its deterministic interval from
  the Campaign Spline. Fog uses six fixed Blender banks across an adaptive path
  band, and each gate owns one shared-shader Echo membrane that transfers its
  exact assigned color before hiding.
- The affected Graybox PlayMode group passed `154/154`. Campaign Builder
  completed two consecutive passes. Full EditMode passed `444/444`; final
  post-Builder PlayMode passed `244/244`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package and ProjectSettings hashes remained exact;
  isolated validation did not mutate the original Editor Product save.
- Human portrait/device play still owns Fog density/seams, surface contrast,
  Echo transfer readability, overdraw, Bloom balance and mobile performance.

## Iteration 41 validation result

- Starting HEAD `f1cc9d1` was clean. Completion commit name:
  `feat: add spline camera inertia and quick continue`.
- Campaign camera rotation follows the Spline with a `0.2s` half-life, bounded
  yaw/pitch lag and a hard no-roll reconstruction. Stage start, Retry and
  Continue snap the follow basis; Pause and failure freeze it.
- Continue now uses a frozen `READY` presentation for `0.5s`, begins gameplay
  when `GO` appears and removes the non-blocking GO flash after `0.25s`.
  Initial Stage start remains the existing three-second countdown.
- Focused quick-Continue PlayMode passed `17/17`; focused curved-camera and
  Booster-return tests passed `1/1` each. Campaign Builder completed two
  consecutive passes. Full EditMode passed `444/444`; final post-Builder
  PlayMode passed `244/244`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package and ProjectSettings hashes remained exact;
  isolated validation did not mutate the original Editor Product save.
- Human portrait/device play still owns the perceived lag strength, motion
  comfort, gate visibility on crests and the READY/GO pacing.

## Iteration 40 validation result

- Starting HEAD `db7443336a4533aa4bca72f20bea1bd8848af93f` was clean.
  Completion commit name: `feat: convert campaign to spline routes and city
  pool`.
- All active Campaign Stages now map scalar progress to one Spline pose. Runner,
  camera, pooled gates, Goal, Fog, Ice and Continue share that authority; the
  generated full-route road replaces the legacy straight pool during Campaign.
- The 24 existing Theme 1 body/glow pairs now form one deterministic fixed city
  pool with at least 7.5 units of lateral road clearance and coverage through
  the Goal buffer.
- Focused Spline/city PlayMode passed `1/1`; full EditMode passed `444/444` and
  full post-Builder PlayMode passed `242/242`. Campaign Builder completed two
  consecutive passes with valid references, unique roots, EventSystem and Build
  Settings.
- Two Campaign simulations retained all three approved 400-row hashes. Two
  Step 10 simulations retained all five approved 80-row / 64,016-run hashes.
  Package manifest, package lock and `ProjectSettings.asset` hashes remain
  exact; isolated validation did not mutate the original Editor Product save.
- Human portrait/device review owns curve anticipation, gate readability on
  slopes, Fog/Ice visibility, Continue comfort, city density/pop, stable
  horizon, Bloom and mobile performance. Banking, loops, inversions, Race Mode,
  Stages 21–36 and new city art remain deferred.

## Iteration 39 validation result

- Starting HEAD `56c2c11` was clean. Completion commit name:
  `feat: add start-item quick buy and refine vfx`.
- A zero-stock selectable Shield or Booster now opens one blocking PreRun
  confirmation for a fixed `900` Coins. Product grants one item and charges
  once through an atomic, idempotent ledger transaction; success auto-selects
  the item, insufficient balance reports the shortage, and save failure
  publishes no spend or grant.
- The D3D11-reserved protection-shader identifier was replaced and the field
  roots were centered `0.85` units above the Player origin. Booster Cyan/Gold
  world-space boxes were enlarged and raised, both prewarm, and normal/Booster
  chase framing moved closer while keeping Booster rules, timing, FOV, pause
  behavior and the 160-particle cap unchanged.
- Focused Product EditMode passed `62/62`; focused Campaign PlayMode passed
  `152/152`. Campaign Builder completed two consecutive passes. Full EditMode
  passed `444/444`; full post-Builder PlayMode passed `241/241`. The protection
  shader also passed its focused compiler test `1/1` on an actual D3D11 AMD
  Radeon Vega 8 device with no shader error.
- Package and ProjectSettings hashes remain exact. Validation used an isolated
  project copy and did not mutate the original Editor Product save. Campaign
  and Step 10 simulations were omitted under Tier 2 because Core rules, Stage
  data, deterministic generation, timing, judgment, balance and Experiment
  inputs are unchanged.
- Human portrait/device review owns the modal hierarchy and wording, Shield
  centering/transparency, Warp coverage above and beside the runner, closer
  camera comfort, gate visibility, Bloom and mobile frame cost.
- This Iteration 39 roadmap note is historical and is superseded by Iteration
  40's whole-Campaign Spline conversion. Multi-lane Ghost AI Race Mode remains
  a later roadmap item and is not implemented.

## Iteration 38 validation result

- Starting HEAD `047a8fa` was clean. Completion commit name:
  `fix: restore world-space protection effects`.
- The protection-field material reference was intact, but human play exposed a
  pink shader fallback that name-only validation could not detect. The shader
  now uses URP-compatible branch-free hex math, explicit shader target and
  instancing/stereo setup; Builder and EditMode inspect compiler messages.
- The former camera-local radius-five Warp was the direct cause of the flat
  screen-center presentation. Its existing controller now follows the Player,
  while two Cyan/Gold forward box volumes simulate in World Space and stream
  backward along the runner direction. Booster rules, FOV, warning, pause,
  timing and the 160-particle cap are unchanged.
- Focused protection shader EditMode passed `1/1`. Campaign Builder completed
  two consecutive passes. Full EditMode passed `440/440`; full post-Builder
  PlayMode passed `237/237`.
- Package and ProjectSettings hashes remain exact. Validation used an isolated
  project copy and did not touch the original Editor Product save. Campaign and
  Step 10 simulations were omitted because no Core rule, Stage data,
  deterministic generation, timing, judgment, balance or Experiment input
  changed.
- Human portrait/device review owns final Shield transparency/hex readability,
  absence of pink fallback, Warp corridor depth, center readability, Bloom and
  mobile frame cost.

## Iteration 37 validation result

- Starting HEAD `ff3cc01` was clean. Completion commit name:
  `feat: add spline track lab`.
- Unity Splines `2.9.0` supplies one Builder-authored horizontal S-curve. A
  shared distance-to-path view evaluates runner, yaw-only chase camera, pooled
  gates and Goal poses; the Builder generates a dark road with Cyan edge
  submesh from the same curve.
- The Experiment panel exposes a development-only Lab entry with its own HUD,
  Restart and Exit. Lab failure/clear state is non-persistent. Exit restores
  the straight Campaign track and resets pooled gate positions and rotations.
- Focused EditMode passed `1/1`, focused PlayMode passed `2/2`, Campaign Builder
  passed twice, full EditMode passed `439/439`, and full post-Builder PlayMode
  passed `237/237`.
- ProjectSettings and the actual developer save were not changed. Campaign and
  Step 10 simulations were omitted because Core rules, authored Stage data,
  deterministic generation, timing, judgment, balance and Experiment inputs
  are unchanged.
- Human portrait review owns S-curve readability, camera comfort, road-edge
  Bloom and the transition back to the straight Campaign.

## Iteration 36 follow-up validation result

- Starting HEAD `91f17e8` was clean. Completion commit name:
  `fix: tint runner glass shell`.
- The whole spherical hull now follows the authoritative runner/failure color
  through one cached material property block while remaining non-emissive and
  highly smooth. The rear panel retains the existing HDR semantic material.
- Campaign Builder completed two consecutive passes. Focused EditMode passed
  `3/3`, focused PlayMode passed `1/1`, full EditMode passed `438/438`, and
  full post-Builder PlayMode passed `235/235`.
- Package and ProjectSettings hashes remain exact. Campaign and Step 10
  simulations were omitted because no gameplay or deterministic input changed.

## Iteration 36 validation result

- Starting HEAD `004e0ec` was clean. Completion commit name:
  `feat: improve runner readability and bloom`.
- Blender source and the Runner FBX now provide a dark ball-car body with a
  clearly authored rear silhouette: bumper, side fins, twin thrusters and
  glows, twin chevrons, rear light bar and a restrained current-color panel.
- Builder now regenerates the semantic gameplay colors with HDR emission,
  keeps dark alloy non-emissive and rejects missing/reversed rear parts,
  invalid material contrast or a weakened Bloom profile.
- Campaign Builder completed two consecutive passes. Focused EditMode passed
  `3/3`, focused PlayMode passed `1/1`, full EditMode passed `438/438`, and
  full post-Builder PlayMode passed `235/235`. Missing Script, Missing
  Reference, unique roots, EventSystem, fixed pools and Build Settings passed.
- Campaign and Step 10 simulations were not rerun because this iteration does
  not change Stage data, deterministic generation, timing, judgment, balance,
  collision or Experiment inputs. Package and ProjectSettings hashes remain
  exact to the approved baseline.
- Human portrait/device review still owns final rear-shape recognition,
  player/Track separation, Bloom strength and mobile performance.

## Iteration 35 validation result

- Starting HEAD `c4462e5` was clean. Completion commit name:
  `feat: upgrade protection and booster effects`.
- Campaign and Experiment now pass an immutable attempt-level normal-Shield
  exclusion into the existing deterministic Echo coordinator. Selected and
  stage-provided Shield suppress every Echo Provider; Echo itself does not.
- Builder replaced the six/four cube Shield/Echo shells with two instances of
  one transparent procedural-hex field and replaced side box speed lines with
  centered Cyan/Gold Warp layers capped at 160 emitted particles. Shield and
  Echo consumption now trigger fixed, pause-owned 0.35-second outward pulses
  using the exact consumed field hue.
- Campaign Builder completed two consecutive passes. Focused Core and
  PlayMode coverage, including both consumption pulses, passed; full EditMode
  passed `436/436`, and full final post-Builder PlayMode passed `235/235`.
- Campaign simulation ran twice with 400 byte-identical rows and the new hashes
  above. Only 24 Shield-bearing rows in Echo Stages 18–20 changed; all
  no-Shield rows are exact. Step 10 ran twice and retained every approved hash.
- Package and meaningful ProjectSettings hashes remain exact. Isolated
  validation did not use or mutate the developer Product save. The already-open
  original Editor had independently advanced that save from revision 231 to
  revision 237 before final validation; the revision 237 snapshot above was
  preserved rather than restored.
- Human portrait/device review owns field transparency, hex scale, Echo hue
  recognition, Warp density, central readability, Bloom balance and mobile
  frame cost.

## Iteration 34 validation result

- Starting HEAD `04826df` was clean. Completion commit name:
  `fix: preserve recycled track continuity`.
- Human play exposed the road disappearing after sustained movement. The
  imported mesh is 40 units long, but generated segment anchors still spanned
  only one unit; recycled segments therefore accumulated instead of appending.
- Builder now authors `-20 / +20` anchors and rejects a non-40-unit or
  discontinuous pool. Runtime placement aligns each start anchor to the prior
  segment's end anchor.
- Campaign Builder completed two consecutive passes. The new PlayMode
  regression recycles the fixed six-segment pool through 1.2 km and checks
  every span and seam. Full EditMode passed `434/434`; full post-Builder
  PlayMode passed `233/233`.
- Campaign and Step 10 simulations were not rerun because Stage data,
  deterministic gate generation, mechanic timing, judgment, balance and
  Experiment inputs are unchanged. Package and meaningful ProjectSettings
  hashes remain exact; isolated validation did not use the developer save.
- Human portrait play must confirm the road remains continuous through a full
  Stage and during prolonged development runs.

## Iteration 33 validation result

- Starting HEAD `12011cc` was clean. Completion commit name:
  `fix: normalize theme one artwork axes`.
- The generated Scene exposed the common imported-FBX root quaternion
  `(-0.7071068, 0, 0, 0.7071067)`, equivalent to `X -90°`. This rotated the
  authored 40-unit Z Track into the vertical Y axis. The Theme 1 instantiation
  boundary now explicitly normalizes all five imported artwork roots.
- Campaign Builder completed two consecutive passes and now rejects any
  non-normalized Theme 1 root or Track bounds that are not approximately
  7 units wide, below 3 units high and 40 units long on Z.
- Full EditMode passed `434/434`; final post-Builder PlayMode passed `232/232`.
  Existing Track recycling, Ice, Booster, Continue and fixed-pool coverage
  remained active.
- Campaign and Step 10 simulations were not rerun because collision, Stage
  data, generation, timing, balance, judgment and Experiment inputs did not
  change. Package and meaningful ProjectSettings hashes remain exact.
- Isolated validation did not use the developer save. The open original Editor
  independently advanced the live play state to revision 231; it was preserved
  and recorded above.
- Human portrait play must confirm the corrected road plane, runner/gate/Goal
  silhouettes and camera composition.

## Iteration 32 validation result

- Starting HEAD `6a542a9` was clean. Completion commit name:
  `feat: apply neon ui skin`.
- Added a reproducible procedural source for seven transparent 9-slice
  surfaces and ten neon action/resource icons. One shared Editor skin owner
  imports and applies them to both generated Frontend and Campaign UI without
  replacing live labels, navigation events or Product state.
- Frontend and Campaign Builders each completed two passes. Full EditMode
  passed `434/434`; final post-Builder PlayMode passed `231/231`. The new
  coverage checks real alpha, sprite borders, button state surfaces and
  non-raycast icons in both Scenes.
- Campaign and Step 10 simulations were not rerun because no Stage data,
  deterministic generation, timing, balance, judgment or Experiment input
  changed. Their approved hashes remain authoritative.
- Validation ran only in the isolated project copy. The developer save stayed
  at revision 229 and the package and meaningful ProjectSettings hashes match
  the authoritative baseline.
- Human portrait/device review owns icon scale, text/icon balance, 9-slice
  corner quality, disabled-state readability and final typography.

## Iteration 31 validation result

- Starting HEAD `3086233` was clean. Completion commit `6a542a9`:
  `feat: add neon gameplay visual slice`.
- Added reproducible Blender source/export tooling, one editable Theme 1
  `.blend`, five FBX models, fifteen UV/PBR maps, genuine-alpha Lobby ambient
  art, imported Campaign presentation and fixed pooled gate-break VFX.
- Campaign Builder completed two final passes. Full EditMode passed `433/433`
  and final post-Builder PlayMode passed `229/229`. Missing Script, required
  reference, duplicate-root and imported-prefab-parenting checks passed.
- Campaign and Step 10 simulations were not rerun because no Stage data,
  deterministic generation, timing, balance, judgment or Experiment input
  changed. Their approved hashes remain authoritative.
- Product validation ran only in the isolated project copy and did not touch
  the developer save. The open original Editor independently advanced the
  Heart clock to revision 229; that live state was preserved and is recorded
  above rather than restored. Package and meaningful ProjectSettings hashes
  match the authoritative baseline.
- Human portrait/device review still owns model scale, runner/gate silhouette,
  break readability, emissive balance, Bloom intensity and mobile performance.

## Iteration 30 validation result

- Starting HEAD `7284cb2` was clean. Completion commit name:
  `feat: add color courtyard visual slice`.
- Added three original Theme 1 raster assets and one data-driven Lobby visual
  catalog. Frontend Builder imports them as non-mipmapped UI Sprites, assigns
  the Background / Midground / Foreground layers and preserves palette-only
  fallbacks for Themes 2 and 3.
- The six existing automatic milestone visuals are now compact energy nodes
  around the reactor. Their activation still derives only from Product Lobby
  progression; all wallet, Stage card and launch behavior is unchanged.
- Frontend Scene Builder completed two passes. Focused Builder EditMode passed
  `9/9`, focused Frontend PlayMode passed `19/19`; full EditMode passed
  `431/431`; full post-Builder PlayMode passed `228/228`.
- Campaign and Step 10 simulations were not rerun because no gameplay, Stage
  data, timing, generation, judgment or Experiment input changed. Their
  existing approved hashes remain authoritative.
- Product save and backup remained byte-, timestamp- and SHA-exact. Package
  hashes and the meaningful ProjectSettings hash match the baseline. No
  Missing Script marker or required-reference failure was found.
- Human portrait review still owns visual scale, crop, contrast, pulse
  restraint and milestone-node readability on target devices.

## Iteration 29 validation result

- Starting HEAD `3e9b389` was clean. Completion commit name:
  `feat: simplify lobby hierarchy`.
- Frontend Lobby now uses a persistent top resource bar, one dominant theme /
  upgrade center and one compact bottom Stage card with a single primary
  `PLAY` action. Duplicate visible Lobby/account labels were removed.
- Product Heart snapshots expose the next recharge UTC without changing save
  schema or persistence. Lobby Heart text refreshes once per second and shows
  full count, next-recharge countdown or unlimited remaining time.
- The current Stage card includes authored difficulty. Shield/Booster stock is
  secondary, and milestone reward copy appears only while a reward is pending.
- Frontend Scene Builder completed two consecutive passes. Focused Lobby
  EditMode passed `8/8`, focused Product EditMode passed `58/58`, focused
  Frontend PlayMode passed `18/18`; full EditMode passed `430/430`; full
  post-Builder PlayMode passed `227/227`.
- Campaign and Step 10 simulations were not rerun because no Campaign Scene,
  gameplay, deterministic generation, timing, judgment or Experiment input
  changed. Their Iteration 28 and Step 10 hashes remain authoritative.
- Product save and backup remained byte-, timestamp- and SHA-exact. Package
  and meaningful ProjectSettings hashes remain the authoritative baseline.

## Iteration 28 validation result

- Starting HEAD `eddf720` was clean. Completion commit name:
  `feat: rebalance ice tap rhythm`.
- The Campaign Stage sequence now applies an Ice-only tap budget. Stage 15 / 16
  use one tap on all 12 / 14 Ice gates. Stage 17 uses one tap on 14 gates and
  two taps on one deterministic gate, keeping the double-tap rate at `6.67%`.
- Non-Ice stages, Ice speed/spacing/runway presentation, Continue, stable Stage
  IDs and Experiment Lab remain unchanged. Retry reproduces the same colors.
- Focused EditMode passed `13/13`; focused PlayMode passed `1/1`; full EditMode
  passed `429/429`; full PlayMode passed `227/227`. No Builder was required.
- Two complete 400-row Campaign simulations were byte-identical. Summary /
  JSON / CSV SHA-256 are
  `D3287E93FE0F022ABDF504EC3304076724BFBB3357A890DEFB0545E072CD9DB1`,
  `43C61E0CCB28B10211C6E201631EE2938D0160F72DA3A334AE0A459331276500`, and
  `4E1E87C9C1D5B27D63122744889DE066A3D22F2A93DF86CB6062DAC2165F5B99`.
  All continuity and pool-violation counters are zero. Step 10 was not rerun
  because Experiment behavior and inputs are unchanged.
- The real Product save and backup remained byte-, timestamp- and SHA-exact.

## Iteration 27 validation result

- Starting HEAD `4c3ce13` was clean. Completion commit name:
  `feat: add preplaced ice runway`.
- Stage Catalog revision 10 authors a `2.0` Campaign Ice speed multiplier for
  Stages 15–17. The Stage IDs, gate count, sequence, seed, spacing modifier,
  judgment and Experiment Lab definitions remain unchanged.
- Campaign Scene owns one fixed `IceRunway` with exactly 50 prebuilt panels.
  It activates all deterministic Ice approach intervals before Countdown,
  never grows at runtime, preserves the layout through Continue and clears it
  on Retry. The normal six-segment track no longer changes material for
  Campaign Ice.
- The Fog View was moved into a matching Unity script asset after a clean
  domain reload exposed its former file/class-name reference instability.
  Fog state, timing and behavior are unchanged.
- Campaign Builder completed two consecutive runs. Focused Ice tests passed
  EditMode `8/8` and PlayMode `4/4`; full EditMode passed `425/425`; final
  post-Builder PlayMode passed `226/226`. Missing references, duplicate roots,
  fixed pool and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical. Summary /
  JSON / CSV SHA-256 are
  `86D55FB085FE51135BFA5E0F5D17D0242166C1F9DBD3EE03F58485A33BFC27B8`,
  `3CE081477E7A431939D4C8AB6D0140FFBB6C22D6A6850E7551499E4C28BC28C2`, and
  `9D6E7903B31D59C3307DBE49A1E1FCF705A299FE37571299F17FAD63DF464B95`.
  All displacement, gap, duplicate-index, cursor-reset and full-pool-reset
  counters are zero. Step 10 was not rerun because Experiment behavior and
  inputs retain the historical `1.45` Ice comparison.
- The real Product save remained byte-, length-, timestamp- and SHA-identical
  to the captured pre-test snapshot.

## Iteration 26 validation result

- Starting HEAD `74a9953` was clean. Completion commit name:
  `feat: add fog visibility curve`.
- Fog curtain timing is authored in Stage Catalog revision 9. Stages 12 / 13 /
  14 use a 0.5-second fade-in, 5 / 6 / 7 seconds at full opacity, and a
  0.5-second fade-out respectively.
- Campaign judgment, gate plans, speed, spacing and stable Stage IDs are
  unchanged. Experiment Lab retains its historical nearest-two benchmark.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `423/423`; post-Builder PlayMode passed `225/225`. Missing references,
  duplicate generated objects, fixed pools and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical and retained
  Summary / JSON / CSV hashes
  `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`,
  `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`, and
  `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`.
  Step 10 was not rerun because Experiment inputs and behavior are unchanged.
- The real Product save and backup remained byte- and timestamp-identical to
  their pre-test snapshots.

## Iteration 25 validation result

- Starting HEAD `a8f7892` was clean. Completion commit name:
  `feat: add timed fog curtain`.
- Campaign Fog now uses a single speed-adaptive world-space curtain instead
  of per-gate nearest-two neutralization. The curtain holds for 1.5 seconds,
  fades for 0.5 seconds, preserves its state through Pause and Continue
  countdown, and resets on Retry.
- Campaign judgment, authored gate plans, speed, spacing and stable Stage IDs
  are unchanged. Experiment Lab keeps the historical nearest-two benchmark.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `421/421`; final post-Builder PlayMode passed `225/225`. Missing references,
  duplicate generated Fog curtain, fixed pools and Build Settings checks passed.
- Campaign and Step 10 simulations were not rerun because no Core, Catalog,
  generator, judgment, seed, balance or simulation input changed; Iteration 23
  hashes remain authoritative.
- The real Product save and backup remained byte- and timestamp-identical to
  their pre-test snapshots.

## Iteration 24 validation result

- Starting HEAD `61147cd` was clean. Completion commit name:
  `fix: recycle failed gates after continue`.
- Continue now recycles the failed gate's fixed-pool slot immediately instead
  of permanently deactivating it. Repeated Continues therefore keep supplying
  later gates and preserve Goal completion even after more Continues than the
  six visible gate slots.
- The clean respawn path no longer writes velocity to a kinematic Rigidbody.
- The Stage 11 regression deliberately Continues more than six times from the
  late Camouflage sequence, resolves all 46 gates, crosses Goal and reaches
  Stage Cleared.
- Focused Stage 11 PlayMode passed `1/1`; full EditMode passed `418/418`; full
  PlayMode passed `223/223`. No Scene, Builder, Core, Catalog, deterministic
  content or Product-save contract changed, so Builder, Campaign and Step 10
  artifacts retain the Iteration 23 baseline.
- The real Product save and backup remained byte-identical to their pre-test
  snapshots.

## Iteration 23 validation result

- Starting HEAD `7858803` was clean and matched the approved Iteration 22
  baseline. Completion commit name: `feat: refine continue and clear economy
  flow`.
- Continue pricing is `900 / 1,900 / 2,900 / 4,900-repeat` with no attempt
  cap. Resume Countdown applies modifiers immediately and presents consumed
  buffs as already gone.
- Failure shows Coin/Heart resources and a truthful insufficient-Coin popup.
  Campaign Clear hides Replay, displays first-clear and milestone rewards
  separately, refunds a consumed stored Heart atomically, and opens an
  unlocked next Stage directly in PreRun.
- Normal / Hard / Very Hard first-clear rewards are `100 / 200 / 500` Coins.
  Stages 11, 14 and 17 are Hard; Stage 20 is Very Hard.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `418/418`; final post-Builder PlayMode passed `222/222`. Missing Script,
  Missing Reference, duplicate generated root and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical with the
  authoritative hashes above. Step 10 was not rerun because Experiment inputs
  and contracts were unchanged.
- The real schema-3 Product save remained byte-identical at SHA-256
  `A81C6DA765C17DF44C131650EE73316D0F8B052B41938DC1AA82B18A350C8E18`.
- A real rewarded-ad provider, Shop navigation, Fog/Ice presentation redesign
  and final Lobby layout remain separate iterations and human review items.

## Iteration 22 validation result

- Full EditMode passed `414/414`; full PlayMode passed `218/218`.
- New isolated Product coverage verifies exact Economy edits, invalid-input
  non-publication, Campaign-only reset and Economy-only reset. Editor coverage
  verifies the approved menu path and existing Build Settings order.
- No Scene, Builder-owned hierarchy, gameplay rule, Stage data or deterministic
  input changed. Builders, Campaign simulation and Step 10 were therefore not
  rerun; their prior approved baselines remain authoritative.
- The actual developer save remained schema 3 revision 30 with SHA-256
  `77AF3ED2CFBB13591E2119FD77F94C68A5BFC488CF4DB62C2FDCC52C8B443B50`.

## Iteration 21 validation result

- Campaign Builder completed two consecutive passes and regenerated the
  Ticket-first failure UI with all required references.
- Full EditMode passed `408/408`; full post-Builder PlayMode passed `218/218`.
- The actual Editor Product save remained byte-, timestamp- and hash-exact at
  schema 2 revision 29 with SHA-256
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`.
  Schema-3 migration was validated only with isolated saves.
- Campaign and Step 10 simulations were not rerun because Core gameplay,
  Stage data, generation, timing and deterministic inputs are unchanged; the
  previously approved hashes remain authoritative.

Version

Iteration 20

(Monetization Platform Baseline)

---

- Unity IAP `5.4.2` is the approved standard Google Play billing package and
  Android now uses `com.wscho.colorgaterunner`.
- No Store, product catalog, runtime purchase service, receipt grant, Firebase
  SDK, or Shop UI is claimed by this baseline.
- Full EditMode passed `400/400` and full PlayMode passed `215/215` after the
  final application-ID change. No Builder or deterministic simulation was
  required because no Scene, gameplay rule, content, or simulation input
  changed; all prior Campaign and Step 10 hashes remain authoritative.
- The actual Editor Product save remained schema 2 at revision 29 with Guest
  ID `2dfe4f6ffa914e7a95e2fd30de5b6307`, Highest Stage 14, 13 Stage records,
  1,500 Coins, 3 Shields and 3 Boosters. Its SHA-256 is
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`;
  its last-write time predates both final validation suites.

Version

Iteration 19

(Continue Economy Policy)

---

- Campaign failure now offers authorized Continue sources rather than the
  legacy free button. Coin prices are `300 / 600 / 900` by successful Coin
  ordinal, with one completed rewarded-ad right and a total cap of three
  Continues per attempt. Retry resets the attempt-local policy.
- Coin spend is an atomic, idempotent schema-2 Product transaction before Core
  resume. Insufficient funds, save failure, failed/cancelled/unavailable ads,
  duplicate callbacks and stale callbacks preserve the frozen failure state.
  An unavailable release provider is hidden and never grants a fake success.
- The Campaign Builder command built twice and validated successfully. Full
  EditMode passed `400/400`; final post-Builder PlayMode passed `215/215`.
- Two 400-row Campaign simulations were byte-identical with Summary, JSON and
  CSV hashes `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`,
  `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`,
  and `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`.
  Step 10 remained unchanged.

Version

Iteration 18

(Consumable Owned Start Items)

---

- Stage 8+ selectable Shield and Booster now consume one owned unit per
  selected item in one atomic successful save before Countdown. Starting with
  no selected item performs no inventory write.
- Zero stock disables the corresponding toggle. Stale insufficient inventory
  and save failure remain in PreRun with truthful status and no partial
  published decrement. Retry consumes again for the new attempt; Back before
  Start consumes nothing; duplicate Start consumes once.
- Stage 6/7 provided items remain free. Missing Product context cannot grant a
  selectable item for free, while no-item and provided-item starts remain
  available. Product save schema remains 2 with no migration.
- Targeted Product tests passed `30/30`; targeted PlayMode tests passed `5/5`.
  The Campaign Builder command built twice and validated the generated Scene.
  Full EditMode passed `372/372` and full post-Builder PlayMode passed
  `207/207`.
- Campaign and Step 10 simulations were omitted under Tier 2 because Core,
  stage data and deterministic simulation inputs did not change. All
  Iteration 17 artifact hashes remain authoritative.

Version

Iteration 17

(Campaign Learning Curve Rebalance)

---

- Catalog revision 7 preserves stable IDs and Stage 1–5 exact simulation
  results while replacing the Stage 6–20 learning order.
- Stage 6/7 provided-item labels take precedence over locked selection;
  Stage 8 is the first selectable-item application Stage.
- Camouflage, Fog, Ice and Echo now use contiguous intro/practice/mastery
  blocks. Hidden and Flicker Campaign bindings are deferred to Stages 21+;
  their Experiment implementations and tests remain.
- EditMode `367/367`, post-Builder PlayMode `195/195`, and two Campaign
  Builder runs passed. The 400-row Campaign report reproduced byte-identically
  twice with zero continuity or pool violations.
- Save-failure PlayMode coverage now arms the exact next write, verifies the
  user-visible failure state, and no longer expects a log that the product
  contract never emits.

Version

Iteration 16

(Campaign Act 2: Stage 14–20)

---

- Catalog revision 6 adds stable IDs `stage-14` through `stage-20`, all using
  the existing Red/Blue/Green cycle and existing Shield/Booster selection.
- Stage 14 is a shorter unmodified recovery stage. Stages 15–20 revisit one
  existing primary mechanic each: Camouflage, Fog, Ice, Echo, Hidden and
  Flicker. No stage enables mixed modifiers or a new tutorial grant.
- Campaign Builder generated 20 Stage buttons in two consecutive passes. A
  two-percent normalized gap between current-stage and title text fixes the
  overlap exposed by portrait reference-resolution validation.
- Targeted EditMode `3/3`, full EditMode `368/368`, and final post-Builder
  PlayMode `186/186` passed. No Missing Script, Missing Reference, duplicate
  root, EventSystem or Build Settings regression was reported.
- Campaign simulation now contains 400 rows. Its prior Stage 1–13 CSV rows and
  JSON result prefix are byte-exact, while the approved full hashes are owned
  by the Authoritative Baseline above. Step 10 was not rerun because shared
  mechanic rules, Experiment inputs and player profiles did not change.
- Mechanical Average/No Item first-clear rates are `23.6%` at Stage 14,
  `15.9%`, `15.8%`, `12.9%`, `24.8%`, `13.4%`, and `12.6%` through Stage 20.
  Stage 18 deliberately forms an Echo relief beat. These figures do not prove
  fun, readability, fairness or launch balance.

---

Version

Iteration 15

(Automatic Lobby Progression Foundation)

---

- Schema 2 adds stable-ID Campaign records, Economy wallet/inventory and
  idempotent transaction IDs, plus applied/presented Lobby milestone state.
- Production Boot imports legacy Campaign PlayerPrefs once without deleting
  them. Stage clear then persists record, unlock and first-clear rewards in one
  Product save transaction; failed writes publish no partial state.
- Frontend Lobby shows wallet, Shield/Booster inventory, theme progress, next
  automatic upgrade target, and pending reward summary. Every two first clears
  advances one of 18 milestones across the planned 36-Stage campaign.
- Frontend-origin Clear/Failure home actions return to Frontend Lobby so the
  earned upgrade is visible. Direct and development entry keeps the legacy
  Campaign Lobby route.
- Product-focused EditMode `25/25`, Frontend PlayMode `16/16`, full EditMode
  `365/365`, and post-Builder PlayMode `186/186` passed. Frontend Builder passed
  twice with no missing reference, duplicate root or Build Settings failure.
- Campaign and Step 10 simulations were not rerun under TEST_PLAN Tier 2:
  Stage definitions, deterministic generation, timing, judgment and balance
  inputs did not change. Their approved hashes remain authoritative.

---

Version

Iteration 14

(PreRun Back Navigation / Settings Toggle Presentation Hotfix)

---

- A successfully consumed Frontend Campaign launch records a Scene-local,
  non-persistent entry origin. PreRun Back reuses the serialized Frontend path
  and existing Scene loader without activating the legacy Campaign Lobby.
- Missing launch context, direct SampleScene, development, and Experiment
  paths retain their existing Campaign Lobby behavior.
- Return input is blocked while loading. Failure stays in PreRun, reports the
  error, restores the Back action, and permits one clean retry.
- The shared Settings Builder now creates one `StateLabel` per toggle. The
  controller updates its text and color on open, click, save, re-entry, and
  save reload; legacy overlapping `OnLabel` and `OffLabel` objects are absent.
- EditMode `361/361` and post-Builder PlayMode `184/184` passed. Frontend and
  Boot were rebuilt twice through the Frontend Builder, and Campaign Builder's
  internal two-pass build succeeded.
- Campaign remains 260 rows and Step 10 remains 80 rows / 64,016 runs with all
  eight approved hashes unchanged. Packages and non-volatile ProjectSettings
  semantics are unchanged.

---

Version

Iteration 13

(Lobby Consolidation / Gameplay Settings Entry / Unified Settings Access)

---

- Frontend now recommends the current Campaign Stage from the existing
  device-wide PlayerPrefs store through a read-only adapter. The AppRoot-owned
  one-shot stable-ID request enters the existing PreRun/item-selection flow
  without displaying Campaign Lobby in production.
- Direct Campaign, development, Experiment Lab, and missing-context entry keep
  the existing Campaign Lobby as a safe fallback.
- Frontend and Pause share Notifications, Music, SFX, Vibration, and external-
  link Settings. Master stays hidden/preserved; Music/SFX restore their last
  non-zero values; notification delivery and content-specific audio remain
  unimplemented and are presented truthfully.
- Pause remains owned by the existing coordinator. Its generated HUD button
  is inside Safe Area, at least 44 by 44, clear of Stage/Shield HUD, and does
  not leak clicks to gameplay.
- Full EditMode `361/361` and post-Builder PlayMode `180/180` passed. Frontend,
  Boot, and Campaign Builders each passed twice with no Missing Script,
  Missing Reference, duplicate root, or EventSystem regression.
- Campaign remained 260 rows and Step 10 remained 80 rows / 64,016 runs with
  all eight approved hashes unchanged. Package hashes are unchanged.
- WebGL `APP_UI_EDITOR_ONLY` and `SENTIS_ANALYTICS_ENABLED` are recorded as
  package-managed volatile defines and excluded from the feature commit; no
  other ProjectSettings semantic change is allowed.

---

Version

Iteration 12

(Account Onboarding / Auto Lobby / Settings / Gameplay Pause UX implemented)

---

Account Onboarding, Settings, and Gameplay Pause Result

- `AccountChoiceCompleted` is Profile onboarding state, not a Settings value.
  Missing schema-1 fields decode as `false`; Settings repair cannot reset it.
- A thin `LocalProductSession` clones the current save, delegates persistence
  to the existing Save service, and publishes/rebinds state only after write
  success. Failed account or Settings writes leave public state unchanged.
- Account Choice offers a working local Guest path. Lobby has no Back-to-Title
  route; system Back requests exit confirmation. The unavailable Google action
  is hidden in release UI.
- One `SettingsPanelController` contract is used in Frontend and Gameplay.
  Apply persists Master/Music/SFX/Vibration; Music and SFX await real content
  targets, while Master and Booster vibration are applied now.
- `GameplayPauseCoordinator` is the sole Pause/modal/transition owner. Pause
  uses a full-screen raycast-blocking 0.85 alpha Dim and only pauses the two
  Builder-registered attempt ParticleSystems.
- Campaign-to-Frontend return uses a Builder-serialized active Scene path.
  Load failure stays paused with a recoverable error instead of recording a
  Stage result.
- Full EditMode `352/352`, post-Builder PlayMode `179/179`, all three Builders
  twice, Campaign 260-row hashes, and Step 10 80-row/64,016-run hashes passed.
  Packages and ProjectSettings are unchanged.

---

Version

Iteration 11

(Campaign Progress Restore Hotfix: PlayMode PlayerPrefs isolation and
restart/re-entry regression coverage implemented)

---

Campaign Progress Restore Hotfix Result

- Campaign progress remains device-wide Unity `PlayerPrefs` owned only by
  `PlayerPrefsStageProgressStore`. It is not linked to the Product Guest and
  is not copied, reset, migrated, or written into `product-save.json`.
- Investigation found that two PlayMode fixtures deleted the real Editor
  `HighestUnlocked` and Stage 6 Record keys during cleanup without preserving
  their previous values. Windows Editor project copies share this PlayerPrefs
  namespace when Company and Product names match.
- All Campaign-affecting PlayMode fixtures now capture Highest Unlocked plus
  every Stage 1-13 Record before setup work and restore exact key existence,
  integer/string values, and `PlayerPrefs.Save()` in exception-safe cleanup.
- Regression coverage now proves Stage 11 first entry and re-entry, Lobby /
  PreRun / Gameplay stable-ID parity, fresh Stage 1 clear followed by Stage 2
  after controller recreation, new Guest preservation of existing Campaign
  progress, and missing/corrupt Highest fallback without Record deletion.
- Values deleted before this investigation cannot be reconstructed from
  evidence. The missing prior Highest Unlocked and Stage 6 Record are not
  guessed or automatically repaired.
- Profile-scoped Campaign progress remains deferred to the separately approved
  `StageProgressService` Iteration. A future testability improvement may inject
  an in-memory Campaign store, but this Hotfix does not alter runtime storage.

Iteration 11 Validation

- Snapshot Utility tests: `3/3`; Campaign restore regression tests: `4/4`.
- EditMode: `335/335`; PlayMode: `167/167`; post-Builder PlayMode: `167/167`.
- Frontend Builder, Boot Builder, and Campaign Builder each succeeded twice.
  Builder and PlayMode validation found no duplicate generated objects,
  Missing Script, Missing Reference, or required-reference failure.
- The actual Editor Campaign PlayerPrefs snapshot retained 11 existing entries
  and SHA-256
  `2A8AF71352FCCF9D80C8BDCB9BDC89FC3F61A7B356633C3423A847F576D0B6C9`
  before and after the test runs. The Product save file hash also remained
  unchanged, preserving the current Guest ID.
- Campaign simulation remains 260 rows with all three approved hashes. Step 10
  remains 80 rows / 64,016 runs with all five approved hashes.
- `ProjectSettings.asset`, Package manifest/lock, Scene assets, runtime
  Campaign code, Stage Catalog, balance, and Product Save Schema are unchanged.

---

Commercial Flow Foundation Frontend Iteration 2 Result

- `Frontend.unity` is Build Index 1 between Boot and the existing Campaign
  `SampleScene`. Boot serializes Frontend as its destination; Frontend
  separately serializes the existing active Campaign path. Runtime does not
  hardcode the `SampleScene` name.
- One `FrontendPageRouter` owns the Title/Lobby Page, blocking Modal, and
  Transition state. Page Views do not activate one another or load Scenes.
- Title shows the initialized Guest display name, truthful `GUEST` state,
  version, account-unavailable notice, and read-only Settings summary. Empty
  Terms/Privacy/Support actions are hidden.
- Placeholder Lobby shows `CONTINUE CAMPAIGN` without reading or estimating
  Stage progress. Currency, Event, Notification, and Lobby-theme slots remain
  inactive and consume no layout space.
- Frontend contains no AppRoot. It captures an immutable display Context from
  the Boot-initialized AppRoot. Direct Frontend execution without AppRoot
  displays blocking `BOOT REQUIRED` and disables Campaign entry.
- Lobby Back returns to Title, Title Back opens Exit Confirmation, a
  cancellable Modal consumes Back, and Transition state ignores Back and
  blocks duplicate input.
- Campaign entry loads the existing Scene once and retains its current Lobby,
  PreRun, Gameplay, result, progress, and return contracts. No Campaign-to-
  Frontend route was added.
- The approved portrait/Input baseline was first restored in isolated commit
  `c191826`; this Iteration does not modify `ProjectSettings.asset` again.

Iteration 10 Validation

- EditMode: `335/335` passed. PlayMode: `160/160` passed. The same counts pass
  after all final Builder runs.
- Frontend Builder succeeded twice, Boot Builder succeeded twice, and the
  Campaign Builder completed its two-build command successfully. Required
  references, Safe Area, one EventSystem, one primary Page, unique generated
  roots, and no Missing Script were validated.
- Stage 1-13 Campaign simulation remains 260 rows and preserves Summary/JSON/
  CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and preserves all five approved hashes. Package
  manifest/lock hashes remain unchanged. The only ProjectSettings change in
  this Iteration is adding Frontend to `EditorBuildSettings.asset`.
- Human review remains required for 9:16 small-screen readability, Boot-to-
  Title and Lobby-to-Campaign transition feel, Android Back behavior, UI input
  leakage on devices, and the intentional new-Lobby/old-Lobby UX handoff.

Deferred after Iteration 10

- Final Lobby, Campaign Page, Stage Detail, Gameplay/Results shells,
  StageProgressService integration, Campaign-to-Frontend navigation, external
  account/cloud, economy, events, analytics, ads, IAP, and final visual art.

---

Commercial Flow Foundation Iteration 1 Result

- `Boot.unity` is Build Index 0 and initializes one persistent `AppRoot`
  before loading the first active non-Boot Build Settings Scene. The serialized
  destination is `Assets/Scenes/SampleScene.unity`; runtime contains no
  `SampleScene` name hardcode.
- `AppRoot` owns one explicit service graph: Clock, Profile, Local Account,
  Settings, Local Save, and the ordered initialization pipeline. Individual
  services are not singletons and no mutable service locator was added.
- The new `ColorGateRunner.Product` assembly has no Unity reference.
  `Application.persistentDataPath`, `JsonUtility`, Scene loading, and Boot UI
  stay in the Unity Presentation adapter.
- A fresh local product save creates exactly one Guest with an injected-capable
  UUID and UTC clock. Reload preserves the Profile ID and updates Last Played.
  Device identifiers are neither read nor stored.
- Product save schema 1 contains Profile, Settings, SaveRevision, and
  LastWriteUtc. Candidate revision and timestamp are applied before temporary
  serialization and validation. Primary/temp/backup, corrupt-primary recovery,
  supported schema-0 migration, and future-version blocking are implemented.
- Desktop/native uses atomic replace when supported. WebGL explicitly selects
  backup-based recoverable replacement and never reports an atomic result.
- `PlayerPrefsStageProgressStore` remains the sole Stage progression owner.
  Product save has no Stage section, does not read/write progression keys, and
  performs no copy, migration, reset, or new Stage result write.
- Boot shows Loading, version, a blocking local-error panel, and Retry. Retry
  reuses the existing service graph; returning to Boot rejects duplicate
  `AppRoot` creation.

Iteration 9 Validation

- EditMode: `313/313` passed.
- PlayMode: `152/152` passed; the same `152/152` passed after Boot Builder and
  again after two Campaign Scene Builder runs.
- Boot Builder succeeded twice. Campaign Scene Builder succeeded twice. Both
  validate required references, Safe Area, one EventSystem, and no Missing
  Script; PlayMode validates the existing Campaign Scene after regeneration.
- Stage 1-13 Campaign simulation produced 260 rows and reproduced Summary,
  JSON, and CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 reproduced 80 rows, 64,016 runs, and all five Iteration 8 hashes.
- Package manifest and lock hashes remain unchanged. The only approved
  ProjectSettings diff is the Boot-first `EditorBuildSettings` Scene list.
- Human review remains required for Boot presentation, transition feel,
  understandable save-error text, restart persistence on Android/WebGL, and
  preservation of real device Stage progress.

Deferred after Iteration 9

- Title, Frontend/Lobby redesign, Campaign Page, Stage Detail, Gameplay shell,
  Results redesign, StageProgressService integration/migration, external
  login/cloud, economy, events, analytics, ads, and IAP.

Previous Iteration 5 Result

- `StageCatalog.asset` is the single production source for all 11 campaign
  stages. Runtime, simulation, and tests consume its pure-Core adapter through
  `IStageCatalog`.
- Clone gameplay has been removed. Echo is a modifier on an ordinary gate and
  does not add gates, positions, collision paths, or a second generator.
- Camouflage reveal uses effective-speed ETA, transitions once, never hides
  again, and always keeps ordinary judgment active.
- Stages 1-5 lock both start items in catalog data and the PreRun UI.
- Stage 6 provides one local Shield at start; Stage 7 provides one local
  Booster at start. Both activate after the same countdown boundary as an
  equivalent selected item, and the picker prevents duplicate selection.
- Player movement crossing an unresolved gate plane reuses the existing
  one-shot `TryResolveCrossing()` path. This prevents high-speed Booster
  movement from tunneling past the thin physics Trigger without adding a
  second judgment system or increasing the six-gate pool.
- Stages 8, 9, 10, and 11 teach Camouflage, Fog, Ice, and Echo respectively.
  Stages 6-10 use two active colors to isolate the new mechanic; Stage 11
  returns to three colors for the finale.
- Stage speed Start/Max/Curve, cadence, active color count, mechanic,
  Echo/Camouflage values, seed, and local grant are Inspector-authored.
- The stage picker is catalog-driven and contains 11 stable-ID buttons in a
  two-column portrait layout.

Iteration 5 Validation

- EditMode: 249 / 249 passed.
- PlayMode: 128 / 128 passed.
- Stage 7 PlayMode starts Booster at `GO`, crosses all 40 planned gates at
  runtime speed, retains the fixed pool, and reaches Goal/StageCleared.
- The requested Stage 6-11 simulation subset contains 120 rows and 96,024
  modeled runs: six stages, five profiles, four item inputs, Perfect once and
  every stochastic condition 1,000 times.
- Stages 6 and 8-11 have zero changed rows versus the preceding campaign
  baseline. All 20 Stage 7 rows change only under the approved start-Booster
  contract.
- Stage 7 Average/None changes mechanically from first-clear
  `47.6% -> 47.0%`, Continue-clear `84.8% -> 95.1%`, median clear time
  `33.203s -> 31.356s`, and average Booster bypasses `9.780 -> 15.774`.
  These are not claims about fun or comfort.
- All 24 Stage 6-11 Perfect/item rows clear, and all 120 rows report zero
  Booster/Continue displacement, gate-index gap/duplicate, cursor reset, and
  full-pool reset violations.
- A fresh Step 10 run reproduced all five baseline hashes exactly.
- The full refreshed Stage 1-11 matrix is stored in the existing Mechanic
  Campaign artifacts.
- Core keeps `noEngineReferences: true`; no package or ProjectSettings feature
  change was introduced.

Stage Progression Hotfix

- Fixed a stale stable-ID reference in `ShowLobby()`. The lobby previously
  displayed the newly unlocked stage number but `PlayFromLobby()` could still
  start the prior stage ID.
- The displayed `StageDefinition.StageId` is now synchronized whenever lobby
  progression selects a stage.
- PlayMode explicitly verifies displayed Stage 2 starts Stage 2 and displayed
  Stage 3 starts Stage 3.

Test Build Tooling

- `Tools > Color Gate Runner > Test Builds` provides Android APK, WebGL,
  combined Android-plus-WebGL, and output-folder commands.
- Both platforms use the enabled `EditorBuildSettings` scenes and Development
  mode. Outputs stay under ignored `Builds/Test`; Android always produces an
  APK and restores the previous App Bundle setting afterward.
- Every platform command refreshes AssetDatabase, Stage Catalog, Campaign,
  Frontend and Boot before packaging, then removes that platform's previous
  output. A failed invocation therefore cannot leave an older APK or WebGL
  folder looking current.
- WebGL test builds temporarily use a `540 x 960` logical canvas and the
  project-owned `ColorGateRunnerPortrait` template. Its browser container
  preserves an exact `9:16` aspect ratio with neutral letterboxing instead of
  stretching to a square or landscape host.
- The build path restores the prior WebGL width, height, and template settings
  after success or failure, so Android and persistent ProjectSettings remain
  unchanged.
- Missing scenes, missing platform support, platform-switch failure, and
  unsuccessful `BuildReport` results fail explicitly instead of reporting a
  false success.
- Generated Android Gradle projects force Kotlin stdlib `1.8.22` and exclude
  the legacy split jdk7/jdk8 artifacts that otherwise duplicate classes with
  current Google dependencies. The postprocessor is idempotent and repairs a
  cached pre-`plugins` insertion without deleting the whole build cache. Its
  callback remains registered when the Editor starts on WebGL.
- Build-menu, portrait-template and Gradle-upgrade EditMode coverage passes
  `7/7`; the full current EditMode suite passes `450/450`.
- The current combined menu-driven Android build completed and produced a
  68,774,305-byte APK. Installation and target-device behavior remain manual.
- The same command produced a 155,566,553-byte WebGL output folder. Its
  generated page contains a `540 x 960` canvas and exact
  `9:16` frame rules. Browser play, input, resizing and host-server behavior
  remain human acceptance checks.

Authoritative Iteration 6 Result (now Hidden)

- The memory mechanic then named Flicker, now named Hidden, is a flag on the
  shared ordinary-gate `GateModifier`. It adds no gate, generator, collision
  path, judgment path, or pool object.
- Its settings, now `HiddenSettings`, validate the approved
  Inspector-authored fields. The
  Launcher converts its serialized values into the pure-Core settings and
  disables Booster for Hidden-only runs.
- The existing deterministic Experiment sequence selects stable Hidden gate
  IDs without changing color/spacing PRNG output, gate count, or Step 10.
- A pure-Core visibility state requires both minimum readable time and shared
  effective-speed ETA, starts one transition, and never reveals target
  information again before judgment.
- The pooled View begins with target color/symbol plus a persistent `HIDDEN`
  identity. Hide blends to the existing neutral silhouette and removes only
  target symbol information; geometry, judgment opening, transform, collider,
  and marker remain.
- Player, held Echo, Shield, and ordinary failure reuse the existing priority.
  Retry/Replay reproduce selection and reset visibility state.
- Campaign Stages 1-11 and the 16-condition Step 10 matrix are unchanged.

The initial Hidden settings are Inspector-authored human-play candidates:
eligible progress `0.15-0.85`, chance `0.35`, cooldown `2`, visible duration
`1.00s`, hide lead `0.65s`, transition `0.12s`, maximum `4`, and guaranteed
first occurrence. Booster is disabled for this standalone condition.

Iteration 6 Validation

- Original open Editor: scripts compiled without C# errors.
- EditMode: 262/262 passed.
- PlayMode: 134/134 passed.
- Scene Builder: the command's two consecutive builds succeeded; the rebuilt
  isolated scene then passed PlayMode 134/134 again.
- Campaign simulation: 220 rows and 176,044 modeled runs. Summary, JSON, and
  CSV hashes are byte-identical to the preceding baseline.
- Step 10: all five CSV/JSON/summary/comparison/shortlist hashes are
  byte-identical to the approved baseline.
- Packages and ProjectSettings have no intended feature change. Automated
  results do not establish Hidden readability, comfort, comprehension,
  fairness, satisfaction, motivation, or fun.

Iteration 7 Baseline

- Clean worktree and `git diff --check`.
- EditMode 262/262 and PlayMode 134/134.
- Echo and former-Flicker targeted PlayMode cases 12/12.
- Campaign Summary/JSON/CSV and all five Step 10 hashes match Iteration 6.
- Package manifest/lock and ProjectSettings diffs are empty.

Iteration 7 Approved Contract

- Existing `Flicker = 16` gate modifier and mechanic value `5` become Hidden
  with identical selection, hide, judgment, View, and Retry/Replay behavior.
- New Flicker uses new enum values and cycles two or three unique active
  colors. The collision-time color from
  `ExperimentSession.ElapsedPlayingSeconds` is authoritative.
- Gate plans fix cycle colors, interval, deterministic phase, pulse, base
  color, Gate ID, and seed-derived data. View and judgment share the same Core
  active-color calculation.
- Minimum visible cycles are checked from existing deterministic speed,
  spacing, and six-View exposure. Booster is disabled.
- Player → Echo → Shield → Failure remains unchanged.
- Campaign, music/BPM/DSP, Hidden/Flicker or other modifier combinations,
  speed-based interval adjustment, new colors/input, and Stage 6-11 work are
  out of scope.

Iteration 7 Result

- The old modifier and mechanic numeric values `16` and `5` now deserialize
  as Hidden. New Flicker uses values `32` and `6`.
- Every former Launcher field has explicit `FormerlySerializedAs` migration
  metadata. New Flicker fields use a distinct `colorCycleFlicker` serialized
  prefix, so no old field name can bind to the new mechanic.
- Hidden preserves the former seed-selected gate IDs, ETA-based one-way hide,
  neutral silhouette, ordinary judgment, held Echo/Shield behavior, and
  Retry/Replay reset.
- Human-play feedback raised the Hidden default hide lead from `0.65s` to
  `0.85s`. The `1.00s` minimum observation and `0.12s` transition remain.
  Level tuning treats shorter Camouflage reveal time as harder but longer
  Hidden memory time as harder.
- Flicker remains an ordinary gate modifier. Its plan fixes the base color,
  every active-palette color in player tap order, interval, deterministic
  phase offset, pulse, Gate ID, and selection seed without changing gate
  count. The authored `2/3` cycle-count setting is removed.
- `FlickerCycleCalculator` is the single pure-Core phase path used by the
  View, collision-time judgment, simulation, and tests. An exact interval
  boundary uses the new phase.
- Experiment Gameplay Time advances only in Playing, freezes in Countdown,
  Failed, and StageCleared, and resets on Retry/Replay.
- Collision priority remains Player, held Echo, Booster if already present in
  a constructed session, Shield, then ordinary failure. The Launcher disables
  Booster for both Hidden-only and Flicker-only runs.
- The pooled View shows `HIDDEN` for the memory mechanic and `FLICKER` for the
  color cycle. Flicker color and symbol derive from the same Core color, with
  only a short brightness pulse above the immediate logical change.

Iteration 7 Validation

- EditMode: 281/281 passed.
- PlayMode: 143/143 passed before Scene Builder and 143/143 passed again after
  two consecutive successful Scene Builder runs.
- Related passed cases: Echo 27/27, Shield 29/29, Camouflage 9/9, Hidden
  25/25, Flicker 27/27, and Experiment Runtime 5/5.
- The full Stage 1-11 campaign regression wrote 220 matrix rows, including
  the requested Stage 1-5 coverage; Summary, JSON, and CSV hashes are
  byte-identical to the Iteration 6 baseline.
- Step 10 wrote 80 rows and 64,016 runs; all five
  CSV/JSON/summary/comparison/shortlist hashes are byte-identical.
- The rebuilt scene has no Missing MonoBehaviour, required-reference, pool,
  duplicate-root, or Lab return/re-entry failure in the final PlayMode suite.
- Package manifest/lock and ProjectSettings have no intended change.
- Automated validation does not establish switching comfort, concept
  comprehension, boundary readability, fairness, satisfaction, or fun.

Hidden/Flicker Human-Feedback Revision Validation

- Baseline EditMode `283/283` and PlayMode `143/143` passed.
- Final EditMode `288/288` and PlayMode `144/144` passed. Hidden/Flicker
  focused coverage is EditMode `14/14` and `20/20`, PlayMode `6/6` and
  `10/10`.
- Scene Builder completed twice. The regenerated Launcher serializes Hidden
  `0.85s` and contains no Flicker cycle-count field; post-Builder PlayMode is
  `144/144`.
- Stage 1-11 Campaign simulation wrote 220 rows and retained all three hashes.
  Step 10 wrote 80 rows and 64,016 runs and retained all five hashes.
- Package manifest/lock and ProjectSettings have no intended change.

Authoritative Iteration 8 Result

- Campaign now contains 13 stable catalog stages. Stage 12 `HIDDEN MEMORY`
  has 50 gates and Stage 13 `FLICKER FLOW` has 52; both use Red, Blue, Green
  and retain selectable Shield and Booster.
- Stage 12 uses the approved Hidden `0.85s` lead and deterministically selects
  Gate IDs `8, 19, 22, 26`. Stage 13 deterministically selects Flicker Gate
  IDs `8, 14, 19, 22`.
- `FlickerGatePlan` and `DeterministicModifierPlanner` are shared by
  Experiment and Campaign. Runtime, View, tests, and simulation use the same
  absolute Gameplay Time phase calculation.
- Campaign Flicker collision resolves the active color at
  `StageSession.ElapsedPlayingSeconds`. Booster remains the existing
  start-at-`GO` auto-pass and receives no Hidden/Flicker exception.
- Flicker exposure is accepted only when six pooled-view spacings provide at
  least three `0.50s` cycles at the authored Booster speed. Item choice does
  not alter targets, cycles, or phase.
- Catalog revision 5 and the rebuilt Scene contain 13 buttons. Scene Builder
  completed twice and post-Builder PlayMode passed.
- Final EditMode is `294/294`; PlayMode is `147/147` before and after the
  second Builder run.
- Stage 1-13 simulation wrote 260 rows. Its first 220 Stage 1-11 rows are
  exactly equal to the previous JSON results. Perfect clears Stage 12 and 13
  under all four item combinations.
- New 13-stage artifact hashes are Summary
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  JSON
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and CSV
  `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and 64,016 runs with all five hashes unchanged.
  Package manifest/lock retain their prior hashes and ProjectSettings has no
  diff.
- Average/None first-attempt clear is `26.9%` at Stage 11, `16.2%` at Stage
  12, and `17.6%` at Stage 13. This mechanical drop is not auto-tuned and
  requires human difficulty review.

Next Decision

Create fresh Android and WebGL test players from the current implementation
HEAD, then run a focused portrait-device/WebGL smoke and visual-feel audit. The
existing player artifacts predate Iterations 46–60, so choosing another feature
iteration before validating the current integrated product would compound
release-facing uncertainty. Do not begin Stage 27+, live commerce, ads or
additional Lobby systems until that human evidence is reviewed.

---

Completed / Historical Milestones

Stage 1~5

Experiment Lab

Continue

Retry

Goal

Booster

Shield

Camouflage

Fog

Ice

Experiment Countdown

Experiment Failed / Completed Results

Experiment Retry / Replay

Stage 4 Runtime / Simulation Color-Cycle Parity

Campaign Movement / Spatial Scale 2x

Superseded Clone-only Experiment

Superseded Clone Source / Relationship Metadata

Superseded Clone Failure Cause

Superseded Clone Shield / Camouflage Integration

---

Current Decisions

Maximum Tap = 4

Mechanics unlock by Stage

Experiment uses deterministic stages

Experiment uses one explicit StageFlowState

Experiment Retry / Replay preserves condition and seed

Experiment never reads or writes campaign progress

Campaign speed and authored world distances scale together

Campaign cadence and approximate stage duration remain unchanged

Experiment Lab movement values remain unchanged

Echo uses ordinary gates and never changes gate count

Echo Camouflage uses ordinary ETA hide, reveal, and judgment

Echo offers are deterministic and non-stacking

---

Iteration 4 Delivered

Architecture Foundation

Echo Modifier

Camouflage ETA Reveal

Stage Speed Profiles

Campaign Stages 6–11

---

Known Issues and Human Review

- No reproducible current code defect is confirmed by the `2026-09-12`
  automated suite.
- The current Iteration 60 implementation has no matching Android/WebGL player
  build, so installation, browser startup, touch/focus/audio, safe-area behavior
  and device performance remain unverified.
- Fog density/readability, Ice shimmer and steering response, Echo/Hidden/
  Flicker comprehension and fairness, camera comfort, modular runner-form
  distinctness, victory-firework timing, Lobby hierarchy, theme readability,
  Shop package-volume comprehension and Journey/League clarity require human
  portrait-device and WebGL review.
- Rank and Collection are deliberately truthful Coming Soon destinations, not
  missing or simulated backends. Live IAP, receipt validation, rewarded ads,
  cloud/account providers, analytics/crash providers, notification delivery,
  production audio and Stages 27+ remain deferred systems rather than bugs.
