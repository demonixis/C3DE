float4x4 LightView;
float4x4 LightProjection;
float ShadowStrength;
float ShadowBias;
bool ShadowEnabled = false;

texture ShadowMap;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMap);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float CalcShadow(float4 worldPosition)
{
	if (ShadowEnabled == false)
		return 1.0f;
		
    float4x4 LightViewProj = mul(LightView, LightProjection);
    float4 lightingPosition = mul(worldPosition, LightViewProj);

    if (lightingPosition.w <= 0.0f)
        return 1.0f;
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;
    float projectedDepth = lightingPosition.z / lightingPosition.w;

    if (ShadowTexCoord.x < 0.0f || ShadowTexCoord.x > 1.0f ||
        ShadowTexCoord.y < 0.0f || ShadowTexCoord.y > 1.0f ||
        projectedDepth < 0.0f || projectedDepth > 1.0f)
        return 1.0f;

    // Get the current depth stored in the shadow map
    float shadowdepth = tex2D(shadowSampler, ShadowTexCoord).r;
    float ourdepth = projectedDepth - ShadowBias;
    
    if (shadowdepth < ourdepth)
        return 0.5f / ShadowStrength;
		    
    return 1.0f;
}
