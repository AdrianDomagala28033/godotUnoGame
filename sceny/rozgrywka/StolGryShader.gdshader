shader_type spatial;
render_mode unshaded, cull_disabled;

uniform sampler2D noise_texture : repeat_enable;
uniform vec3 color1 : source_color = vec3(1.0, 0.0, 0.0);
uniform vec3 color2 : source_color = vec3(0.0, 0.0, 1.0);
uniform vec3 kolor_oka : source_color = vec3(0.1, 0.1, 0.1);
uniform float speed : hint_range(0.0, 1.0) = 0.1;
uniform float promien_wewnetrzny : hint_range(0.0, 1.0) = 0.15;
uniform float promien_zewnetrzny : hint_range(0.0, 1.0) = 0.3;
uniform float sila_blysku;
uniform float kierunek;

vec2 rotate_uv(vec2 uv, vec2 pivot, float rotation_angle) {
    float cosa = cos(rotation_angle);
    float sina = sin(rotation_angle);
    uv -= pivot;
    return vec2(
        cosa * uv.x - sina * uv.y,
        cosa * uv.y + sina * uv.x
    ) + pivot;
}

void fragment() {
	vec2 shifted_uv = rotate_uv(UV, vec2(0.5, 0.5), TIME * speed * kierunek);
    float noise_val = texture(noise_texture, shifted_uv).r;
    vec2 distorted_uv = UV + vec2(noise_val * 0.2, noise_val * 0.2);
    vec3 final_color = mix(color1, color2, texture(noise_texture, distorted_uv).r);

    vec2 uv_duszy = UV + (TIME * 0.02);
    float szum_srodka = texture(noise_texture, uv_duszy).r;
    vec3 dynamiczny_kolor_oka = mix(kolor_oka, vec3(0.0), szum_srodka * 0.5);

    float dist = distance(UV, vec2(0.5, 0.5));
	float mask = smoothstep(promien_wewnetrzny, promien_zewnetrzny, dist);
    ALBEDO = mix(dynamiczny_kolor_oka, final_color, mask);
}
