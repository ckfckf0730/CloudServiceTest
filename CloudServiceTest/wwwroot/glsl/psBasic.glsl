precision mediump float;

uniform sampler2D uSampler; 
uniform vec4 uColor;
//uniform vec3 uLightDir;
//uniform vec4 uLightColor;

varying vec4 vWorldPos;
varying vec3 vWorldNormal;
varying vec3 vRay;
varying vec2 vUv;

void main() {
    vec3 light = normalize(vec3(1,-2,-2));
	vec3 lightColor = vec3(1, 1, 1);

    float diffuseB = clamp(dot(-light, vWorldNormal), 0.0, 1.0) * 0.75;

	vec3 refLight = normalize(reflect(light, vWorldNormal));
	float specularB = pow(clamp(dot(refLight, -vRay), 0.0, 1.0), 0.5);

	vec4 color = texture2D(uSampler, vUv);

	vec4 finalColor = color * diffuseB;
	finalColor = finalColor * uColor;
	finalColor = finalColor + finalColor * specularB;

    gl_FragColor = vec4(finalColor.xyz, 1);
}   