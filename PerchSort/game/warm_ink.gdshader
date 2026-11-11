shader_type canvas_item;
// Brown lettering and digits: the painted near-black fill becomes the warm chocolate brown of the island designs,
// a little lighter at the top. The light outline and anti-aliased edges keep their own colour.
uniform vec4 top_color : source_color = vec4(0.490, 0.271, 0.090, 1.0);
uniform vec4 bottom_color : source_color = vec4(0.361, 0.169, 0.039, 1.0);

void fragment() {
	vec4 c = COLOR;
	float lum = dot(c.rgb, vec3(0.3, 0.59, 0.11));
	float ink = 1.0 - smoothstep(0.18, 0.42, lum);
	vec3 warm = mix(top_color.rgb, bottom_color.rgb, clamp(UV.y, 0.0, 1.0));
	COLOR = vec4(mix(c.rgb, warm, ink), c.a);
}
