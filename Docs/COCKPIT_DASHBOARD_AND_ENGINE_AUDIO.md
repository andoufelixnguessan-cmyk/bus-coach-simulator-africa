# Tableau de bord, voyants et audio moteur

## Direction produit
La conduite doit disposer d'un vrai combiné d'instruments lisible en paysage, inspiré par le niveau de finition et la hiérarchie visuelle de Bus Simulator: Ultimate, mais avec une interface originale. Chaque modèle de car doit avoir son identité de cockpit : cadrans analogiques ou écran numérique selon la gamme, typographie, disposition, couleurs d'éclairage et sons propres.

## Données spécifiques par véhicule
Chaque fiche véhicule doit définir au minimum :
- famille moteur et cylindrée/type à afficher ;
- régime de ralenti, régime maximal et zone rouge ;
- signature audio (échantillons originaux/licenciés), volume cabine et vibration grave ;
- type de combiné (analogique, hybride ou numérique) ;
- unités, seuils et voyants réellement présents sur le véhicule.

Le prototype contient des profils provisoires distincts pour le car interurbain, standard, confort, grand tourisme, premium et double étage. Les valeurs sont des paramètres de jeu et ne prétendent pas reproduire les spécifications d'une marque réelle.

## Améliorations moteur
Les niveaux d'amélioration (0 à 5) influencent la réponse moteur, le régime maximal et la signature audio. Les améliorations devront aussi être équilibrées avec consommation, température, entretien et usure. Une amélioration ne doit pas simplement augmenter la vitesse sans contrepartie.

## Voyants et instruments en conduite
Le combiné prévoit vitesse, compte-tours, carburant, température du liquide de refroidissement, pression d'huile, pression d'air et tension batterie. Les témoins comprennent clignotants gauche/droite, feux de route, frein de stationnement, ceinture, ABS et défaut moteur.

Règles obligatoires :
- un voyant s'allume uniquement lorsque son état réel l'exige ;
- les témoins de contrôle peuvent s'allumer brièvement au démarrage puis s'éteindre après l'autotest ;
- rouge = arrêt/danger, orange = anomalie à contrôler, vert = fonction active, bleu = feux de route ;
- les voyants de sécurité ne doivent jamais être vendus comme options ;
- les alertes sonores doivent rester distinctes du bruit moteur et être réglables.

## Audio
Le prototype utilise un moteur diesel synthétique temporaire pour valider la variation de régime et de charge sans fichier audio externe. Ce son n'est pas un enregistrement réaliste. La version de production devra intégrer des enregistrements originaux ou correctement licenciés : démarrage, ralenti, accélération, décélération, frein moteur, changement de rapport, turbo, échappement et bruit cabine. Chaque autocar doit avoir ses propres couches audio et les améliorations moteur doivent les modifier de manière crédible.

## État de validation
Les scripts constituent une première base technique. Le combiné n'est pas encore un tableau de bord 3D de cockpit, les valeurs de diagnostic sont en partie simulées, les clignotants/phares ne sont pas encore reliés à des commandes de conduite, et le son synthétique ne remplace pas des enregistrements moteur. Compilation Unity, contrôle visuel, équilibrage et essais Android restent obligatoires.


## Prototype cockpit integration (implemented in source)

The current driving test scene now generates a first-person cockpit with a steering wheel, dashboard shell, center console, windshield glass, analog gauge faces with animated needles, and a digital cluster variant for coach profiles that request it. The steering wheel follows the steering input. Speed, RPM, fuel and coolant values animate the instrument display. Indicator lamps flash and the warning lamps are driven by the available vehicle state.

The first-person camera is the default driving view. A landscape HUD button switches between cockpit and exterior chase camera. Separate touch buttons toggle left/right indicators and headlights; two temporary spotlights respond to the headlight switch. The generated cockpit is a functional low-poly prototype, not final photorealistic art or a model copied from a commercial simulator.

## Remaining production work

- Add model-specific, high-quality cockpit meshes, materials, labels, steering column, mirrors, pedals and dashboard layouts.
- Connect all vehicle switches to full systems (wipers, hazard lights, low/high beams, doors, air conditioning, radio and other equipment).
- Replace simulated diagnostics and the synthesized diesel tone with validated mechanical data and original/licensed engine recordings.
- Add a full dashboard start-up self-test, realistic warning thresholds, air-brake pressure build-up and blink synchronization with exterior indicator lamps.
- Test visibility, safe-area placement, input response, frame time, and sound on the target Android device.

The source was checked for balanced braces and parentheses after integration. This is not a Unity compile or an Android device test.


## Mise à jour des commandes extérieures

Le HUD mobile a été réparé et ses commandes ont été repositionnées avec des pivots adaptés afin de rester dans la zone sûre de l'écran paysage. Les commandes disponibles sont maintenant : clignotant gauche, feux de détresse, clignotant droit, feux avant, pleins phares et changement de caméra.

Les clignotants commandent quatre sources lumineuses extérieures (avant/arrière, gauche/droite) avec une cadence de clignotement commune. Les feux de détresse activent les deux côtés et désactivent les clignotants directionnels. Les pleins phares modifient réellement l'intensité des deux projecteurs et le témoin bleu du tableau de bord suit leur état.

**Limite actuelle :** les projecteurs et indicateurs sont encore des lumières Unity de prototype, sans optiques, cabochons, réflecteurs ni matériaux lumineux de production. La vérification de compilation Unity et les essais tactiles sur Android restent à effectuer.


## Démarrage et arrêt moteur

Le moteur du prototype démarre désormais à la demande depuis le bouton « DÉMARRER / STOP ». Il ne peut être arrêté ou démarré que lorsque le car est presque immobile ; un démarrage est refusé si le réservoir est vide. Moteur arrêté, le couple moteur est nul, le régime retombe à zéro et la boucle diesel synthétique est coupée. Le régime et le bruit reprennent après démarrage.

Une vérification source a aussi corrigé deux références manquantes dans le cockpit : l'entrée de direction exposée par le contrôleur et le matériau clair utilisé pour les graduations. Cela ne vaut pas compilation Unity : le projet doit encore être ouvert et testé dans l'éditeur.
