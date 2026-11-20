# Company economy and route calculation foundation

This milestone adds the first reusable C# data layer for the game's company economy. It is not yet connected to menus, saved games, passenger simulation, or the live driving scene.

## One currency
- All balances, vehicle prices, fares, expenses and upgrades use FCFA (XOF).
- No coins, tokens, premium currency or mandatory real-money purchases.
- Every expense is checked against the available balance before it is recorded.
- Income and expenses create a finance-history entry with the resulting balance.

## Starter company
- Starting capital: 10,000,000 FCFA.
- The new-company setup purchases and grants the 16–22-seat minicar interurbain for 9,000,000 FCFA.
- The company begins with that usable coach and 1,000,000 FCFA remaining.
- This is intended to avoid forcing the player to grind before their first trip.

## Passenger coach catalogue

| Coach type | Passenger seats | Price | Company level |
|---|---:|---:|---:|
| Minicar interurbain | 16–22 | 9,000,000 FCFA | 1 |
| Minibus / petit car | 23–30 | 14,000,000 FCFA | 1 |
| Car moyen | 31–39 | 22,000,000 FCFA | 2 |
| Autocar standard | 40–49 | 32,000,000 FCFA | 3 |
| Autocar confort | 45–53 | 42,000,000 FCFA | 4 |
| Grand tourisme | 49–57 | 58,000,000 FCFA | 5 |
| Autocar premium longue distance | 51–59 | 75,000,000 FCFA | 7 |
| Autocar double étage tourisme | 70–83 | 95,000,000 FCFA | 9 |

These are game-balance values, not real-world market quotations. Seat count is passenger capacity and excludes driver/attendant. Exact capacity must be set by the actual coach interior asset.

## Route fare data

One-way provisional game fares per passenger (Economy / Comfort / VIP):

| Route | Economy | Comfort | VIP |
|---|---:|---:|---:|
| Abidjan–Yamoussoukro | 5,000 | 7,000 | 10,000 |
| Abidjan–Bouaké | 8,000 | 12,000 | 16,000 |
| Abidjan–Korhogo | 12,000 | 16,000 | 22,000 |
| Abidjan–San Pedro | 10,000 | 14,000 | 18,000 |
| Abidjan–Man | 12,000 | 17,000 | 23,000 |
| Abidjan–Accra | 15,000 | 20,000 | 28,000 |
| Abidjan–Lomé | 22,000 | 30,000 | 40,000 |
| Abidjan–Cotonou | 28,000 | 38,000 | 50,000 |
| Abidjan–Ouagadougou | 30,000 | 42,000 | 55,000 |
| Abidjan–Bamako | 35,000 | 48,000 | 65,000 |
| Abidjan–Conakry | 45,000 | 60,000 | 80,000 |
| Accra–Lagos | 30,000 | 42,000 | 55,000 |

VIP and Comfort fares can only be quoted when the selected coach definition supports that service class. Actual market prices are not represented.

## Trip quote behavior
- Uses the midpoint of the coach's passenger-seat range for a preliminary estimate.
- Applies a configurable expected occupancy percentage; the player does not need a full coach.
- Subtracts explicit fuel, staff, toll/border, food/drink and maintenance-reserve costs.
- International border fees are included in the pre-departure cost inputs and must be shown before confirmation.
- The quote is an estimate only. Actual ticket sales and costs must be recorded during a completed trip.

## Code
`Assets/Scripts/Economy/CoachEconomy.cs` contains the coach catalogue, starter company creation, wallet/finance history, route fares and trip quote calculation.

## Not yet implemented
- No garage screen, route-selection screen, passenger booking, ticket sales, save/load integration, real route-distance calculator, fuel-consumption model, or Unity UI is connected yet.
- This code has not been compiled or tested in Unity. Do not describe these systems as playable until the Unity build and device tests pass.
