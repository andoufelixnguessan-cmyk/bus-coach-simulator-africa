# Création du premier autocar 3D

## Générer le modèle dans Blender 4.x
1. Ouvrir Blender.
2. Aller dans l'espace de travail **Scripting**.
3. Cliquer sur **New** dans l'éditeur de texte.
4. Ouvrir le fichier `create_coach.py` avec **Open** ou copier-coller son contenu.
5. Cliquer sur **Run Script**.
6. Le premier autocar apparaît dans la vue 3D.
7. Enregistrer le projet avec **File > Save As** au format `.blend`.

## Modèle actuel
- Autocar interurbain de 12,5 m environ.
- 6 roues, carrosserie bleue, vitres fumées, rétroviseurs, éclairage, compartiments à bagages.
- Intérieur provisoire avec sièges passagers et poste conducteur.
- Le modèle est généré avec des objets modifiables, pas une image.

## Étape suivante
Affiner la géométrie et les détails, puis exporter en glTF 2.0 (.glb) pour l'importer dans Godot.
