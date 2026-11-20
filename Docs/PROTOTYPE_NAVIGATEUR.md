# Prototype navigateur — BUS & COACH SIMULATOR AFRICA

Une démo web légère pour tester les mécaniques principales sans installer Unity.

## Accès

Après activation/configuration de GitHub Pages sur la branche `main` à la racine, la page sera accessible à :

`https://andoufelixnguessan-cmyk.github.io/bus-coach-simulator-africa/prototype.html`

Si GitHub Pages n'est pas encore activé, ouvrir `prototype.html` dans le dépôt puis utiliser l'aperçu HTML disponible dans un environnement de prévisualisation, ou télécharger le fichier et l'ouvrir dans un navigateur récent.

## Fonctions incluses

- Sélection d'un autocar interurbain (standard, confort, double étage, premium).
- Sélection des villes et estimation de distance, recettes, coûts et bénéfices.
- Commandes tactiles de conduite : accélération, freinage, direction, klaxon et phares.
- Indicateurs de vitesse, carburant, distance, trésorerie et progression.
- Gestion simplifiée des passagers, arrêts et contrôle frontalier.
- Sauvegarde automatique locale et reprise du trajet après fermeture/réouverture du navigateur.
- Garantie de bénéfice minimum de 500 000 FCFA pour les trajets internationaux de plus de 700 km dans cette démo.

## Limites importantes

Cette démo est une simulation 2D de validation des règles et de l'interface. Elle n'est pas le jeu 3D Unity, ne contient pas encore les modèles 3D réalistes, la circulation avancée, les sons spécifiques aux véhicules ni la physique réelle. Les distances, tarifs et calculs sont des valeurs de prototype à équilibrer. Les sauvegardes restent dans le stockage local du navigateur et ne sont pas synchronisées avec Unity.

## Test conseillé

1. Choisir un car et un trajet.
2. Démarrer le voyage, accélérer puis freiner et tourner.
3. Fermer complètement l'onglet ou le navigateur pendant un trajet.
4. Rouvrir la page : le trajet doit reprendre à partir de la dernière sauvegarde locale.
5. Tester un trajet international et vérifier le contrôle frontalier ainsi que le résultat financier.
