shader_type canvas_item;
varying vec4 label_tint;

void vertex() {
	label_tint = COLOR;
}

void fragment() {
	float ink = texture(TEXTURE, UV).a;
	COLOR = vec4(1.0, 0.96, 0.80, ink) * label_tint;
}
