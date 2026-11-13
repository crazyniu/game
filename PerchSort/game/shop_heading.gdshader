shader_type canvas_item;
uniform bool dark_letters = false;
varying vec4 word_tint;

void vertex() {
	word_tint = COLOR;
}
// Recolour the existing painted white category words into one gold-and-orange title family.
void fragment() {
	vec3 paint = texture(TEXTURE, UV).rgb;
	if (dark_letters) {
		float ink = texture(TEXTURE, UV).a;
		vec2 edge = TEXTURE_PIXEL_SIZE * 2.0;
		float outline = max(ink, max(texture(TEXTURE, UV + vec2(edge.x, 0.0)).a, texture(TEXTURE, UV - vec2(edge.x, 0.0)).a));
		outline = max(outline, max(texture(TEXTURE, UV + vec2(0.0, edge.y)).a, texture(TEXTURE, UV - vec2(0.0, edge.y)).a));
		vec3 gold = mix(vec3(1.0, 0.87, 0.22), vec3(1.0, 0.54, 0.02), UV.y);
		COLOR = vec4(mix(vec3(0.31, 0.11, 0.015), gold, ink), outline * word_tint.a);
	} else {
	float light = dot(paint, vec3(0.3, 0.59, 0.11));
	float face = smoothstep(0.24, 0.82, light);
	vec3 gold = mix(vec3(1.0, 0.50, 0.015), vec3(1.0, 0.88, 0.24), light);
	COLOR = vec4(mix(vec3(0.65, 0.22, 0.015), gold, face), COLOR.a);
	}
}
