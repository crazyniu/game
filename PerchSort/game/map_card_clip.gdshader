shader_type canvas_item;
// Clip the retained painting in card-local design pixels, preserving its aspect-filled crop.
uniform vec2 window_size;
uniform vec2 image_size;
uniform vec2 image_offset;
uniform float radius = 30.0;

void fragment() {
	vec2 point = UV * image_size + image_offset;
	vec2 q = abs(point - window_size * 0.5) - (window_size * 0.5 - vec2(radius));
	float distance = length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - radius;
	float edge = max(fwidth(distance), 0.001);
	COLOR.a *= 1.0 - smoothstep(-edge, edge, distance);
}
