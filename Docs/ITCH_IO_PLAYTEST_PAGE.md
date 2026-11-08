# itch.io Playtest v0.1 페이지 초안

게시되지 않은 초안이다. 최종 빌드 검증과 Game Director 검토 후 실제 확인한 브라우저 및 배포 주소를 반영한다. 아래 문구는 분석 수집이 꺼진 후보용이며 UGS 연결 후 그대로 재사용하지 않는다.

## 영문 페이지 문구

**Color Gate Runner — Playtest v0.1**

Change your color. Match the gate. Keep running.

Try eight short stages that introduce color matching, Shield, Camouflage, Hidden, Echo and Flicker. This early playtest helps us learn whether the rules and feedback make sense to new players.

**Controls:** Click or tap to change your runner's color. Match the gate before crossing it. The runner moves forward automatically. Use Pause to take a break.

Retries are unlimited. There are no purchases, ads or account sign-ups. Progress lasts for this running playtest; refreshing the page starts a new run.

A successful run targets about 5–10 minutes; first attempts may take longer. Local Chromium WebGL launch, input and retry checks have passed. Full-session timing and itch.io-hosted compatibility still need validation. Mobile browser support has not been confirmed.

**Feedback:** Was the color-matching rule clear? Did you understand why you failed? Which stage made you want to retry—or stop? Share your feedback through the invitation's feedback channel. Please do not include personal information in your feedback.

**Gameplay data:** Analytics is not connected in this candidate. No custom gameplay events are sent to Unity Analytics. itch.io and your browser may process their own technical information; see their respective privacy notices.

## UGS 활성화 전 필요한 문구와 정보

- 게임 운영 주체 및 개인정보 문의 연락처, 게시된 안내 URL.
- 목적: 규칙 이해와 Stage별 실패·재시도·진행 분석.
- 범주: 빌드/경로/Stage 정보, 시도 횟수, Gate와 기믹, 활성 플레이 시간, SDK가 자동 부여하는 식별자 및 기술 메타데이터.
- 공급자: Unity Analytics. 완전 익명으로 표현하지 않는다.
- 보존 기간·처리 지역은 실제 서비스 설정/계약에서 확인한 값만 기재한다.
- 게임 내 동의 거절, 이후 철회, 공급자 데이터 삭제 요청 경로.
- 이름·이메일·직접 수집 IP·자체 기기 ID·Profile/Guest ID·자유입력 글은 게임 이벤트에 넣지 않는다.
- 동의한 표본과 전송 손실로 인한 측정 한계를 설명한다.

Unity 6.2+/Analytics SDK 6.1+의 동의 API와 삭제 순서는 [Unity SDK 개인정보 제어](https://docs.unity.com/en-us/analytics/privacy-and-consent/manage-data-privacy)를 따른다. SDK 자동 이벤트/공통 식별자는 [Unity 이벤트 기록 문서](https://docs.unity.com/en-us/analytics/events/record-event)를 참고한다. 이 초안은 실제 연결 상태·안내 주소·보존 정책이 확정되기 전 게시용 개인정보 고지가 아니다.

## 배포 준비

- 가격: No payments. 수익화 OFF.
- 1차: 10–20명 제한/비공개 테스트. 공개 전 핵심 UX/WebGL 문제 수정.
- 커버: 기존 아트 기반 630×500 후보를 별도 검토.
- 스크린샷: 최종 후보의 시작, 일반 게임플레이, Shield, Hidden/Echo/Flicker 중 3–5장을 선정. 개발·경제 화면을 플레이테스트 화면으로 홍보하지 않는다.
- itch.io iframe과 전체 화면 검증을 통과한 ZIP만 업로드한다.

## 무료 웹판(Free Edition) 페이지 초안 (Iteration 75)

전체 캠페인을 무료로 공개하는 판이다. 빌드는 `Tools/Color Gate Runner/Free Edition/Build WebGL Free
Edition` 메뉴가 만드는 `Builds/WebFree/ColorGateRunner-Web-<버전>.zip`이다. 업로드는 사용자가 직접 한다.

### 영문 페이지 문구

**Color Gate Runner — Free Web Edition**

Change your color. Match the gate. Keep running.

Play all 26 stages across eight chapters: color matching, items, Camouflage, Fog, Ice, Echo, Hidden and
Flicker. Every stage has its own soundtrack, from hard rock to calm city pop.

**Controls:** Click or tap to change your runner's color. Match the gate before crossing it. The runner
moves forward automatically. Use Pause to take a break.

**Free to play:** No purchases, no ads, no sign-up and no heart timer. Coins and items are earned only by
playing.

**Saving:** Progress is saved in this browser only. Clearing site data, private windows or a new upload of
the game can reset it.

**Online ranking and data:** The game works offline. When you clear a stage with a new best score, or
open the RANK tab, it signs you in anonymously to Unity Gaming Services and sends your Campaign total score,
so it can show the ranking.
You appear as an automatic name such as `Guest#1234`. See the Privacy notice below.

**Feedback:** Which stage made you want to retry, or stop? Please leave a comment. Do not include personal
information.

### 개인정보 안내 초안 (Iteration 86, 게시 전)

`UgsRankingGateway`/`RankPageView` 기준으로 쓴 초안이다. `[ ]` 항목은 운영자가 정해야 한다. Iteration 86
웹판 확인 결과, RANK 탭을 열기 전에는 외부 요청이 없고, 연 뒤에는 Unity `player-auth`, `social`(이름),
`leaderboards`만 요청한다. 결제 패키지는 Iteration 86에서 뺐다. Iteration 87부터 스테이지 최고점이 오른
클리어 때도 같은 요청으로 점수를 보낸다.

**Privacy notice — Color Gate Runner Free Web Edition**

*Who runs this game:* [operator name], contact: [email address].

*When data is sent:* When you clear a stage with a new best score, and when you open the RANK tab. Nothing
is sent before your first clear.

*What is sent:*
- An anonymous player ID that Unity creates for this browser. It is not linked to your itch.io account,
  email or real name.
- Your ranking name, which is set automatically to `Guest#` plus a number. You cannot type your own name.
- Your Campaign total score.
- Technical information that Unity's services need to answer the request, such as your IP address,
  browser and platform, and the SDK version.

*Who receives it:* Unity Gaming Services (Unity Technologies), which runs the sign-in and leaderboard.
See Unity's privacy policy: https://unity.com/legal/privacy-policy

*Why:* To show the ranking and your place in it. Your Guest name and score are visible to other players
on the RANK tab. The game does not use this data for ads or analytics.

*How long it is kept:* [Until you ask us to delete it, or until the ranking is closed.]

*Your choices:* If you never clear a stage or open the RANK tab, nothing is sent. To delete your ranking entry and
anonymous ID, email [email address] with the Guest name shown on your RANK tab. Clearing this site's
data in your browser removes the local sign-in, but does not delete the entry already on the server.

*Local saving:* Your progress and settings are stored only in your browser (itch.io's game frame) and
are not sent to us.

itch.io and your browser may process their own technical information; see their privacy notices.

#### 한국어 요약 (운영자 확인용)

- 스테이지 최고점이 오른 클리어 때와 RANK 탭을 열 때 Unity로 익명 ID, `Guest#번호` 이름, 캠페인 총점, 요청에 필요한 기술 정보(IP 등)가 간다.
- 이름과 점수는 다른 플레이어에게 보인다. 광고·분석에는 쓰지 않는다.
- 게임 안에는 삭제 버튼이 없다. 삭제는 메일 요청으로 받고, Unity Dashboard에서 플레이어를 지운다.
- 운영자가 정할 것: 이름, 연락 메일, 보관 기간.

### itch.io 설정

- Kind of project: HTML. ZIP 업로드 후 "This file will be played in the browser" 선택.
- Viewport: 540×960, Mobile friendly(세로), Fullscreen button 켬.
- Pricing: No payments. 후원(donation)도 끈다.
- 새 파일을 올리면 게임 주소가 바뀌어 플레이어의 브라우저 저장이 초기화될 수 있다. 업데이트 공지에 밝힌다.
