shader_type canvas_item;
// Dark outline around white painted words and numbers (the candy-button lettering look). Needs transparent padding.
// `bend` bows the words along a ribbon: the ends sit lower than the middle by `bend` (as a share of the texture height).
uniform vec4 outline_color : source_color = vec4(0.13, 0.22, 0.17, 1.0);
uniform float width = 6.0;
uniform float bend = 0.0;

void fragment() {
	float u = UV.x * 2.0 - 1.0;
	vec2 at = vec2(UV.x, UV.y - bend * u * u);
	vec4 c = texture(TEXTURE, at);
	float a = 0.0;
	for (int i = 0; i < 16; i++) {
		float t = float(i) * 0.3926991;
		vec2 d = vec2(cos(t), sin(t)) * TEXTURE_PIXEL_SIZE * width;
		a = max(a, texture(TEXTURE, at + d).a);
		a = max(a, texture(TEXTURE, at + d * 0.55).a);
	}
	COLOR = vec4(mix(outline_color.rgb, c.rgb, c.a), max(c.a, a * outline_color.a));
}
