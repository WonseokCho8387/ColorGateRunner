# Art Direction

## Visual goal

Clean, readable, toy-like mobile visuals using simple geometric shapes.

## Camera

- Portrait 9:16
- Slightly elevated third-person view
- Player and next two gates must remain visible
- No camera rotation during gameplay

## Palette

- Player and gates use exactly two gameplay colors:
  - Red
  - Blue
- Background uses neutral low-saturation colors
- Gameplay colors must not appear on non-gameplay obstacles

## Feedback

### Correct gate

- Short scale punch
- Small particle burst
- Score text updates immediately
- Feedback duration below 0.35 seconds

### Wrong gate

- Player movement stops immediately
- Brief screen shake
- Player becomes desaturated
- Game-over UI appears within 0.5 seconds

## Constraints

- No downloaded art assets for the MVP
- Use primitive meshes and generated materials
- Avoid post-processing for the first milestone
- Maintain readability on a small phone screen