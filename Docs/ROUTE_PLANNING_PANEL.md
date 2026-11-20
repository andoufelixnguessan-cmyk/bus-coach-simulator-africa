# Écran de planification des trajets

## Ce qui vient d'être ajouté

`Assets/Scripts/UI/RoutePlanningPanel.cs` construit un panneau de planification mobile avec les composants UGUI classiques de Unity :

- choix d'une ville de départ parmi les villes du réseau ;
- choix de la prochaine ville uniquement parmi les liaisons routières disponibles ;
- ajout progressif de plusieurs étapes et possibilité de recommencer ;
- affichage du parcours, de la distance, de la durée estimée et des frais frontaliers annoncés ;
- sélection d'un car appartenant à l'entreprise, avec contrôle d'un minimum de six roues ;
- choix du service économique, confort ou VIP ;
- préparation d'un objet `PlayerRouteTripSetup` via `PlayerRouteTripFactory`.

Les montants, distances et durées affichés restent des valeurs provisoires d'équilibrage du jeu. Ce panneau ne calcule pas un itinéraire GPS réel.

## Branchement nécessaire dans le menu de compagnie

Le panneau doit être placé sous un Canvas actif dans une scène Unity. Le gestionnaire de garage/compagnie doit ensuite lui transmettre l'entreprise, les identifiants des cars réellement possédés et le rappel qui reçoit le trajet préparé :

```csharp
RoutePlanningPanel panel = canvas.GetComponentInChildren<RoutePlanningPanel>(true);
panel.Configure(companyProfile, ownedCoachIds, OnTripPrepared);

private void OnTripPrepared(PlayerRouteTripSetup setup)
{
    // Conserver setup pour l'étape suivante : réservation, contrôle des documents,
    // checklist de sécurité et préparation des coûts avant le départ.
}
```

`ownedCoachIds` doit contenir uniquement les identifiants des véhicules du garage du joueur. Ne pas transmettre tout le catalogue des cars.

## Limites actuelles

- Le panneau est un composant prêt à être branché ; il n'est pas encore appelé par un menu de scène existant.
- La sélection prépare les données métier mais ne génère pas les routes 3D et ne lance pas la conduite.
- Le réseau ne propose que les liaisons explicitement enregistrées. Une ville intermédiaire doit être reliée à la ville précédente.
- Les tarifs, distances, temps et frais sont des estimations internes au jeu.
- Aucun test de compilation ou test d'interface sur appareil Android n'a encore été effectué.

## Contrôles de cohérence

Le panneau délègue la validation finale à `PlayerRouteTripFactory`. Le factory vérifie l'entreprise, la propriété du car, le minimum de six roues, la compatibilité de la classe de service et la continuité des liaisons routières. Les contrôles frontaliers et leurs frais sont conservés dans le setup pour les étapes ultérieures.
