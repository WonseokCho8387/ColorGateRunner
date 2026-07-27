# Art Direction

## Visual goal

Clean, readable, toy-like mobile visuals using simple geometric shapes.

## Camera

- Portrait 9:16
- Slightly elevated third-person view
- Player and next two gates must remain visible
- No camera rotation during gameplay

## Palette

- Player and gates begin with two gameplay colors:
  - Red
  - Blue
- Green is introduced at the Step 7 mid-run milestone and is shown in the
  color-cycle indicator before its tutorial gates arrive.
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
- Player progressively drops, tilts, and shrinks
- Game-over UI appears after approximately 0.9 seconds

### Speed readability

- Four stages use restrained FOV changes from 60 to 74 degrees
- Player trail and peripheral speed lines strengthen with each stage
- Speed-stage and raw pacing labels are developer diagnostics and are hidden
  by default in the normal player presentation.

### Shield

- Active shield is continuously visible around the player
- A rotating, bobbing primitive pickup is visible before acquisition.
- Acquisition and break use distinct centered messages and reusable bursts
- Recovery uses a controlled blink without obscuring upcoming gates

### Results

- One central high-contrast card contains `GAME OVER`, the largest `THIS RUN`
  score, an accented Best badge, a dedicated `NEW BEST!` treatment, five
  aligned rank rows, and Retry.
- The current-run row uses a blue highlight and `YOU` marker.
- Only the current-run score repeats a subtle pulse; the whole card does not.

## Constraints

- No downloaded art assets for the MVP
- Use primitive meshes and generated materials
- Avoid post-processing for the first milestone
- Maintain readability on a small phone screen
