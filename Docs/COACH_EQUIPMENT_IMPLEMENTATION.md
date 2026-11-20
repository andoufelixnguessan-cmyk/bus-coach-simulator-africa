# Coach equipment compatibility and garage purchases

This milestone adds data and purchase rules for optional onboard comfort equipment. It uses the same company FCFA balance as coach purchases and trip expenses.

## Equipment catalogue

| Equipment | Provisional game price | Compatibility rule |
|---|---:|---|
| Climatisation | 1,200,000 FCFA | All passenger coaches from 16 seats |
| Radio et système audio | 180,000 FCFA | All passenger coaches from 16 seats |
| Écrans TV passagers | 650,000 FCFA | Coaches from 23 seats, Comfort/VIP-capable |
| Prises USB passagers | 250,000 FCFA | All passenger coaches from 16 seats |
| Wi-Fi à bord | 300,000 FCFA | All passenger coaches from 16 seats |
| Sièges inclinables | 900,000 FCFA | Coaches from 23 seats, Comfort/VIP-capable |
| Repose-pieds | 280,000 FCFA | Coaches from 31 seats, Comfort/VIP-capable |
| Rideaux individuels | 160,000 FCFA | Coaches from 23 seats, Comfort/VIP-capable |
| Microphone et annonces | 140,000 FCFA | All passenger coaches from 16 seats |
| Réfrigérateur de service | 420,000 FCFA | Coaches from 23 seats |
| Équipement de restauration | 350,000 FCFA | Coaches from 23 seats |
| Toilettes à bord | 2,800,000 FCFA | Coaches from 40 seats, Comfort/VIP-capable |

These are provisional balancing prices for the game, not supplier quotes. Compatibility rules are initial gameplay rules and must be refined against each original 3D coach's real interior layout, available power and available space.

## Safety is not a paid upgrade
- Seatbelts for every passenger seat, emergency exits, fire extinguishers and first-aid equipment are standard safety equipment.
- The player must never be asked to buy these essentials to make a coach road-legal or safe.
- Equipment installation is optional, charged in FCFA and only offered when the selected coach supports it.

## Code
`Assets/Scripts/Garage/CoachEquipmentSystem.cs` checks ownership, seat capacity, service-class compatibility, duplicate installation and available FCFA before an installation is recorded.

## Current limits
- This is a data/rules foundation only. It is not connected to a garage screen or visible 3D equipment yet.
- No functional AC temperature control, radio playback, TV playback, food inventory, passenger use, electrical load or equipment wear is implemented yet.
- The catalogue has not been compiled or tested in Unity.
