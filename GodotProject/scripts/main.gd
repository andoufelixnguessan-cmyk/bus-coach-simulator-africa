extends Node3D
## Première scène jouable de BUS & COACH SIMULATOR AFRICA.
## Modèles provisoires en primitives : ils seront remplacés par les assets Blender.

const ROAD_LENGTH := 500.0
const MAX_SPEED := 34.0 # environ 122 km/h
const ACCELERATION := 12.0
const BRAKING := 22.0

var speed := 0.0
var distance_km := 0.0
var fuel_percent := 100.0
var steering := 0.0
var throttle_touch := false
var brake_touch := false
var left_touch := false
var right_touch := false
var coach: Node3D
var camera_3d: Camera3D
var speed_label: Label
var distance_label: Label
var fuel_label: Label
var status_label: Label

func _ready() -> void:
	_build_world()
	_build_coach()
	_build_camera()
	_build_hud()
	_update_hud()

func _physics_process(delta: float) -> void:
	var accelerating := Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP) or throttle_touch
	var braking := Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN) or brake_touch
	var steer_left := Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT) or left_touch
	var steer_right := Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT) or right_touch

	if accelerating:
		speed = move_toward(speed, MAX_SPEED, ACCELERATION * delta)
	else:
		speed = move_toward(speed, 0.0, 2.0 * delta)
	if braking:
		speed = move_toward(speed, 0.0, BRAKING * delta)

	steering = float(steer_right) - float(steer_left)
	var steering_factor: float = clampf(speed / MAX_SPEED, 0.15, 1.0)
	coach.position.x = clamp(coach.position.x + steering * 8.0 * steering_factor * delta, -4.0, 4.0)
	coach.position.z -= speed * delta
	distance_km += speed * delta / 1000.0
	fuel_percent = maxf(0.0, fuel_percent - speed * delta * 0.00012)

	if coach.position.x < -3.8 or coach.position.x > 3.8:
		status_label.text = "Attention : reste sur la chaussée !"
	else:
		status_label.text = "Bonne route !"
	camera_3d.position = coach.position + Vector3(0.0, 5.5, 11.5)
	camera_3d.look_at(coach.position + Vector3(0.0, 1.0, -8.0), Vector3.UP)
	_update_hud()

func _build_world() -> void:
	var world_env := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_SKY
	environment.sky = Sky.new()
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.72, 0.78, 0.86)
	environment.ambient_light_energy = 0.8
	world_env.environment = environment
	add_child(world_env)

	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48.0, -25.0, 0.0)
	sun.light_energy = 1.2
	add_child(sun)

	_add_box("Terrain", Vector3(0, -0.35, -220), Vector3(180, 0.5, 520), Color(0.20, 0.40, 0.20))
	_add_box("Route", Vector3(0, -0.04, -220), Vector3(12, 0.12, 520), Color(0.12, 0.13, 0.15))
	_add_box("Ligne centrale", Vector3(0, 0.035, -220), Vector3(0.16, 0.025, 520), Color(0.95, 0.88, 0.55))
	for side in [-1.0, 1.0]:
		_add_box("Accotement", Vector3(side * 6.4, 0.0, -220), Vector3(0.8, 0.08, 520), Color(0.55, 0.48, 0.34))
		for i in range(22):
			var tree_z := -i * 22.0 - 12.0
			_add_tree(Vector3(side * (12.0 + float(i % 3) * 2.0), 0.0, tree_z))

func _build_coach() -> void:
	coach = Node3D.new()
	coach.position = Vector3(0, 0.1, 0)
	add_child(coach)

	_add_coach_box("Carrosserie", Vector3(0, 1.65, 0), Vector3(2.65, 3.0, 10.5), Color(0.82, 0.12, 0.08), coach)
	_add_coach_box("Toit", Vector3(0, 3.18, 0), Vector3(2.7, 0.16, 10.55), Color(0.92, 0.92, 0.90), coach)
	_add_coach_box("Pare-brise", Vector3(0, 2.55, -5.28), Vector3(2.35, 1.15, 0.08), Color(0.10, 0.23, 0.30), coach)
	for side in [-1.0, 1.0]:
		_add_coach_box("Bande vitrée", Vector3(side * 1.34, 2.45, 0.0), Vector3(0.06, 0.95, 8.5), Color(0.12, 0.28, 0.36), coach)
		for z in [-3.4, -1.7, 0.0, 1.7, 3.4]:
			_add_wheel(Vector3(side * 1.38, 0.62, z), coach)
	_add_coach_box("Pare-chocs", Vector3(0, 0.62, -5.34), Vector3(2.5, 0.35, 0.25), Color(0.12, 0.12, 0.13), coach)
	_add_coach_box("Pare-chocs arrière", Vector3(0, 0.62, 5.34), Vector3(2.5, 0.35, 0.25), Color(0.12, 0.12, 0.13), coach)

func _build_camera() -> void:
	camera_3d = Camera3D.new()
	camera_3d.position = Vector3(0, 5.5, 11.5)
	camera_3d.fov = 72.0
	add_child(camera_3d)
	camera_3d.look_at(Vector3(0, 1, -8), Vector3.UP)
	camera_3d.current = true

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(18, 18)
	panel.add_theme_stylebox_override("panel", _panel_style())
	layer.add_child(panel)
	var column := VBoxContainer.new()
	column.add_theme_constant_override("separation", 5)
	panel.add_child(column)
	speed_label = _new_label("VITESSE : 0 km/h", 24)
	distance_label = _new_label("DISTANCE : 0.0 km", 18)
	fuel_label = _new_label("CARBURANT : 100 %", 18)
	column.add_child(speed_label)
	column.add_child(distance_label)
	column.add_child(fuel_label)

	status_label = _new_label("Bonne route !", 20)
	status_label.anchor_left = 0.5
	status_label.anchor_right = 0.5
	status_label.offset_left = -180
	status_label.offset_right = 180
	status_label.offset_top = 18
	status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	layer.add_child(status_label)

	var controls := HBoxContainer.new()
	controls.anchor_left = 0.0
	controls.anchor_top = 1.0
	controls.anchor_right = 1.0
	controls.anchor_bottom = 1.0
	controls.offset_left = 18
	controls.offset_top = -94
	controls.offset_right = -18
	controls.offset_bottom = -18
	controls.alignment = BoxContainer.ALIGNMENT_CENTER
	controls.add_theme_constant_override("separation", 12)
	layer.add_child(controls)
	_add_hold_button(controls, "◀ GAUCHE", "left")
	_add_hold_button(controls, "FREIN", "brake")
	_add_hold_button(controls, "ACCÉLÉRER", "throttle")
	_add_hold_button(controls, "DROITE ▶", "right")

func _add_hold_button(parent: HBoxContainer, caption: String, action: String) -> void:
	var button := Button.new()
	button.text = caption
	button.custom_minimum_size = Vector2(150, 64)
	button.add_theme_font_size_override("font_size", 18)
	button.button_down.connect(func(): _set_control(action, true))
	button.button_up.connect(func(): _set_control(action, false))
	parent.add_child(button)

func _set_control(action: String, pressed: bool) -> void:
	match action:
		"left": left_touch = pressed
		"right": right_touch = pressed
		"brake": brake_touch = pressed
		"throttle": throttle_touch = pressed

func _update_hud() -> void:
	if speed_label:
		speed_label.text = "VITESSE : %d km/h" % roundi(speed * 3.6)
		distance_label.text = "DISTANCE : %.1f km" % distance_km
		fuel_label.text = "CARBURANT : %d %%" % roundi(fuel_percent)

func _new_label(caption: String, font_size: int) -> Label:
	var label := Label.new()
	label.text = caption
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", Color.WHITE)
	return label

func _panel_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.025, 0.045, 0.075, 0.88)
	style.set_content_margin_all(14)
	style.set_corner_radius_all(12)
	return style

func _add_box(label: String, pos: Vector3, dimensions: Vector3, color: Color, parent: Node = null) -> void:
	var mesh_node := MeshInstance3D.new()
	mesh_node.name = label
	var box := BoxMesh.new()
	box.size = dimensions
	mesh_node.mesh = box
	mesh_node.position = pos
	mesh_node.material_override = _material(color)
	(parent if parent else self).add_child(mesh_node)

func _add_coach_box(label: String, pos: Vector3, dimensions: Vector3, color: Color, parent: Node3D) -> void:
	_add_box(label, pos, dimensions, color, parent)

func _add_wheel(pos: Vector3, parent: Node3D) -> void:
	var wheel := MeshInstance3D.new()
	var cylinder := CylinderMesh.new()
	cylinder.top_radius = 0.48
	cylinder.bottom_radius = 0.48
	cylinder.height = 0.32
	wheel.mesh = cylinder
	wheel.position = pos
	wheel.rotation_degrees.z = 90
	wheel.material_override = _material(Color(0.035, 0.035, 0.04))
	parent.add_child(wheel)

func _add_tree(pos: Vector3) -> void:
	var trunk := MeshInstance3D.new()
	var trunk_mesh := CylinderMesh.new()
	trunk_mesh.top_radius = 0.18
	trunk_mesh.bottom_radius = 0.25
	trunk_mesh.height = 2.0
	trunk.mesh = trunk_mesh
	trunk.position = pos + Vector3(0, 1.0, 0)
	trunk.material_override = _material(Color(0.30, 0.18, 0.09))
	add_child(trunk)
	var crown := MeshInstance3D.new()
	var crown_mesh := SphereMesh.new()
	crown_mesh.radius = 1.35
	crown_mesh.height = 2.7
	crown.mesh = crown_mesh
	crown.position = pos + Vector3(0, 2.8, 0)
	crown.material_override = _material(Color(0.12, 0.34, 0.12))
	add_child(crown)

func _material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.82
	return material
