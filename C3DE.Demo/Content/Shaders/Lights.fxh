float3 LightPosition;
// Colors
float4 LightColor;
float4 SpecularLightColor;

//material properties
float3 LightDirection;
float LightSpotAngle;
float LightRange;
float LightFallOff;
float LightIntensity;
float SpecularPower;
float SpecularIntensity;
int LightType;

bool SpecularTextureEnabled;

texture SpecularTexture;
sampler2D specularSampler = sampler_state
{
    Texture = (SpecularTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float3 CalcDirectionalLightColor(float4 worldPosition, float3 worldNormal)
{
	float3 directionToLight = normalize(LightPosition - worldPosition.xyz);
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal.xyz));
    return diffuseIntensity * LightColor * LightIntensity;
}

float3 CalcPointLightColor(float4 worldPosition, float3 worldNormal)
{
	float3 directionToLight = normalize(LightPosition - worldPosition.xyz);
    float diffuseIntensity = saturate(dot(directionToLight, worldNormal.xyz));
    float d = distance(LightPosition, worldPosition.xyz);
    float attenuation = 1.0 - pow(clamp(d / LightRange, 0.0, 1.0), LightFallOff);
    return diffuseIntensity * attenuation * LightColor * LightIntensity;
}

float3 CalcSpotLightColor(float4 worldPosition, float3 worldNormal)
{
    float3 directionToLight = normalize(LightPosition - worldPosition.xyz);
    float3 diffuse = saturate(dot(worldNormal, directionToLight));
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal.xyz));
    float d = dot(directionToLight, normalize(LightDirection));
    float a = cos(LightSpotAngle);
    float attenuation = (a < d) ? attenuation = 1.0 - pow(clamp(a / d, 0.0, 1.0), LightFallOff) : 1.0;
    return diffuseIntensity * attenuation * LightColor * LightIntensity;
}

float3 CalcSpecular(float4 worldPosition, float3 worldNormal, float3 cameraPosition, float2 uv)
{
	float3 directionToLight = normalize(LightPosition - worldPosition.xyz);
	float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal.xyz));
	float3 directionToCamera = normalize(cameraPosition - worldPosition.xyz);
	float4 specular = SpecularLightColor * SpecularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), SpecularPower);
   
	if (SpecularTextureEnabled)
		specular *= tex2D(specularSampler, uv);
   
	return specular;
}

float3 CalcLightFactor(float4 worldPosition, float3 worldNormal)
{
    if (LightType == 1)
        return CalcDirectionalLightColor(worldPosition, worldNormal);
    else if (LightType == 2)
        return CalcPointLightColor(worldPosition, worldNormal);
    else if (LightType == 3)
        return CalcSpotLightColor(worldPosition, worldNormal);
	
    return float3(1.0, 1.0, 1.0);
}