# Passage de relais Unity Editor — BUS & COACH SIMULATOR AFRICA

Dernière mise à jour : 20 novembre 2026.

## Ce qui est préparé dans le dépôt

- Projet Unity ciblé sur Unity 6.3 LTS.
- Dépendances URP et UGUI déclarées dans `Packages/manifest.json`.
- Jeu en paysage, Android ARM64 et API minimale 26 configurés par l'outil Editor.
- Commande de préparation du projet : `BUS & COACH SIMULATOR AFRICA > 1 - Prepare Unity project`.
- Vérification préliminaire : `BUS & COACH SIMULATOR AFRICA > 3 - Verify project readiness`.
- Commande de compilation : `BUS & COACH SIMULATOR AFRICA > 2 - Build Android APK`.
- La scène générée est `Assets/Scenes/PrototypeDriving.unity`.
- La sortie APK prévue est `build/Android/BUS-COACH-SIMULATOR-AFRICA.apk`.

## Travail à faire dans Unity

1. Installer Unity 6.3 LTS avec Android Build Support, Android SDK & NDK Tools et OpenJDK.
2. Ouvrir le dossier racine du dépôt dans Unity Hub.
3. Attendre l'importation complète des packages et scripts.
4. Lancer la commande de vérification de readiness et corriger toute erreur rouge de Console.
5. Lancer la commande de préparation du projet si nécessaire.
6. Démarrer Play Mode et tester le déplacement, les collisions, la caméra, les commandes tactiles et l'interface.
7. Connecter un téléphone Android et compiler l'APK.
8. Installer l'APK et tester sur appareil réel, notamment la fluidité, les commandes, la sauvegarde après fermeture forcée et la reprise.
9. Corriger tous les problèmes révélés par la Console et les tests physiques avant diffusion.

## État réel — ne pas confondre code et validation

Le dépôt contient une sauvegarde locale périodique du profil d'entreprise et un checkpoint du car (position, rotation et carburant). À la reprise, le moteur est coupé et le frein de stationnement serré.

**La reprise complète d'une session de voyage n'est pas encore terminée.** Le checkpoint du car ne restaure pas encore le manifeste passagers, l'index du prochain arrêt, les contrôles frontaliers déjà validés, les dépenses du voyage ni l'horloge/météo de la session. Le joueur peut retrouver le car à sa position sauvegardée sans retrouver l'état métier complet du voyage.

La durée de session affichée dans le planificateur est une cible de conception, pas encore une accélération du temps ou une simulation de trajet calibrée.

## Conditions de sortie

Ne pas annoncer une version prête à publier tant que :
- le projet compile sans erreur dans Unity 6.3 LTS ;
- la scène de conduite fonctionne en Play Mode ;
- les commandes sont vérifiées sur Android ;
- la sauvegarde et la reprise après fermeture forcée sont validées ;
- les performances sont mesurées sur un appareil modeste ;
- les assets, sons, routes et licences nécessaires à la distribution sont contrôlés.

Les routes, distances, tarifs et frais de frontière du prototype sont des valeurs de jeu estimatives ; ils ne constituent pas des données de navigation ou des tarifs officiels.
