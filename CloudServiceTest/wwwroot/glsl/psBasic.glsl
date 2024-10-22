precision mediump float;

//uniform vec3 uLightDir;
//uniform vec4 uLightColor;

varying vec4 vColor;
varying vec4 vWorldPos;
varying vec3 vWorldNormal;
varying vec3 vRay;


void main() {
    vec3 light = normalize(float3(1,-1,1));
	vec3 lightColor = float3(1, 1, 1);

    float diffuseB = clamp(dot(-light, vWorldNormal), 0, 1) * 0.75;

	float3 refLight = normalize(reflect(-light, vWorldNormal));
	float specularB = pow(clamp(dot(refLight, -vRay), 0, 1), 0.5);

	vColor = vColor * diffuseB;
	vColor = vColor + vColor * specularB;

    gl_FragColor = vColor;
}   