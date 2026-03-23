// Constants
#if SM4
#define MAX_LIGHT_COUNT 128
#else
// SM3 (OpenGL): keep the loop statically bounded for MojoShader while still allowing
// a slightly higher light budget than before.
#define MAX_LIGHT_COUNT 16
#endif

// Lighting
// LightData.x: Type: Directional, Point, Spot
// LightData.y: Intensity
// LightData.z: Range
// LightData.w: FallOff
// SpotData.xyz: Direction (SM4 only)
// SpotData.w: Angle (SM4 only)

float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float4 LightData[MAX_LIGHT_COUNT];
#if SM4
float4 SpotData[MAX_LIGHT_COUNT];
#endif
int LightCount = 0;

float GetLightRangeAttenuation(int i, float distanceToLight)
{
    float normalizedDistance = saturate(distanceToLight / max(LightData[i].z, 0.0001));
#if SM4
    return 1.0 - pow(normalizedDistance, LightData[i].w);
#else
    float squared = normalizedDistance * normalizedDistance;
    float quartic = squared * squared;
    return 1.0 - lerp(squared, quartic, saturate(LightData[i].w - 1.0));
#endif
}

#if SM4
float GetSpotAttenuation(int i, float3 directionToLight)
{
    float spotCos = dot(directionToLight, -normalize(SpotData[i].xyz));
    float angleCos = cos(SpotData[i].w);

    if (spotCos <= angleCos)
        return 0.0;

    return saturate((spotCos - angleCos) / max(1.0 - angleCos, 0.0001));
}
#endif

float3 CalculateOneLight(int i, float3 worldPosition, float3 worldNormal, float3 viewDirection, float3 specularColor, int specularPower)
{
    float lightType = LightData[i].x;
    float3 lightVector = LightPosition[i] - worldPosition;
    float distanceSquared = dot(lightVector, lightVector);
    float inverseDistance = rsqrt(max(distanceSquared, 0.0001));
    float distanceToLight = distanceSquared * inverseDistance;
    float3 directionToLight = lightVector * inverseDistance;

    if (lightType < 0.5)
        directionToLight = normalize(-LightPosition[i]);

    float diffuseIntensity = saturate(dot(directionToLight, worldNormal));

    if (diffuseIntensity <= 0.0)
        return FLOAT3(0.0);

    float lightIntensity = LightData[i].y;

    if (lightType > 0.5)
        lightIntensity *= GetLightRangeAttenuation(i, distanceToLight);

#if SM4
    if (lightType > 1.5)
        lightIntensity *= GetSpotAttenuation(i, directionToLight);
#endif

    if (lightIntensity <= 0.0)
        return FLOAT3(0.0);

    float3 halfVector = normalize(directionToLight + viewDirection);
    float specularIntensity = pow(saturate(dot(worldNormal, halfVector)), specularPower);
    float3 lightColor = LightColor[i] * lightIntensity;
    float3 diffuse = lightColor * diffuseIntensity;
    float3 specular = lightColor * specularColor * specularIntensity;

    return diffuse + specular;
}
