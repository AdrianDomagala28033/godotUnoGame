shader_type canvas_item;

uniform sampler2D noise_texture : repeat_enable;
uniform vec4 color1 : source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 color2 : source_color = vec4(0.0, 0.0, 1.0, 1.0);
uniform float speed : hint_range(0.0, 1.0) = 0.1;

void fragment() {
    vec2 shifted_uv = UV + TIME * speed;
    float noise_val = texture(noise_texture, shifted_uv).r;
    vec2 distorted_uv = UV + vec2(noise_val * 0.2, noise_val * 0.2);
    float final_noise = texture(noise_texture, distorted_uv).r;
    vec4 final_color = mix(color1, color2, final_noise);
    
    COLOR = final_color;
}