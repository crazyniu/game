shader_type canvas_item;
// Existing dark painted text sources use ocean teal throughout the UI; pale painted edges stay intact.
uniform vec4 top_color : source_color = vec4(0.08, 0.46, 0.52, 1.0);
uniform vec4 bottom_color : source_color = vec4(0.04, 0.32, 0.40, 1.0);

void fragment() {
	vec3 paint = texture(TEXTURE, UV).rgb;
	float brightness = max(paint.r, max(paint.g, paint.b));
	float ink = 1.0 - smoothstep(0.65, 0.90, brightness);
	vec3 teal = mix(top_color.rgb, bottom_color.rgb, clamp(UV.y, 0.0, 1.0));
	COLOR = vec4(mix(paint, teal, ink), COLOR.a);
}
