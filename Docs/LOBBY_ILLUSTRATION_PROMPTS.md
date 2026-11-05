# Lobby Illustration Prompts (Iteration 65)

ChatGPT 이미지 생성으로 로비 일러스트를 만들기 위한 프롬프트와 규격이다. 사용자가 만든
이미지를 받으면 Claude가 같은 파일 이름으로 교체하고 Builder·캡처로 검증한다. 시각 방향의
권위는 `ART_DIRECTION.md`(Iteration 61 네온 밤 디자인)이며, 이 문서는 제작 지시만 담는다.

## 1. 화면 안에서 일러스트가 놓이는 방식

로비 테마 하나는 레이어 3장으로 이루어진다. 진행도에 따라 테마 3개가 차례로 열린다.

| 레이어 | 용도 | 화면 위치 | 파일 형식 |
|---|---|---|---|
| Background | 화면 전체 배경 | 세로 화면 전체 | 불투명 PNG |
| Midground | 가운데 상징물(리액터·비컨) | 가로 8–92%, 위에서 19–73% 높이의 상자 | **투명 배경** PNG |
| Ambient | 은은히 깜박이는 빛 오버레이(불투명도 64–80%) | 화면 전체 | **투명 배경** PNG |

UI가 덮는 영역(배경에서 중요한 것을 두지 않을 곳, 위에서부터 비율):

- 위 4–13%: 상단 자원 바(프로필, 코인, 하트, 설정)
- 위 68–87%: Stage 카드(스테이지 이름, PLAY 버튼)
- 위 87–100%: 하단 메뉴 5칸

따라서 배경의 **주인공 구도는 위 15–65% 구간**에 두고, 아래 35%는 어둡고 단순한 바닥/길이어야
글자가 잘 읽힌다.

## 2. 크기와 전달 방법

- Background: ChatGPT에서 **세로 1024×1536**으로 만든다. Claude가 가운데를 9:16으로 잘라
  941×1672로 맞추므로, **중요한 요소는 가로 가운데 84% 안**에 둔다(좌우 끝 8%씩은 잘린다).
- Midground: **정사각 1024×1024, 투명 배경**. 상징물이 높이의 약 90%, 너비의 약 85%를 채우고
  가운데에 둔다. 아래쪽 받침이 잘리지 않게 한다.
- Ambient: **세로 1024×1536, 투명 배경**. 빛은 화면 가장자리와 가운데 상징물 주변에만 두고,
  가운데 세로 띠(가로 30–70%)와 아래 35%는 비워 둔다.
- 파일은 아래 이름 그대로 저장해서 전달한다. 투명 레이어는 체크무늬가 아니라 실제 투명
  PNG인지 확인한다(다운로드한 파일을 열었을 때 배경이 비어 있어야 한다).

| 테마 | Background | Midground | Ambient |
|---|---|---|---|
| Color Courtyard (첫 테마) | `ColorCourtyard_Background.png` | `ColorCourtyard_Reactor.png` | `ColorCourtyard_Ambient.png` |
| Neon Garden (두 번째) | `NeonGarden_Background.png` | `NeonGarden_Reactor.png` | `NeonGarden_Ambient.png` |
| Sky Festival (세 번째) | `SkyFestival_Background.png` | `SkyFestival_Beacon.png` | `SkyFestival_Ambient.png` |

## 3. 공통 스타일 블록

모든 프롬프트 앞에 아래 문단을 그대로 붙인다. 같은 대화창에서 연속으로 만들면 톤이 더
잘 맞는다.

```text
Mobile game lobby art for "Color Gate Runner", a hyper-casual neon runner.
Style: polished stylized 3D game illustration, cyberpunk city at night,
clean readable shapes, soft bloom, no photorealism, no painterly brush noise.
Palette: near-black ink and deep navy (#05060D, #0A0F1F) as the base, cool
signal-white/cyan light (#C8F4FF), magenta accents (#FF2BD6), occasional warm
gold only for small light points (#FFC83D). Red, blue and green appear only as
the three glowing "color gate" energies. Lighting is restrained: a few strong
neon sources, deep shadows, no daylight sky, no sunset.
Absolutely no text, letters, numbers, logos, watermarks, UI, buttons, frames,
characters or people.
```

## 4. 테마별 프롬프트

### 4.1 Color Courtyard — 첫 테마: 밤의 색 게이트 광장

**Background** (1024×1536, 불투명)

```text
[공통 스타일 블록]
Vertical portrait composition, 2:3. A circular plaza on a high-rise rooftop
in a neon megacity at night, seen from a low front angle. In the upper middle
(15–65% of the height) a round empty platform of dark alloy with a thin cyan
ring, ready to hold a centerpiece; behind it distant skyscrapers with sparse
lit windows and alternating cyan and magenta light strips, a violet haze on
the horizon and a small moon. Two slim gate pillars stand at the left and
right of the platform, one glowing red, one glowing green, and a blue light
under the platform. The lower 35% of the image is a calm, dark, nearly empty
tiled walkway fading into shadow, with only faint cyan guide lines.
Keep all important elements inside the central 84% of the width.
```

**Midground – `ColorCourtyard_Reactor.png`** (1024×1024, 투명)

```text
[공통 스타일 블록]
Isolated game prop on a fully transparent background, centered, front
three-quarter view. A compact "color reactor": a dark alloy circular base with
cyan edge lights, holding a floating faceted crystal core. Three ribbons of
energy, red, blue and green, orbit the core and connect to two short side
pylons. Hard-surface sci-fi design, chamfered edges, small magenta indicator
lights. The prop fills about 90% of the height and 85% of the width, the base
fully visible. No floor, no shadow plane, no background, no scenery.
```

**Ambient – `ColorCourtyard_Ambient.png`** (1024×1536, 투명)

```text
[공통 스타일 블록]
Transparent PNG light overlay for a mobile screen, 2:3 vertical. Only soft
glowing light effects on a fully transparent background: thin cyan light
streaks and small diamond-shaped sparks curving up the left and right edges,
a faint magenta haze in the top corners, a few floating light particles.
Leave the central vertical band (30–70% of the width) and the bottom 35%
completely empty. No solid shapes, no objects, no background color.
```

### 4.2 Neon Garden — 두 번째 테마: 발광 식물 옥상 정원

**Background** (1024×1536, 불투명)

```text
[공통 스타일 블록]
Vertical portrait composition, 2:3. A rooftop greenhouse garden at night in
a cyberpunk city. Bioluminescent plants with cyan and magenta glowing leaves,
thin hanging vines with light beads, a glass dome frame overhead with a few
broken panes showing the night sky. In the upper middle (15–65% of the height)
a raised circular planter platform, empty on top, lit by a soft cyan ring.
Distant towers behind the glass with sparse windows and violet haze. Small
red, blue and green flower lamps along the path as color accents. The lower
35% is a dark, calm stone path with faint moss glow, nearly empty.
Keep all important elements inside the central 84% of the width.
```

**Midground – `NeonGarden_Reactor.png`** (1024×1024, 투명)

```text
[공통 스타일 블록]
Isolated game prop on a fully transparent background, centered, front
three-quarter view. A "garden reactor": a glowing bioluminescent tree-like
crystal sprout growing out of a dark alloy planter ring with cyan edge lights.
Its three main branches end in red, blue and green glowing seed pods; thin
magenta light veins run through the trunk. Stylized, readable silhouette.
The prop fills about 90% of the height and 85% of the width, planter fully
visible. No floor, no shadow plane, no background.
```

**Ambient – `NeonGarden_Ambient.png`** (1024×1536, 투명)

```text
[공통 스타일 블록]
Transparent PNG light overlay, 2:3 vertical. Only soft light: floating
glowing pollen and firefly-like particles in cyan and soft magenta drifting
up the left and right sides, a few translucent glowing leaf silhouettes at
the top corners. Leave the central vertical band (30–70% of the width) and
the bottom 35% completely empty. No solid objects, no background color.
```

### 4.3 Sky Festival — 세 번째 테마: 하늘 축제 옥상

**Background** (1024×1536, 불투명)

```text
[공통 스타일 블록]
Vertical portrait composition, 2:3. A festival on the highest rooftop of a
neon megacity at night, far above the clouds of light below. Strings of small
holographic lanterns in cyan and magenta cross the sky, light drones hover in
the distance, faint firework trails in cyan and gold high in the dark sky.
In the upper middle (15–65% of the height) a tall launch platform with an
empty top, edged with cyan lights, two banner poles with red and green light
strips and a blue light beam rising behind. The lower 35% is a dark, calm
rooftop deck fading into shadow with faint guide lines, nearly empty.
Keep all important elements inside the central 84% of the width.
```

**Midground – `SkyFestival_Beacon.png`** (1024×1024, 투명)

```text
[공통 스타일 블록]
Isolated game prop on a fully transparent background, centered, front
three-quarter view. A "festival beacon": a slim dark alloy tower on a
circular base, its top holding a rotating ring of three light panels in red,
blue and green around a bright cyan-white core that shoots a short light
beam upward. Small magenta signal lights on the base. The prop fills about
90% of the height and 85% of the width, base fully visible. No floor, no
shadow plane, no background.
```

**Ambient – `SkyFestival_Ambient.png`** (1024×1536, 투명)

```text
[공통 스타일 블록]
Transparent PNG light overlay, 2:3 vertical. Only soft light: small glowing
confetti sparks and tiny lantern lights in cyan, magenta and a little gold
drifting down the left and right edges, a faint firework glow in the top
corners. Leave the central vertical band (30–70% of the width) and the bottom
35% completely empty. No solid objects, no background color.
```

## 5. 결과가 마음에 들지 않을 때 덧붙일 문장

- 너무 밝거나 낮처럼 보일 때: `Make it darker overall, night only, deep shadows, fewer light sources.`
- 글자나 로고가 생겼을 때: `Remove all text, letters, signs and logos completely.`
- 아래쪽이 복잡할 때: `Make the bottom third almost empty and dark, only a plain floor.`
- 투명 배경이 아닐 때: `Output as a PNG with a truly transparent background, no checkerboard, no backdrop.`
- 빛 주변에 회백색 얼룩이나 체크무늬 구멍이 남을 때(Iteration 65에서 실제로 생김): `Transparent PNG with true alpha. The glow must be colored light that fades smoothly into full transparency. No checkerboard pattern, no white or gray haze, no background fragments around the object. Keep the glow tight around the object.`
- 세 테마 톤이 다를 때: 첫 테마 Background를 대화에 다시 올리고 `Match the lighting, palette and rendering style of this image.`

## 6. 받은 뒤 Claude가 하는 일

1. 받은 파일을 같은 이름으로 교체한다(Background는 가운데를 9:16으로 잘라 941×1672로 맞춤).
2. 투명 레이어의 알파와 UI 가림 영역을 확인한다.
3. Frontend Builder를 두 번 실행하고 그래픽 캡처로 1080×1920, 720×1280 화면을 확인한다.
4. 최종 시각 판단(분위기, 통일감, 가독성)은 사용자가 한다.
