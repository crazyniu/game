shader_type canvas_item;
// Calms a full-screen painting behind play so the birds read first: a soft blur, less saturation, and a pale wash
// that is strongest over the play area and fades out towards the screen edges, which keep their detail.
uniform float blur_px = 3.0;          // blur radius in source pixels
uniform float desaturate = 0.25;      // 0 keeps the colours, 1 is grey
uniform float wash = 0.38;            // strength of the pale wash at the centre of the play area
uniform vec4 wash_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 play_area = vec4(0.10, 0.16, 0.90, 0.90);   // x0, y0, x1, y1 (UV) where the wash is full
uniform float feather = 0.14;         // width of the fade from full wash to none (UV)

varying vec4 tint;   // the node's modulate (the night painting fades in by its alpha)

void vertex() {
	tint = COLOR;
}

void fragment() {
	vec2 step_uv = TEXTURE_PIXEL_SIZE * blur_px;
	vec3 sum = vec3(0.0);
	float total = 0.0;
	for (int x = -2; x <= 2; x++) {
		for (int y = -2; y <= 2; y++) {
			float w = exp(-float(x * x + y * y) / 4.5);
			sum += texture(TEXTURE, UV + vec2(float(x), float(y)) * step_uv).rgb * w;
			total += w;
		}
	}
	vec3 c = sum / total;
	float grey = dot(c, vec3(0.299, 0.587, 0.114));
	c = mix(c, vec3(grey), desaturate);
	float mx = smoothstep(play_area.x - feather, play_area.x, UV.x) * (1.0 - smoothstep(play_area.z, play_area.z + feather, UV.x));
	float my = smoothstep(play_area.y - feather, play_area.y, UV.y) * (1.0 - smoothstep(play_area.w, play_area.w + feather, UV.y));
	c = mix(c, wash_color.rgb, wash * mx * my);
	COLOR = vec4(c, 1.0) * tint;
}
