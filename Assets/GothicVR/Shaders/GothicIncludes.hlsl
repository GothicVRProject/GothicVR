#ifndef GOTHIC_COMMON_INCLUDED
#define GOTHIC_COMMON_INCLUDED

#define REFERENCE_TEX_ARRAY_SIZE 256

float3 _SunDirection;
real3 _SunColor;
real3 _AmbientColor;
real _PointLightIntensity;

float3 ApplyFog(float3 color, float3 worldPos)
{
	float viewDistance = length(_WorldSpaceCameraPos - worldPos);
	float unityFogFactor = saturate(viewDistance * unity_FogParams.z + unity_FogParams.w);
	return lerp(unity_FogColor.rgb, color, unityFogFactor);
}

float CalcMipLevel(float2 uv)
{
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);
	float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));

	return max(0.0, 0.5 * log2(delta_max_sqr));
}

float CustomDistanceAttenuation(float distanceSqr, float rcpSqrRange)
{
	return 1 - saturate(distanceSqr * rcpSqrRange);
}

Light CustomGetAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
                // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
                half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
                half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
                half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
                uint lightLayerMask = _AdditionalLightsBuffer[perObjectLightIndex].layerMask;
#else
	float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
	half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
	half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
	half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
	uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
	float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

	half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
                // full-float precision required on some platforms
	float attenuation = CustomDistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.x) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

	Light light;
	light.direction = lightDirection;
	light.distanceAttenuation = attenuation;
	light.shadowAttenuation = 1.0; // This value can later be overridden in GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
	light.color = color;
	light.layerMask = lightLayerMask;

	return light;
}

half3 AdditionalUnityLightDiffuse(Light light, real3 normal)
{
	real diffuseDot = saturate(dot(light.direction, normal));
	return light.color * light.distanceAttenuation * diffuseDot * _PointLightIntensity;
}

half3 SunAndAmbientDiffuse(float3 normal, half3 vertexShadowmap)
{
	half diffuseDot = saturate(dot(normal, -_SunDirection));
	return saturate(diffuseDot * _SunColor * vertexShadowmap + _AmbientColor);
}

#endif