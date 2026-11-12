shader_type canvas_item;
render_mode unshaded;

uniform sampler2D noise_tex : repeat_enable, filter_linear_mipmap;
uniform vec4 fog_color : source_color = vec4(0.8, 0.85, 0.9, 1.0);
uniform float density : hint_range(0.0, 1.0) = 0.6;
uniform vec2 tiling = vec2(2.0, 2.0);
uniform vec2 speed_a = vec2(0.02, 0.01);
uniform vec2 speed_b = vec2(-0.015, 0.008);
uniform float edge_low : hint_range(0.0, 1.0) = 0.3;
uniform float edge_high : hint_range(0.0, 1.0) = 0.8;

group_uniforms pixelation;
uniform bool pixelate = false;
// Number of "pixels" across / down the node's rect
uniform vec2 pixel_grid = vec2(160.0, 90.0);
group_uniforms;

void fragment() {
	vec2 uv = UV;
	if (pixelate) {
		// Snap to the center of the grid cell
		uv = (floor(UV * pixel_grid) + 0.5) / pixel_grid;
	}

	// Derivatives from the *unsnapped* UV, so mip selection doesn't
	// break at cell borders (snapped UVs have zero/huge derivatives)
	vec2 dx = dFdx(UV * tiling);
	vec2 dy = dFdy(UV * tiling);

	// Two samples of the same texture, different scale and direction
	float a = textureGrad(noise_tex, uv * tiling + TIME * speed_a, dx, dy).r;
	float b = textureGrad(noise_tex, uv * tiling * 1.7 + TIME * speed_b, dx * 1.7, dy * 1.7).r;

	float n = (a + b) * 0.5;
	n = smoothstep(edge_low, edge_high, n);

	COLOR = vec4(fog_color.rgb, n * density * fog_color.a);
}