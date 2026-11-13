shader_type canvas_item;
// A costume that lives on the bird itself (no particles around it): its outer petals, wings and crown tips flutter
// while the parts against the body stay put, a soft highlight sweeps across it now and then, and its golden flower
// centres glow. Drawn on the outfit overlay (BirdView._wear), so it follows every painted frame.
uniform vec2 core = vec2(0.48, 0.56);   // body centre in the overlay canvas (UV); the costume is rigid around it
uniform float core_radius = 0.20;       // fully rigid inside this radius (UV)
uniform float flutter_px = 3.5;         // largest flutter offset, in overlay pixels
uniform float flutter_speed = 2.4;
uniform float shimmer = 0.32;           // strength of the sweeping highlight
uniform float shimmer_period = 3.2;     // seconds between sweeps
uniform float glow = 0.42;              // strength of the flower-centre glow
uniform vec4 glow_color : source_color = vec4(1.0, 0.86, 0.32, 1.0);
uniform float seed = 0.0;               // per-bird phase so a flock never moves in step

varying vec4 tint;

void vertex() {
	tint = COLOR;
}

void fragment() {
	float t = TIME + seed * 7.0;
	float edge = smoothstep(core_radius, core_radius + 0.22, distance(UV, core));
	vec2 wave = vec2(sin(t * flutter_speed + UV.y * 19.0), cos(t * flutter_speed * 0.83 + UV.x * 15.0));
	vec4 c = texture(TEXTURE, UV + wave * flutter_px * TEXTURE_PIXEL_SIZE * edge);
	// A narrow diagonal band of light crosses the costume once per period, then rests.
	float phase = fract(t / shimmer_period) * 2.4 - 0.5;
	float band = exp(-pow(((UV.x + UV.y) * 0.5 - phase) / 0.05, 2.0)) * shimmer;
	// Golden flower centres: warm, saturated pixels.
	float gold = smoothstep(0.18, 0.32, min(c.r, c.g) - c.b) * step(0.55, c.g) * step(c.g * 0.97, c.r);   // not leaf green
	float pulse = (0.55 + 0.45 * sin(t * 2.8)) * glow * gold;
	c.rgb += vec3(1.0, 0.97, 0.88) * band * c.a + glow_color.rgb * pulse * c.a;
	COLOR = c * tint;
}
