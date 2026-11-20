import bpy
import math
from mathutils import Vector, Euler

# BUS & COACH SIMULATOR AFRICA — premier autocar procédural
# Blender 4.x — unités en mètres — avant du car vers -Y.

# Nettoyage de la scène
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials):
    for datablock in list(datablocks):
        if datablock.users == 0:
            datablocks.remove(datablock)

def material(name, color, metallic=0.0, roughness=0.42):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    # Trouver le shader par son type plutôt que par son nom
    # (le nom peut varier selon la langue/version de Blender).
    bsdf = next((node for node in mat.node_tree.nodes
                 if node.type == 'BSDF_PRINCIPLED'), None)
    if bsdf is not None:
        base_color = bsdf.inputs.get("Base Color")
        metal_input = bsdf.inputs.get("Metallic")
        rough_input = bsdf.inputs.get("Roughness")
        if base_color is not None:
            base_color.default_value = (*color, 1.0)
        if metal_input is not None:
            metal_input.default_value = metallic
        if rough_input is not None:
            rough_input.default_value = roughness
    return mat

PAINT = material("Carrosserie | Bleu profond métallisé", (0.025, 0.12, 0.30), 0.68, 0.27)
PAINT_LIGHT = material("Liseré | Bleu électrique", (0.04, 0.38, 0.78), 0.55, 0.25)
WHITE = material("Toit | Blanc perlé", (0.82, 0.86, 0.90), 0.28, 0.3)
GLASS = material("Vitrage | Verre fumé", (0.018, 0.055, 0.085), 0.35, 0.12)
RUBBER = material("Pneus | Caoutchouc", (0.018, 0.021, 0.025), 0.0, 0.88)
ALLOY = material("Jantes | Aluminium", (0.55, 0.61, 0.67), 0.82, 0.22)
DARK = material("Détails | Anthracite", (0.035, 0.045, 0.055), 0.25, 0.45)
LIGHT = material("Phares | LED", (0.82, 0.92, 1.0), 0.25, 0.16)
RED = material("Feux arrière", (0.65, 0.018, 0.025), 0.2, 0.2)
SEAT = material("Sièges | Tissu bleu", (0.035, 0.10, 0.22), 0.0, 0.8)
SEAT_TRIM = material("Sièges | Surpiqûres", (0.18, 0.42, 0.68), 0.0, 0.65)

def cube(name, location, dimensions, mat, bevel=0.04):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if mat:
        obj.data.materials.append(mat)
    if bevel:
        mod = obj.modifiers.new("Arrondis de carrosserie", "BEVEL")
        mod.width = bevel
        mod.segments = 3
        mod = obj.modifiers.new("Normales pondérées", "WEIGHTED_NORMAL")
    return obj

def cylinder(name, location, radius, depth, mat, vertices=48, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                       end_fill_type='NGON', location=location,
                                       rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    for p in obj.data.polygons:
        p.use_smooth = True
    return obj

def uv_sphere(name, location, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(mat)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    for p in obj.data.polygons:
        p.use_smooth = True
    return obj

# Châssis et volumes principaux
cube("Châssis porteur", (0, 0, 0.72), (2.35, 11.25, 0.42), DARK, 0.08)
cube("Soubassement", (0, 0, 1.12), (2.48, 11.75, 0.72), PAINT, 0.13)
cube("Carrosserie principale", (0, 0, 2.30), (2.50, 11.85, 2.55), PAINT, 0.19)
cube("Toit aérodynamique", (0, 0.02, 3.62), (2.54, 11.92, 0.20), WHITE, 0.10)
cube("Bande latérale bleue gauche", (-1.263, 0, 1.38), (0.035, 11.35, 0.12), PAINT_LIGHT, 0.025)
cube("Bande latérale bleue droite", (1.263, 0, 1.38), (0.035, 11.35, 0.12), PAINT_LIGHT, 0.025)

# Pare-brise avant en deux parties, face avant vers -Y
for x in (-0.57, 0.57):
    pane = cube("Pare-brise panoramique", (x, -5.94, 2.72), (1.08, 0.045, 1.02), GLASS, 0.07)
    pane.rotation_euler.x = math.radians(-8)
cube("Montant central pare-brise", (0, -5.965, 2.72), (0.055, 0.07, 1.04), DARK, 0.015)
cube("Bandeau sous pare-brise", (0, -5.91, 2.08), (2.34, 0.12, 0.16), DARK, 0.035)

# Vitres latérales et montants
window_y = [-4.75, -3.55, -2.35, -1.15, 0.05, 1.25, 2.45, 3.65, 4.75]
for side in (-1, 1):
    x = side * 1.258
    for idx, y in enumerate(window_y):
        cube("Vitre latérale %s %02d" % (side, idx+1),
             (x, y, 2.77), (0.035, 1.04, 0.91), GLASS, 0.035)
        cube("Joint inférieur vitre",
             (side * 1.278, y, 2.285), (0.025, 1.08, 0.035), ALLOY, 0.008)
    # Rétroviseurs et bras
    cube("Bras rétroviseur", (side*1.48, -5.05, 2.82), (0.42, 0.07, 0.07), DARK, 0.025)
    mirror = cube("Rétroviseur extérieur", (side*1.68, -5.12, 2.80), (0.22, 0.18, 0.42), DARK, 0.06)
    cube("Miroir réfléchissant", (side*1.798, -5.12, 2.80), (0.018, 0.13, 0.30), ALLOY, 0.025)

# Porte passagers latérale droite (panneau vitré)
cube("Porte passagers", (1.267, -4.72, 2.12), (0.045, 0.92, 1.85), DARK, 0.035)
cube("Vitre porte", (1.295, -4.72, 2.68), (0.025, 0.78, 0.72), GLASS, 0.025)
cube("Poignée porte", (1.32, -4.42, 1.75), (0.025, 0.12, 0.045), ALLOY, 0.012)

# Trois essieux : 2 roues avant + 4 roues arrière = 6 roues
wheel_positions = [-4.20, 3.20, 4.35]
for y in wheel_positions:
    for side in (-1, 1):
        x = side * 1.31
        cylinder("Pneu autocar", (x, y, 0.67), 0.52, 0.34, RUBBER,
                 rotation=(0, math.pi/2, 0))
        cylinder("Jante aluminium", (x + side*0.18, y, 0.67), 0.30, 0.025, ALLOY,
                 rotation=(0, math.pi/2, 0))
        cylinder("Moyeu", (x + side*0.20, y, 0.67), 0.105, 0.035, DARK,
                 vertices=32, rotation=(0, math.pi/2, 0))
        for a in range(8):
            angle = a * math.tau / 8
            uv_sphere("Boulon de roue",
                      (x + side*0.222, y + math.cos(angle)*0.215,
                       0.67 + math.sin(angle)*0.215),
                      (0.025, 0.025, 0.025), WHITE)

# Phares avant, clignotants et calandre
for x in (-0.86, 0.86):
    cube("Bloc optique LED", (x, -5.99, 1.68), (0.48, 0.08, 0.24), LIGHT, 0.07)
    cube("Clignotant", (x, -6.00, 1.48), (0.32, 0.07, 0.075), RED, 0.025)
cube("Calandre avant", (0, -6.00, 1.72), (0.82, 0.06, 0.22), DARK, 0.04)
cube("Pare-chocs avant", (0, -5.98, 1.20), (2.35, 0.20, 0.20), DARK, 0.07)
for x in (-0.86, 0.86):
    cube("Feu arrière", (x, 5.99, 1.62), (0.30, 0.07, 0.65), RED, 0.05)
cube("Pare-chocs arrière", (0, 5.98, 1.18), (2.35, 0.18, 0.20), DARK, 0.06)

# Compartiments à bagages
for side in (-1, 1):
    for y in (-2.9, -0.8, 1.3, 2.7):
        cube("Trappe soute à bagages", (side*1.268, y, 1.55), (0.025, 1.75, 0.42), PAINT, 0.025)
        cube("Poignée soute", (side*1.29, y, 1.55), (0.025, 0.12, 0.035), ALLOY, 0.01)

# Habitacle : allée centrale et sièges passagers (rangées 2+2)
for row in range(9):
    y = -2.8 + row * 0.82
    for x in (-0.72, 0.72):
        cube("Assise siège passager", (x, y, 1.55), (0.52, 0.52, 0.18), SEAT, 0.10)
        cube("Dossier siège passager", (x, y+0.20, 1.92), (0.52, 0.16, 0.68), SEAT, 0.11)
        cube("Appuie-tête", (x, y+0.19, 2.30), (0.36, 0.13, 0.20), SEAT_TRIM, 0.07)

# Poste conducteur à l'avant gauche
cube("Siège conducteur", (-0.72, -4.72, 1.62), (0.58, 0.62, 0.22), DARK, 0.10)
cube("Dossier conducteur", (-0.72, -4.48, 2.00), (0.58, 0.18, 0.72), SEAT, 0.10)
cube("Tableau de bord", (0, -5.35, 1.88), (2.18, 0.62, 0.32), DARK, 0.12)
cube("Écran instruments", (-0.70, -5.68, 2.12), (0.58, 0.035, 0.22), GLASS, 0.035)
# Volant orienté vers le pare-brise
bpy.ops.mesh.primitive_torus_add(major_radius=0.25, minor_radius=0.035,
                                 major_segments=48, minor_segments=12,
                                 location=(-0.72, -5.12, 2.12),
                                 rotation=(math.radians(90), 0, 0))
bpy.context.object.name = "Volant conducteur"
bpy.context.object.data.materials.append(DARK)

# Logo de marque provisoire sur l'avant
cube("Plaque logo avant", (0, -6.055, 1.92), (0.42, 0.025, 0.12), ALLOY, 0.025)

# Origine et sélection du car
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
# Vue 3D agréable à l'ouverture
for area in bpy.context.screen.areas:
    if area.type == 'VIEW_3D':
        area.spaces.active.region_3d.view_distance = 19
        area.spaces.active.region_3d.view_location = (0, 0, 1.8)
        area.spaces.active.region_3d.view_rotation = Euler((math.radians(68), 0, math.radians(-28)), 'XYZ').to_quaternion()

print("BUS & COACH SIMULATOR AFRICA : autocar 3D de base créé.")
print("Dimensions approximatives : 12,5 m x 2,55 m x 3,8 m — 6 roues.")
