shader_type canvas_item;
// Match the reference's soft-white fill without changing the approved texture, silhouette or dark rim.
varying vec4 item_tint;

void vertex() {
	item_tint = COLOR;
}

void fragment() {
	vec4 ink = texture(TEXTURE, UV);
	float fill = smoothstep(0.84, 0.94, ink.r) * smoothstep(0.68, 0.82, ink.g);
	vec3 soft_white = vec3(ink.r, ink.r, ink.r * (247.0 / 253.0));
	COLOR = vec4(mix(ink.rgb, soft_white, fill), ink.a) * item_tint;
}
