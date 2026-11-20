# Android Performance Targets — BUS & COACH SIMULATOR AFRICA

## Product requirement
The game must feel responsive and remain playable on the user's Android phone. Mobile performance is a release requirement, not a later optional polish task. Android is the priority platform; PC must not compromise the mobile experience.

## Reference phone
- User's reference device: **Samsung Galaxy A07**.
- Exact variant, RAM capacity, Android version and free storage are not yet confirmed. Do not assume these details.
- Until the exact configuration is recorded, develop and profile conservatively using a low-memory Android profile comparable to an entry-level configuration.
- The Galaxy A07 must be included in real-device compatibility testing before claiming the game is optimized for it.

## Performance goals
These are targets to measure in real builds, not claims that have already been achieved:
- Aim for a stable **30 FPS** during normal driving on the reference Galaxy A07, using the Économie preset if necessary.
- Avoid sustained drops below the target, stutter during camera changes, and touch-input lag.
- Aim for 45–60 FPS on stronger supported devices; do not force high frame rates at the expense of heat, battery life or stability.
- Touch steering, accelerator, brake, handbrake, indicators and camera controls must respond immediately and remain reachable above Android system navigation areas.
- Keep memory use, battery drain, heat and loading times under control during long journeys.
- Test extended driving sessions, not only launch screens or menus.

## Scalable graphics
Provide three in-game presets:
- **Économie**: default recommendation for the reference phone until profiling proves another preset is suitable; reduced shadows, reflections, effects, traffic density, vegetation density and rendering distance.
- **Équilibré**: recommended only on devices where profiling confirms stable performance.
- **Élevé**: richer lighting, shadows and details for stronger devices.

Allow the player to lower graphics without losing essential driving information or gameplay functionality. Add resolution scaling and a frame-rate limit where technically supported. Avoid forcing expensive visual effects on the reference phone.

## Required optimization practices
- Use Unity URP with mobile-oriented settings and a conservative rendering budget.
- Use LODs for coaches, roadside objects and scenery.
- Use occlusion culling and sensible camera/render distances.
- Use texture compression and suitable texture sizes; avoid unnecessarily large textures.
- Bake static lighting where practical; limit real-time lights, transparent effects and expensive shadows.
- Pool repeated objects such as traffic vehicles, passengers and roadside effects.
- Avoid unnecessary per-frame allocations, expensive physics calculations and excessive active objects.
- Keep collision meshes simpler than visual meshes.
- Stream or load route sections in manageable chunks where needed.
- Profile CPU, GPU, memory, thermal behaviour and battery use on actual Android hardware.
- Provide sensible settings for reduced traffic and scenery density on the low preset while preserving the core simulation.

## First playable milestone
Before adding a large map or many vehicle models, build and test one representative route with:
1. One complete coach with cockpit and exterior model.
2. Working steering, acceleration, braking and camera controls.
3. A short road section with basic, optimized scenery.
4. Android touch controls positioned safely above system navigation.
5. A graphics-quality selector with Économie available.
6. A development build installed and tested on the user's Samsung Galaxy A07.

Only after this milestone performs acceptably on the reference phone should traffic, passengers, larger maps and more detailed effects be expanded.

## Release acceptance checklist
- [ ] Exact Galaxy A07 variant, RAM, Android version and available storage recorded.
- [ ] Installs and launches on the reference phone.
- [ ] Touch controls are comfortable and never hidden behind system navigation.
- [ ] Stable target frame rate is measured during driving, not assumed.
- [ ] No repeated freezes, crashes or severe overheating in an extended test.
- [ ] Économie preset remains playable on the reference phone.
- [ ] Loading and memory use are measured on device.
- [ ] Battery use is checked during a representative long journey.
- [ ] Performance is rechecked after adding traffic, passengers, weather and new coaches.

## Honesty about status
This document defines targets and tests. It does not mean the game is already optimized or playable. Performance can only be confirmed after a Unity Android build has been profiled and tested on the target phone. The Samsung Galaxy A07 is the reference device; its exact hardware configuration still needs to be recorded.
