shader_type canvas_item;

uniform float smoothing : hint_range(0.0, 1.0) = 0.1;
uniform float outline_thickness : hint_range(0.0, 1.0) = 0.0;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    // Apply smoothing
    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, tex_color.a);
    
    // Apply outline
    if (outline_thickness > 0.0) {
        float outline = texture(TEXTURE, UV + vec2(-1.0, 0.0) * outline_thickness * TEXTURE_PIXEL_SIZE).a +
                        texture(TEXTURE, UV + vec2(1.0, 0.0) * outline_thickness * TEXTURE_PIXEL_SIZE).a +
                        texture(TEXTURE, UV + vec2(0.0, -1.0) * outline_thickness * TEXTURE_PIXEL_SIZE).a +
                        texture(TEXTURE, UV + vec2(0.0, 1.0) * outline_thickness * TEXTURE_PIXEL_SIZE).a;
        outline = min(outline, 1.0);
        tex_color = mix(outline_color, tex_color, alpha);
        alpha = max(alpha, outline * outline_color.a);
    }
    
    COLOR = vec4(tex_color.rgb, alpha);
}