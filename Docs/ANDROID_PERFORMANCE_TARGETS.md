# Android Performance Targets — BUS & COACH SIMULATOR AFRICA

## Product requirement

The game must feel responsive and remain playable on the user's Android phone. Mobile performance is a release requirement, not a later optional polish task. The first release targets Android; PC is not allowed to compromise the mobile experience.

## Performance goals

These are project targets to measure in real builds, not claims that have already been achieved:

- Target 30 FPS minimum on the baseline supported Android device during normal driving; aim for a stable 45–60 FPS on stronger devices.
- Avoid sustained frame-rate drops, stutter during camera changes, and input lag.
- Touch steering, accelerator, brake, handbrake, indicators and camera controls must respond immediately and remain reachable above Android system navigation areas.
- Keep memory use, battery drain, heat and loading times under control during long journeys.
- Test extended sessions, not only a short launch screen or menu.

## Scalable graphics

Provide clear in-game graphics presets:
- **Économie**: reduced shadows, reflections, effects, traffic density and rendering distance for modest phones.
- **Équilibré**: recommended default for supported devices.
- **Élevé**: richer lighting, shadows and details for stronger phones.

The player must be able to lower graphics without losing essential driving information or gameplay functionality. Resolution scaling and quality changes should be available where technically supported.

## Required optimization practices

- Use Unity URP with mobile-oriented settings.
- Use LODs for coaches, roadside objects and scenery.
- Use occlusion culling and sensible camera/render distances.
- Use texture compression and suitable texture sizes.
- Bake static lighting where practical; limit real-time lights and expensive shadows.
- Pool repeated objects such as traffic vehicles, passengers and roadside effects.
- Avoid unnecessary per-frame allocations, expensive physics calculations and excessive active objects.
- Keep collision meshes simpler than visual meshes.
- Stream or load route sections in manageable chunks where needed.
- Profile CPU, GPU, memory, thermal behaviour and battery use on actual Android hardware.

## First playable milestone

Before adding a large map or many vehicle models, build and test one representative route with:
1. One complete coach with cockpit and exterior model.
2. Working steering, acceleration, braking and camera controls.
3. A short road section with basic scenery.
4. Android touch controls positioned safely above system navigation.
5. A graphics-quality selector.
6. A development build installed and tested on a real Android phone.

Only after this milestone performs acceptably should traffic, passengers, larger maps and more detailed effects be expanded.

## Release acceptance checklist

- [ ] Installs and launches on the declared minimum Android version/device.
- [ ] Touch controls are comfortable and never hidden behind system navigation.
- [ ] Stable target frame rate is measured during driving, not assumed.
- [ ] No repeated freezes, crashes or severe overheating in an extended test.
- [ ] Low graphics preset remains playable on the baseline phone.
- [ ] Loading and memory use are measured on device.
- [ ] Battery use is checked during a representative long journey.
- [ ] Performance is rechecked after adding traffic, passengers, weather and new coaches.

## Honesty about status

This document defines targets and tests. It does not mean the game is already optimized or playable. Performance can only be confirmed after a Unity Android build has been profiled and tested on the target phone. The baseline phone model and Android version must be recorded during the first device test.
