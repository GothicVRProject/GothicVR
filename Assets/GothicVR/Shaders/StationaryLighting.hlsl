#ifndef STATIONARY_LIGHTING_INCLUDED
#define STATIONARY_LIGHTING_INCLUDED

#define MAX_TOTAL_STATIONARY_LIGHTS 512
#define MAX_AFFECTING_STATIONARY_LIGHTS 16

float4 _GlobalStationaryLightPositionsAndAttenuation[MAX_TOTAL_STATIONARY_LIGHTS];
real3 _GlobalStationaryLightColors[MAX_TOTAL_STATIONARY_LIGHTS];

half3 AdditionalStationaryDiffuse(uint lightIndex, real3 worldPos, real3 normal)
{
	float4 lightPosAndAttenuation = _GlobalStationaryLightPositionsAndAttenuation[lightIndex];
	float3 lightVector = lightPosAndAttenuation.xyz - worldPos;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
	half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
	float diffuseDot = saturate(dot(lightDirection, normal));

	return _GlobalStationaryLightColors[lightIndex] * CustomDistanceAttenuation(distanceSqr, lightPosAndAttenuation.w) * diffuseDot * _PointLightIntensity;
}

#endif