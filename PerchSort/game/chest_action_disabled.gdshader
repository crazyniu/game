shader_type canvas_item;
varying vec4 button_tint;

void vertex() {
	button_tint = COLOR;
}

void fragment() {
	vec4 paint = texture(TEXTURE, UV);
	float value = dot(paint.rgb, vec3(0.299, 0.587, 0.114));
	COLOR = vec4(vec3(value), paint.a) * button_tint;
}
