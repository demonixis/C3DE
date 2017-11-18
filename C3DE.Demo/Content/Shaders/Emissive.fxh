float3 EmissiveColor;
float EmissiveIntensity;
bool EmissiveTextureEnabled;

texture EmissiveTexture;
sampler2D emissiveSampler = sampler_state
{
    Texture = (EmissiveTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 CalcEmissiveColor(float2 uv) : COLOR0
{
    float3 emissiveColor = EmissiveColor;

    if (EmissiveTextureEnabled)
        emissiveColor = tex2D(emissiveSampler, uv).xyz;

    return float4(emissiveColor * EmissiveIntensity, 1.0);
}