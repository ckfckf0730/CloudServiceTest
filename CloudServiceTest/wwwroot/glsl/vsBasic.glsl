attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec4 aColor;

uniform mat4 uWorldMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform vec3 uEye;

varying vec4 vColor;
varying vec4 vWorldPos;
varying vec3 vWorldNormal;
varying vec3 vRay;

void main() {
    vWorldPos = uWorldMatrix  * vec4(aPosition, 1);
    vWorldNormal = uWorldMatrix  * vec4(aNormal, 0);

    gl_Position = uProjectionMatrix * uViewMatrix * vWorldPos;
    vColor = aColor;

    vRay = normalize(vWorldPos.xyz - eye);
}