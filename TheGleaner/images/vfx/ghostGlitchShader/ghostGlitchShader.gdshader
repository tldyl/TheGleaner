shader_type canvas_item;

// --- 基础控制 ---
uniform vec4 spectral_color : source_color = vec4(0.0, 0.45, 1.0, 1.0);
uniform float base_transparency : hint_range(0.0, 1.0) = 0.5;

// --- 银色巡航线控制 ---
uniform float silver_thickness : hint_range(0.0, 0.3) = 0.12;  // 银线粗细
uniform float silver_length : hint_range(0.05, 0.5) = 0.15;    // 能量线的长度 (占周长的百分比)
uniform float silver_speed : hint_range(0.0, 2.0) = 0.3;       // 巡航速度

// --- 随机辅助函数 ---
float rand(float t) { return fract(sin(t * 12.9898) * 43758.5453123); }
float rand2(vec2 co) { return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453123); }

// --- XorDev 能量形态 (内部流体) ---
float get_spectral_shapes(vec2 uv, float t) {
	vec3 p = vec3(uv * 2.5, t * 0.12);
	float d = 1.0;
	for (int i = 0; i < 4; i++) {
		p.xy *= mat2(vec2(cos(t * 0.05), sin(t * 0.05)), vec2(-sin(t * 0.05), cos(t * 0.05)));
		p += cos(p.yzx * d + t * 0.4) / d;
		d /= 0.72;
	}
	return abs(1.2 - dot(sin(p), cos(p.yzx * 0.8))) / 3.0;
}

void fragment() {
	float t = TIME;
	float cycle = mod(t, 12.0); 
	vec2 uv = UV;

// --- 1. 不规则灵魂撕裂 ---
//	bool is_tearing = (cycle > 6.5 && cycle < 9.5);
//	if (is_tearing) {
//		float region = rand2(floor(uv * 1.7 + rand(floor(t))));
//		float drift_anim = sin((cycle - 6.5) * 1.047); 
//		uv.x += (rand(region) - 0.5) * 0.25 * drift_anim;
//		uv.y += (rand(region + 1.2) - 0.5) * 0.1 * drift_anim;
//	}

	// --- 2. 基础采样与遮罩 ---
	vec4 tex = texture(TEXTURE, uv);
	float main_mask = smoothstep(0.1, 0.25, tex.a);
	
	// 定义轮廓带 (边缘区域)
	float edge_mask = smoothstep(0.0, 0.04, tex.a) * (1.0 - smoothstep(0.04, 0.04 + silver_thickness, tex.a));

	if (tex.a < 0.001) discard;

	// --- 3. XorDev 内部填充 ---
	float density = get_spectral_shapes(uv, t);
	vec3 spectral_rgb = spectral_color.rgb * density * 22.0;
	vec3 final_rgb = mix(tex.rgb, spectral_rgb, 0.7);
	final_rgb = tanh(final_rgb * final_rgb / 2.2);

	float final_a = base_transparency * main_mask * (0.6 + density * 1.4);

	// --- 4. 沿轮廓绕圈巡航的银色粗线 (Polar Cruise) ---
	// 计算当前像素相对于中心的角度
	vec2 center_rel = uv - vec2(0.5, 0.5);
	float angle = atan(center_rel.y, center_rel.x); // 范围 -PI 到 PI
	float angle_01 = (angle / 6.283185) + 0.5;      // 映射到 0.0 - 1.0
	
	// 计算能量线的位置
	// 使用 fract 让它随时间在 0-1 之间循环移动
	float rover_pos = fract(t * silver_speed);
	
	// 计算当前像素角度与巡航线位置的距离
	float dist_to_rover = abs(angle_01 - rover_pos);
	// 处理 0.0 和 1.0 首尾相连的情况
	dist_to_rover = min(dist_to_rover, 1.0 - dist_to_rover);
	
	// 只有在距离巡航线中心很近的情况下才点亮
	float rover_mask = smoothstep(silver_length, 0.0, dist_to_rover);
	
	// 银色高亮
	vec3 silver_color = vec3(0.95, 0.98, 1.0) * 12.0;
	float final_silver_line = edge_mask * rover_mask;
	
	final_rgb += silver_color * final_silver_line;
	final_a = max(final_a, final_silver_line * 0.9);

	// --- 5. 异步无规律故障块 ---
	for (float i = 1.0; i <= 3.0; i++) {
		float burst_seed = floor(t * (0.5 + rand(i))); 
		if (rand(burst_seed + i * 51.2) > 0.88) {
			float glitch_t = floor(t * 8.0);
			float block_seed = burst_seed + glitch_t + i;
			vec2 b_pos = vec2(rand(block_seed), rand(block_seed + 0.14));
			// 尺寸限定：8% ~ 16%
			vec2 b_size = (vec2(0.4) + vec2(rand(block_seed + 0.2), rand(block_seed + 0.3)) * 0.4) / 5.0;
			
			if (UV.x > b_pos.x && UV.x < b_pos.x + b_size.x &&
				UV.y > b_pos.y && UV.y < b_pos.y + b_size.y) {
				
				float col_type = rand(block_seed + 0.5);
				vec3 g_col;
				if (col_type < 0.2) g_col = vec3(0.0); // 黑色
				else if (col_type < 0.35) g_col = vec3(1.0, 0.9, 0.1); // 黄色
				else if (col_type < 0.5) g_col = vec3(1.0, 0.1, 0.1); // 红色
				else g_col = vec3(rand(block_seed + 0.6), rand(block_seed + 0.7), rand(block_seed + 0.8));
				
				final_rgb = mix(final_rgb, g_col * 1.5, 0.8);
				final_a = 0.8;
			}
		}
	}

	// --- 6. 边缘整体光晕 ---
	float edge_glow = pow(1.0 - main_mask, 2.0) * density;
	final_rgb += spectral_color.rgb * edge_glow * 4.0;

	COLOR = vec4(final_rgb, final_a);
}