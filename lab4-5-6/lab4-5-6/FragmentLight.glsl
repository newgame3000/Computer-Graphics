#version 330 core

struct Material {
    vec3 ka;
    vec3 kd;
    vec3 ks;
    float p;
};

struct Light {
    vec3 ia;
    vec3 il;
    vec3 position;
};

in vec3 normalnf;
in vec3 fragCoord;

out vec4 color;

uniform mat4 view4f;

uniform float k;
uniform vec3 c;
uniform vec3 camera;
uniform Material m;
uniform Light l;

void main() {
    
    vec3 position = vec3(view4f * vec4(l.position, 1) );
    
    vec3 background = m.ka * l.ia;
    vec3 diffuse = m.kd * l.il;
    
    vec3 toLight = normalize(position - fragCoord);
    diffuse *= max(dot(toLight, normalnf), 0);
    
    vec3 specular = m.ks * l.il;

    if (dot(toLight, normalnf) > 1e-3) {
        vec3 r = reflect(-toLight, normalnf);
        specular *= pow(max(dot(r, normalize(camera - fragCoord)), 0), m.p);
    } else {
        specular *= 0;
    }
    
   color = vec4(c * background + (c * diffuse + c * specular) / (k + length(position - fragCoord) / 20), 1);
}