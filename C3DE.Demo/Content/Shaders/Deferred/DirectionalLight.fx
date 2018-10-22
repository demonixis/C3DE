float4x4 World;
float4x4 InvertViewProjection;
float3 LightPosition;
float3 Color;
float3 CameraPosition;
float Intensity;

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
    float3 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = float4(input.Position, 1);
    output.WorldPosition = mul(input.Position, World);
    output.UV = input.UV;
    return output;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(colorSampler, input.UV);

    //read depth
    float4 depth = tex2D(depthSampler, input.UV);
    float depthVal = depth.r;

    // Unlit case: If all depth values are 9 we just draw the color on the lightmap.
    if (depth.r == 0 && depth.g == 0 && depth.b == 0 && depth.a == 0)
        return float4(color.rgb, 0);

    float4 normalData = tex2D(normalSampler, input.UV);
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    float specularPower = normalData.a * 255;
    float specularIntensity = color.a;

    //compute screen-space position
    float4 position;
    position.x = input.UV.x * 2.0f - 1.0f;
    position.y = -(input.UV.x * 2.0f - 1.0f);
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    
    //compute diffuse light
    float3 directionToLight = normalize(LightPosition - input.WorldPosition.xyz);
    float diffuseIntensity = saturate(dot(directionToLight, normal));
    float3 diffuseLight = diffuseIntensity * Color * Intensity;

    float3 reflectionVector = normalize(reflect(-directionToLight, normal));
    float3 directionToCamera = normalize(CameraPosition - position.xyz);
    float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);

    //output the two lights
    return float4(diffuseLight.rgb, specularLight);
}

technique DirectionalLightTechnique
{
    pass Pass0
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderAmbient();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
    }
}