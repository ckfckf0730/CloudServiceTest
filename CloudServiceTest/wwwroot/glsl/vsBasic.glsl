attribute vec3 aPosition;
attribute vec4 aColor;

uniform mat4 uWorldMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

varying vec4 vColor;

void main() {
    gl_Position = uProjectionMatrix * uViewMatrix * uWorldMatrix  * vec4(aPosition, 1.0);
    vColor = aColor;
}