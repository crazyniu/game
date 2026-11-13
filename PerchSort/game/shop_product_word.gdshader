shader_type canvas_item;

varying vec4 word_tint;

void vertex() {
	word_tint = COLOR;
}

// Keep the original painted outline and face relief while making the lettering ivory.
void fragment() {
	vec4 paint = texture(TEXTURE, UV);
	float face = smoothstep(0.30, 0.82, max(paint.r, max(paint.g, paint.b)));
	vec3 outline = vec3(0.19, 0.14, 0.045);
	vec3 ivory = mix(vec3(0.93, 0.83, 0.56), vec3(1.0, 0.98, 0.86), paint.g);
	COLOR = vec4(mix(outline, ivory, face), paint.a) * word_tint;
}
