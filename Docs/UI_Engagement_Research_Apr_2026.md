# RuneDrop UI engagement research (April 15, 2026)

## Goal
Identify UI patterns that improve retention and repeat sessions for mobile games, then map them to RuneDrop updates.

## What the external data says

1. **Early retention is fragile in games, so first-session clarity + immediate goals matter.**
   - Adjust's *Mobile App Trends 2025* reports global gaming retention roughly at **D1 27%, D7 13%, D14 8%, D30 5%** (2024), emphasizing how quickly players churn after install.
   - Source: https://a.storyblok.com/f/47007/x/9a13feb8eb/mobile-app-trends-2025.pdf

2. **Session quality (time spent per session) can be increased with deeper engagement loops.**
   - Same report shows gaming session lengths trending slightly up globally, while sessions/day are mostly flat—suggesting value from better in-session stickiness and meaningful mid-session choices.
   - Source: https://a.storyblok.com/f/47007/x/9a13feb8eb/mobile-app-trends-2025.pdf

3. **Motivation science indicates strong game motivation comes from autonomy + competence + clear feedback.**
   - Ryan, Rigby, Przybylski (2006): game motivation is stronger when players feel meaningful choice (autonomy), optimal challenge and mastery (competence), and intuitive controls/feedback.
   - Source: https://selfdeterminationtheory.org/SDT/documents/2006_RyanRigbyPrzybylski_MandE.pdf

## Practical UI implications for RuneDrop

1. **Always show a short-term objective**
   - e.g., today's depth focus / next milestone depth.
   - Why: gives instant purpose in first 10 seconds and creates a reason to replay.

2. **Make high-impact choices visually obvious**
   - decision room should clearly communicate safe vs risky and have large touch targets.
   - Why: high clarity lowers friction and increases interaction confidence.

3. **Use progression framing after failure**
   - death screen should frame “how close to next milestone” not only loss.
   - Why: loss + next-goal framing supports immediate re-entry into another run.

4. **Preserve consistency of visual language across all screens**
   - shared card styles, button hierarchy, spacing, and typography.
   - Why: polished consistency increases trust/perceived quality and reduces cognitive load.

## Applied in this PR

- Main menu now surfaces login streak and a daily depth focus.
- Death screen now shows progress to the next milestone depth.
- Prior pass already refreshed decision/death/shop layouts using shared premium components.
