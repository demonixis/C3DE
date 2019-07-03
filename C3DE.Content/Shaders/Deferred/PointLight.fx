float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InvertViewProjection;

float3 Color;
float3 LightPosition;
float Radius;
float Intensity = 1.0f;

float3 CameraPosition;

texture ColorMap;
sampler colorSampler = sampler_state
{
    Texture = (ColorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture DepthMap;
sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    // Process geometry coordinates
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    output.Position = mul(viewPosition, Projection);
    output.ScreenPosition = output.Position;
    return output;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;

    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    float4 color = tex2D(colorSampler, texCoord);

    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, texCoord);
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    float specularPower = normalData.a * 255;
    float specularIntensity = color.a;

    //read depth
    float4 depth = tex2D(depthSampler, texCoord);
    float depthVal = depth.r;

    // Unlit case: If all depth values are 9 we just draw the color on the lightmap.
    if (depth.r == 0 && depth.g == 0 && depth.b == 0)
        return float4(color.rgb, 0);

    //compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //surface-to-light vector
    float3 lightVector = LightPosition - position.xyz;
    float attenuation = saturate(1.0f - length(lightVector) / Radius);
    lightVector = normalize(lightVector);

    //compute diffuse light
    float NdL = max(0, dot(normal, lightVector));
    float3 diffuseLight = NdL * Color.rgb;

    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    float3 directionToCamera = normalize(CameraPosition - position.xyz);
    float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);

    return attenuation * Intensity * float4(diffuseLight.rgb, specularLight);
}

technique PointLightTechnique
{
    pass Pass0
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderAmbient();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
    }
}
