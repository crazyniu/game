shader_type canvas_item;
// Hint spotlight: the board dims except two soft rounded holes (the move's source and target branches),
// each ringed by a slowly pulsing warm glow. Rects are design pixels (x, y, w, h); w = 0 disables a hole.

uniform vec2 canvas_size = vec2(1080.0, 1920.0);
uniform vec4 hole_a = vec4(0.0);
uniform vec4 hole_b = vec4(0.0);
uniform float radius = 56.0;
uniform float feather = 22.0;
uniform float dim = 0.58;
uniform vec4 shade : source_color = vec4(0.04, 0.07, 0.10, 1.0);
uniform vec4 glow : source_color = vec4(1.0, 0.84, 0.36, 1.0);
uniform float fade = 1.0;

float hole_distance(vec2 p, vec4 r) {
	if (r.z <= 0.0) {
		return 1e6;
	}
	vec2 half_size = r.zw * 0.5;
	vec2 q = abs(p - (r.xy + half_size)) - (half_size - vec2(radius));
	return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
}

void fragment() {
	vec2 p = UV * canvas_size;
	float d = min(hole_distance(p, hole_a), hole_distance(p, hole_b));
	float outside = smoothstep(0.0, feather, d);
	float pulse = 0.65 + 0.35 * sin(TIME * 3.2);
	float ring = exp(-abs(d - 4.0) / 7.0) * pulse;
	vec3 colour = mix(shade.rgb, glow.rgb, clamp(ring * 1.4, 0.0, 1.0));
	float alpha = max(dim * outside, ring * 0.85);
	COLOR = vec4(colour, alpha * fade);
}
