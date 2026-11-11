shader_type canvas_item;
render_mode light_only;

uniform sampler2D noise_tex : repeat_enable, filter_linear_mipmap;
uniform vec4 fog_color : source_color = vec4(0.8, 0.85, 0.9, 1.0);
uniform float density : hint_range(0.0, 1.0) = 0.6;
uniform vec2 tiling = vec2(2.0, 2.0);
uniform vec2 speed_a = vec2(0.02, 0.01);
uniform vec2 speed_b = vec2(-0.015, 0.008);
uniform float edge_low : hint_range(0.0, 1.0) = 0.3;
uniform float edge_high : hint_range(0.0, 1.0) = 0.8;

void fragment() {
	// Two samples of the same texture, different scale and direction
	float a = texture(noise_tex, UV * tiling + TIME * speed_a).r;
	float b = texture(noise_tex, UV * tiling * 1.7 + TIME * speed_b).r;

	float n = (a + b) * 0.5;
	n = smoothstep(edge_low, edge_high, n);

	COLOR = vec4(fog_color.rgb, n * density * fog_color.a);
}