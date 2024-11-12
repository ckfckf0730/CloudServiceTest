attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec2 aUv;

uniform mat4 uWorldMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform vec3 uEye;

varying vec4 vWorldPos;
varying vec3 vWorldNormal;
varying vec3 vRay;
varying vec2 vUv;

void main() {
    vWorldPos = uWorldMatrix  * vec4(aPosition, 1);
    vWorldNormal = (uWorldMatrix  * vec4(aNormal, 0)).xyz;
    vWorldNormal = normalize(vWorldNormal);

    gl_Position = uProjectionMatrix * uViewMatrix * vWorldPos;
    vUv = aUv;

    vRay = normalize(vWorldPos.xyz - uEye);
}