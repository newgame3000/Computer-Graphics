#version 330 core
layout (location = 0) in  vec3 coord3f;
layout (location = 1) in  vec3 normalf;

uniform mat4 proj4f;
uniform mat4 view4f;
uniform mat4 model4f;

out vec3 normalnf;
out vec3 fragCoord;

void main(void) {
    normalnf = normalize(vec3(view4f * model4f * vec4(normalf, 0.0)));
    fragCoord = vec3(view4f * model4f * vec4(coord3f, 1.0));
    gl_Position = (proj4f * view4f * model4f) *  vec4(coord3f, 1.0);
} 