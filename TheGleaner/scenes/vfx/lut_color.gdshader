shader_type canvas_item;
render_mode unshaded, blend_add;

uniform sampler2D lut : repeat_disable;
uniform float blink_alpha = 1.0;

void fragment() {
	vec4 texture_color =  texture(TEXTURE, UV);
	vec4 lut_color = texture(lut, texture_color.rr);
	COLOR = vec4(lut_color.rgb, texture_color.a * blink_alpha);
}
