shader_type canvas_item;
render_mode unshaded, blend_add;

// 羽化半径（0=硬边，10=柔和，推荐值）
uniform float feather : hint_range(0, 200.0) = 10.0;
// 圆形大小（0~1，1=填满TextureRect）
uniform float circle_size = 0.5;
uniform vec4 texture_tint : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float alpha_blink : hint_range(0.0, 1.0) = 1.0;

void fragment() {
	// 获取原始图片
	vec4 col = texture(TEXTURE, UV);
	// 计算UV中心距离
	vec2 uv = UV - vec2(0.5);
	float dist = length(uv);
	
	// 核心：圆形遮罩 + 平滑羽化（硬边秒变柔边）
	float alpha = 1.0 - smoothstep(circle_size - feather/100.0, circle_size, dist);
	
	// 最终输出
	COLOR = vec4(col.rgb * texture_tint.rgb, col.a * alpha * texture_tint.a * alpha_blink);
}