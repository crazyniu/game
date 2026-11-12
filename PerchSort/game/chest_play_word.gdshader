shader_type canvas_item;
// Normalize existing approved lettering to white ink and a deep green outline.
varying vec4 item_tint;
void vertex() { item_tint = COLOR; }
void fragment() {
	vec4 ink = texture(TEXTURE, UV);
	float fill = smoothstep(0.55, 0.82, max(ink.r, max(ink.g, ink.b)));
	vec3 white_ink = vec3(max(ink.r, max(ink.g, ink.b)));
	vec3 outline = vec3(0.055, 0.24, 0.035);
	COLOR = vec4(mix(outline, white_ink, fill), ink.a) * item_tint;
}
