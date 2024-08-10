shader_type canvas_item;

uniform vec4 cape1_color_original : source_color;
uniform vec4 cape1_color_new : source_color;
uniform vec4 cape2_color_original : source_color;
uniform vec4 cape2_color_new : source_color;
uniform vec4 helmet_feathers_original : source_color;
uniform vec4 helmet_feathers_new : source_color;
uniform vec4 armour_dark_original : source_color;
uniform vec4 armour_dark_new : source_color;
uniform vec4 armour_med_original : source_color;
uniform vec4 armour_med_new : source_color;
uniform vec4 armour_light_original : source_color;
uniform vec4 armour_light_new : source_color;

void fragment() {
    vec4 current_color = texture(TEXTURE, UV);
    
    if (distance(current_color, cape1_color_original) < 0.1) {
        COLOR = cape1_color_new;
    }
    else if (distance(current_color, cape2_color_original) < 0.1) {
        COLOR = cape2_color_new;
    }
    else if (distance(current_color, armour_dark_original) < 0.1) {
        COLOR = armour_dark_new;
    }
	else if (distance(current_color, helmet_feathers_original) < 0.1) {
        COLOR = helmet_feathers_new;
    }
    else if (distance(current_color, armour_med_original) < 0.1) {
        COLOR = armour_med_new;
    }
    else if (distance(current_color, armour_light_original) < 0.1) {
        COLOR = armour_light_new;
    }
    else {
        COLOR = current_color;
    }
}