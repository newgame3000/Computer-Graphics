#version 330 core
layout (location = 0) in  vec3 coord3f;
out vec3 color;
uniform mat4 proj4f;
uniform mat4 view4f;
uniform mat4 model4f;

void main(void) {
    gl_Position = (proj4f * view4f * model4f) *  vec4(coord3f, 1.0);
} 