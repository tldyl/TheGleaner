shader_type canvas_item;
//算法参考：https://www.shadertoy.com/view/XdSfWD

// ==========================================
// 1. 全局随机种子
// ==========================================
group_uniforms Global_Randomness;
uniform float global_seed : hint_range(0.0, 1000.0, 1.0) = 42.0;

// ==========================================
// 2. 矩形框基础参数
// ==========================================
group_uniforms Base_Shape;
uniform vec2 rect_center = vec2(0.5, 0.5);
uniform vec2 rect_size = vec2(0.7, 0.5);
uniform float border_thickness = 0.05;
uniform float border_smoothness = 0.01;

// ==========================================
// 3. 贴图与颜色流动参数
// ==========================================
group_uniforms Texture_Settings;
uniform sampler2D custom_texture : filter_linear_mipmap;
uniform vec4 texture_tint : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec2 uv_tiling = vec2(4.0, 1.0);
uniform float texture_density : hint_range(0.1, 20.0) = 1.0;
uniform float flow_speed : hint_range(-10.0, 10.0) = 2.0;

// ==========================================
// 4. 发光贴图叠加层参数 (Glow Layer)
// ==========================================
group_uniforms Glow_Layer;
uniform sampler2D glow_texture : filter_linear_mipmap;
uniform vec4 glow_tint : source_color = vec4(1.0, 0.8, 0.2, 1.0);
uniform float glow_intensity : hint_range(0.0, 10.0) = 2.0;

// ==========================================
// 5. 线条形态与 3D 交错基础参数
// ==========================================
group_uniforms Line_Shape;
uniform int line_count : hint_range(1, 5) = 3;
uniform float line_phase_offset = 2.0;
uniform float wave_frequency = 3.14;
uniform float wave_speed : hint_range(-20.0, 20.0) = 5.0;
uniform float line_edge_softness : hint_range(0.0, 0.5) = 0.05;

uniform float base_line_thickness : hint_range(0.1, 3.0) = 1.0;
uniform float thickness_variation : hint_range(0.0, 1.0) = 0.5;

// ==========================================
// 6. 速度与振幅随机变化参数
// ==========================================
group_uniforms Line_Speed_Variation;
uniform float speed_scale_min : hint_range(-2.0, 2.0) = 0.6;
uniform float speed_scale_max : hint_range(-2.0, 2.0) = 1.4;

group_uniforms Amplitude_Variation;
uniform float amp_var_frequency : hint_range(0.1, 5.0) = 1.0;
uniform float amp_min : hint_range(0.0, 0.5) = 0.05;
uniform float amp_max : hint_range(0.0, 0.5) = 0.3;

// ==========================================
// 7. 整体明度与深度遮蔽参数
// ==========================================
group_uniforms Line_Brightness;
uniform float line_brightness_min : hint_range(0.0, 1.0) = 0.3;
uniform float line_brightness_max : hint_range(0.0, 1.0) = 1.0;

group_uniforms Depth_Shading;
uniform float depth_darken_strength : hint_range(0.0, 1.0) = 0.6;

// ==========================================
// 8. 扰动参数
// ==========================================
group_uniforms Continuous_Distortion;
uniform float distortion_amount : hint_range(0.0, 0.2) = 0.02;
uniform float distortion_frequency : hint_range(1.0, 100.0) = 15.0;
uniform float distortion_speed : hint_range(0.0, 30.0) = 3.0;

// ==========================================
// 9. 抽帧参数 (Frame Skipping)
// ==========================================
group_uniforms Frame_Skipping;
uniform bool enable_frame_skip = false;
uniform float target_fps : hint_range(1.0, 60.0, 1.0) = 12.0;

uniform vec2 iResolution = vec2(1152.0, 648.0);

float hash(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float smooth_noise(float x) {
    float i = floor(x);
    float f = fract(x);
    float u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    return mix(hash(i), hash(i + 1.0), u);
}

float hash21(vec2 p) {
    p = fract(p * vec2(0.1031, 0.1030));
    p += dot(p, p.yx + 33.33);
    return fract(p.x * p.y);
}

float smooth_noise2D(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

void fragment() {
    // ------------------------------------------
    // 0: 抽帧时间计算
    // ------------------------------------------
    float actual_time = TIME;
    if (enable_frame_skip) {
        actual_time = floor(TIME * target_fps) / target_fps;
    }

    // ------------------------------------------
    // A：计算基础周长映射与距离遮罩
    // ------------------------------------------
    float aspect = iResolution.x / iResolution.y;
    vec2 p = UV - rect_center;
    p.x *= aspect;

    vec2 b = rect_size * 0.5;
    b.x *= aspect;

    vec2 d = abs(p) - b;
    float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);

    float l = 0.0;
    if (d.x > d.y) {
        if (p.x > 0.0) l = 2.0 * b.x + (b.y - p.y);
        else           l = 4.0 * b.x + 2.0 * b.y + (p.y + b.y);
    } else {
        if (p.y > 0.0) l = p.x + b.x;
        else           l = 2.0 * b.x + 2.0 * b.y + (b.x - p.x);
    }

    float perimeter = 4.0 * (b.x + b.y);
    float half_thick = border_thickness * 0.5;

    float base_l_uv = (l / perimeter) * max(round(uv_tiling.x), 1.0);
    float base_y = (dist + half_thick) / border_thickness * uv_tiling.y;
    float actual_tex_density = max(round(uv_tiling.x * texture_density), 1.0) / uv_tiling.x;

    float cont_time = actual_time * distortion_speed;

    // ------------------------------------------
    // B：声明用于深度排序的数组
    // ------------------------------------------
    vec4 cols[5];
    float zs[5];

    for (int i = 0; i < 5; i++) {
        cols[i] = vec4(0.0);
        zs[i] = -9999.0;
    }

    // ------------------------------------------
    // C：独立计算每根线的形态和 3D Z轴深度
    // ------------------------------------------
    for (int i = 0; i < 5; i++) {
        if (i >= line_count) break;

        // 1. 速度随机
        float line_rnd = hash(float(i) * 73.19 + global_seed);
        float speed_mult = mix(speed_scale_min, speed_scale_max, line_rnd);

        float current_flow_speed = flow_speed * speed_mult;
        float current_wave_speed = wave_speed * speed_mult;

        float wave_uv_x = base_l_uv - actual_time * current_flow_speed;

        float phase = float(i) * line_phase_offset;
        float angle = wave_uv_x * wave_frequency + actual_time * current_wave_speed + phase;

        // 2. 振幅噪点随机
        float var_coord = (angle / 6.28318) * amp_var_frequency;
        float rnd_amp = smooth_noise(var_coord + float(i) * 12.34 + global_seed * 2.45);
        float current_amp = mix(amp_min, amp_max, rnd_amp);

        // 3. 亮度随机
        float bright_rnd = hash(float(i) * 41.33 + global_seed * 1.37);
        float line_base_brightness = mix(line_brightness_min, line_brightness_max, bright_rnd);

        // 4. 粗细随机 (基准值 + 变异度)
        float thick_rnd = hash(float(i) * 89.12 + global_seed * 4.56);
        float current_thickness = base_line_thickness * mix(1.0 - thickness_variation, 1.0 + thickness_variation, thick_rnd);

        float wiggle = sin(angle) * current_amp;
        float depth_z = cos(angle);

        // 5. 流体形变噪点
        vec2 distortion_uv_space = vec2(wave_uv_x, base_y) * distortion_frequency;
        vec2 distortion_offset;
        distortion_offset.x = (smooth_noise2D(distortion_uv_space + vec2(cont_time, float(i) * 8.8) + global_seed * 3.14) * 2.0 - 1.0) * distortion_amount;
        distortion_offset.y = (smooth_noise2D(distortion_uv_space + vec2(float(i) * 19.3, cont_time) - global_seed * 2.71) * 2.0 - 1.0) * distortion_amount;

        vec2 sample_uv = vec2((wave_uv_x + distortion_offset.x) * actual_tex_density, base_y + wiggle + distortion_offset.y);
        sample_uv.x = fract(sample_uv.x);

        // 应用计算好的独立粗细
        sample_uv.y = (sample_uv.y - 0.5) / current_thickness + 0.5;

        float mask_y = smoothstep(0.0 - line_edge_softness, 0.0 + line_edge_softness, sample_uv.y) * smoothstep(1.0 + line_edge_softness, 1.0 - line_edge_softness, sample_uv.y);

        // --- 基础图层渲染 ---
        vec4 col = texture(custom_texture, sample_uv) * mask_y;
        col *= texture_tint;
        col.rgb *= line_base_brightness;

        // --- 发光图层叠加 ---
        vec4 glow_tex_col = texture(glow_texture, sample_uv);
        // 通过 rgb、透明度(a)、色相和强度进行加法混合，乘上 mask_y 避免在边缘外发光
        vec3 glow_effect = glow_tex_col.rgb * glow_tex_col.a * glow_tint.rgb * glow_tint.a * glow_intensity;
        col.rgb += glow_effect * mask_y;

        cols[i] = col;
        zs[i] = depth_z;
    }

    // ------------------------------------------
    // D：冒泡排序 (根据 Z 轴从远到近排序)
    // ------------------------------------------
    for (int i = 0; i < 4; i++) {
        for (int j = 0; j < 4; j++) {
            if (zs[j] > zs[j+1]) {
                float temp_z = zs[j];
                zs[j] = zs[j+1];
                zs[j+1] = temp_z;

                vec4 temp_c = cols[j];
                cols[j] = cols[j+1];
                cols[j+1] = temp_c;
            }
        }
    }

    // ------------------------------------------
    // E：渲染合并与连续深度渐变
    // ------------------------------------------
    vec4 final_color = vec4(0.0);

    for (int i = 0; i < 5; i++) {
        if (zs[i] == -9999.0) continue;

        vec4 c = cols[i];

        float normalized_z = clamp((zs[i] + 1.0) / 2.0, 0.0, 1.0);

        float depth_multiplier = mix(1.0 - depth_darken_strength, 1.0, normalized_z);
        c.rgb *= depth_multiplier;

        final_color.rgb = mix(final_color.rgb, c.rgb, c.a);
        final_color.a = c.a + final_color.a * (1.0 - c.a);
    }

    // ------------------------------------------
    // F：应用边缘外框遮罩
    // ------------------------------------------
    float frame_mask = 1.0 - smoothstep(half_thick - border_smoothness, half_thick, abs(dist));

    COLOR = final_color;
    COLOR.a *= frame_mask;
}