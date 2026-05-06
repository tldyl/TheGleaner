shader_type canvas_item;

uniform float intensity : hint_range(0, 1) = 1.0;
uniform vec2 circle_center = vec2(0.5, 0.5);
uniform float ratio = 1.0;
uniform float radius = 0.0;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
    // 获取当前屏幕像素的原始颜色
    vec4 screenColor = texture(screen_texture, SCREEN_UV);
    vec2 delta = SCREEN_UV - circle_center;
    delta.x *= ratio;
    float dist = length(delta);

    if (dist > radius) {
        COLOR = screenColor;
    } else {
        // 计算灰度值：使用Rec.601标准亮度系数，符合人眼感知，比简单平均更自然
        float gray = dot(screenColor.rgb, vec3(0.299, 0.587, 0.114));
        vec3 grayColor = vec3(gray);

        // 混合原始颜色和灰度颜色，实现强度过渡
        COLOR.rgb = mix(screenColor.rgb, grayColor, intensity);
        // 保持原始透明度不变
        COLOR.a = screenColor.a;
    }
}