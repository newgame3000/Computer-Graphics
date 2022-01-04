#version 330 core
layout (location = 0) in  vec3 coord3f;
layout (location = 1) in  vec3 normalf;

uniform mat4 proj4f;
uniform mat4 view4f;
uniform mat4 model4f;

uniform bool animation;
uniform float t;

out vec3 normalnf;
out vec3 fragCoord;

uniform float width;
uniform float height;

void main(void) {
    if (animation) {
        gl_Position = vec4(coord3f, 1.0);
        
        gl_Position.y = float(gl_Position.y / float(0.5 * float(height)));

        gl_Position.y *= sin(gl_Position.x + t);
        gl_Position.x = float(gl_Position.x / float(0.5 * float(width)));

        gl_Position.x *= cos(t);

        gl_Position.x = float(gl_Position.x * float(0.5 * float(width)));
        gl_Position.y = gl_Position.y *(0.5 * height);
        
        gl_Position = (proj4f * view4f * model4f) * gl_Position;
        //gl_Position = (view4f * model4f) *  vec4(coord3f, 1.0);
    } else {
        gl_Position = (proj4f * view4f * model4f) *  vec4(coord3f, 1.0); 
         //gl_Position = (view4f * model4f) *  vec4(coord3f, 1.0);
    }
    normalnf = normalize(vec3(view4f * model4f * vec4(normalf, 0.0)));
    fragCoord = vec3(view4f * model4f * vec4(coord3f, 1.0));
} 