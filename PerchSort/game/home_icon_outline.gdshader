shader_type canvas_item;
render_mode blend_premul_alpha;

uniform vec2 draw_size;
uniform vec4 rim_color : source_color = vec4(1.0, 0.98, 0.86, 1.0);
uniform vec4 edge_color : source_color = vec4(0.28, 0.13, 0.035, 1.0);
uniform vec4 shadow_color : source_color = vec4(0.13, 0.07, 0.025, 0.40);

const float MARGIN = 12.0;
varying vec4 icon_modulate;

void vertex() {
	icon_modulate = COLOR;
	VERTEX += (UV * 2.0 - 1.0) * MARGIN;
}

vec4 paint_at(sampler2D source, vec2 uv) {
	vec4 paint = texture(source, uv);
	float inside = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
	// Faint cutout residue must not become isolated outlined specks.
	paint.a = smoothstep(0.06, 0.98, paint.a) * inside;
	return paint;
}

float silhouette(sampler2D source, vec2 uv, float radius) {
	float alpha = paint_at(source, uv).a;
	for (int i = 0; i < 8; i++) {
		float angle = float(i) * 0.7853981634;
		vec2 offset = vec2(cos(angle), sin(angle)) * radius / draw_size;
		alpha = max(alpha, paint_at(source, uv + offset).a);
	}
	return alpha;
}

vec4 over(vec4 below, vec4 above) {
	return vec4(above.rgb * above.a, above.a) + below * (1.0 - above.a);
}

void fragment() {
	vec2 uv = (UV - 0.5) * (draw_size + 2.0 * MARGIN) / draw_size + 0.5;
	vec2 shadow_uv = uv - vec2(0.0, 6.0) / draw_size;
	vec2 blur = vec2(2.5) / draw_size;
	float shadow = (paint_at(TEXTURE, shadow_uv).a
		+ paint_at(TEXTURE, shadow_uv + vec2(blur.x, 0.0)).a
		+ paint_at(TEXTURE, shadow_uv - vec2(blur.x, 0.0)).a
		+ paint_at(TEXTURE, shadow_uv + vec2(0.0, blur.y)).a
		+ paint_at(TEXTURE, shadow_uv - vec2(0.0, blur.y)).a) / 5.0;
	vec4 result = over(vec4(0.0), vec4(shadow_color.rgb, shadow_color.a * shadow));
	result = over(result, vec4(edge_color.rgb, silhouette(TEXTURE, uv, 6.0)));
	result = over(result, vec4(rim_color.rgb, silhouette(TEXTURE, uv, 3.5)));
	result = over(result, paint_at(TEXTURE, uv));
	COLOR = result * vec4(icon_modulate.rgb * icon_modulate.a, icon_modulate.a);
}
