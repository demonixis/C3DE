/*
 *  Sample has been derived from:
 *
 *  NVIDIA FXAA 3.11 by TIMOTHY LOTTES (https://gist.github.com/bkaradzic/6011431)
 *
 * and subsequently adapted for MonoGame:
 *
 *  - texture -> Texture2D
 *  - tex2Dlod -> tex2D
 *  - Position0 -> SV_Position
 *  - ...
 *
 * PC Quality algorithm has to be compiled as "ps_5_0", since texture lookup will be too complex otherwise, causing error X5426.
 *
 */

// helper
#define FxaaTexTop(t, p) tex2D(t, float4(p, 0.0, 0.0))
#define FxaaTexOff(t, p, o, r) tex2D(t, float4(p + (o * r), 0, 0))
#define FxaaSat(x) saturate(x)

// flags
#define FXAA_GREEN_AS_LUMA 1

// quality ("quality" function only)
#define FXAA_QUALITY__PRESET 39

/*=======FXAA QUALITY - EXTREME QUALITY=====================================*/

#if (FXAA_QUALITY__PRESET == 39)
#define FXAA_QUALITY__PS 12
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.0
#define FXAA_QUALITY__P2 1.0
#define FXAA_QUALITY__P3 1.0
#define FXAA_QUALITY__P4 1.0
#define FXAA_QUALITY__P5 1.5
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 2.0
#define FXAA_QUALITY__P9 2.0
#define FXAA_QUALITY__P10 4.0
#define FXAA_QUALITY__P11 8.0
#endif

/*=======APPLICATION INPUT==================================================*/
// for console algorithm
float4 ConsoleOpt1;
float4 ConsoleOpt2;
float ConsoleEdgeSharpness;
float ConsoleEdgeThreshold;
float ConsoleEdgeThresholdMin;
// for pc quality algorithm
float fxaaQualitySubpix;
float fxaaQualityEdgeThreshold;
float fxaaQualityEdgeThresholdMin;
// general
float invViewportWidth;
float invViewportHeight;

// texturing
Texture2D texScreen;
sampler splScreen = sampler_state
{
    texture = <texScreen>;
};

/*=======LUMINOSITY FUNCTION================================================*/

float FxaaLuma(float4 rgba)
{
    rgba.w = dot(rgba.rgb, float3(0.299, 0.587, 0.114));
    return rgba.w;
}

/*=======FXAA3 QUALITY - PC=================================================*/

float4 FxaaPixelShader_PC(
    float2 pos,
    sampler2D tex,
    float2 fxaaQualityRcpFrame,
    float fxaaQualitySubpix,
    float fxaaQualityEdgeThreshold,
    float fxaaQualityEdgeThresholdMin)
{
    float2 posM;
    posM.x = pos.x;
    posM.y = pos.y;
    // check gather 4 alpha
    float4 rgbyM = FxaaTexTop(tex, posM);
#if (FXAA_GREEN_AS_LUMA == 0)
#define lumaM_PC rgbyM.w
#else
#define lumaM_PC rgbyM.y
#endif
    float lumaS = FxaaLuma(FxaaTexOff(tex, posM, int2( 0, 1), fxaaQualityRcpFrame.xy));
    float lumaE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1, 0), fxaaQualityRcpFrame.xy));
    float lumaN = FxaaLuma(FxaaTexOff(tex, posM, int2( 0,-1), fxaaQualityRcpFrame.xy));
    float lumaW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1, 0), fxaaQualityRcpFrame.xy));

    float maxSM = max(lumaS, lumaM_PC);
    float minSM = min(lumaS, lumaM_PC);
    float maxESM = max(lumaE, maxSM);
    float minESM = min(lumaE, minSM);
    float maxWN = max(lumaN, lumaW);
    float minWN = min(lumaN, lumaW);
    float rangeMax = max(maxWN, maxESM);
    float rangeMin = min(minWN, minESM);
    float rangeMaxScaled = rangeMax * fxaaQualityEdgeThreshold;
    float range = rangeMax - rangeMin;
    float rangeMaxClamped = max(fxaaQualityEdgeThresholdMin, rangeMaxScaled);
    bool earlyExit = range < rangeMaxClamped;

    if (earlyExit)
        return rgbyM;

    float lumaNW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1,-1), fxaaQualityRcpFrame.xy));
    float lumaSE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1, 1), fxaaQualityRcpFrame.xy));
    float lumaNE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1,-1), fxaaQualityRcpFrame.xy));
    float lumaSW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1, 1), fxaaQualityRcpFrame.xy));

    float lumaNS = lumaN + lumaS;
    float lumaWE = lumaW + lumaE;
    float subpixRcpRange = 1.0 / range;
    float subpixNSWE = lumaNS + lumaWE;
    float edgeHorz1 = (-2.0 * lumaM_PC) + lumaNS;
    float edgeVert1 = (-2.0 * lumaM_PC) + lumaWE;

    float lumaNESE = lumaNE + lumaSE;
    float lumaNWNE = lumaNW + lumaNE;
    float edgeHorz2 = (-2.0 * lumaE) + lumaNESE;
    float edgeVert2 = (-2.0 * lumaN) + lumaNWNE;

    float lumaNWSW = lumaNW + lumaSW;
    float lumaSWSE = lumaSW + lumaSE;
    float edgeHorz4 = (abs(edgeHorz1) * 2.0) + abs(edgeHorz2);
    float edgeVert4 = (abs(edgeVert1) * 2.0) + abs(edgeVert2);
    float edgeHorz3 = (-2.0 * lumaW) + lumaNWSW;
    float edgeVert3 = (-2.0 * lumaS) + lumaSWSE;
    float edgeHorz = abs(edgeHorz3) + edgeHorz4;
    float edgeVert = abs(edgeVert3) + edgeVert4;

    float subpixNWSWNESE = lumaNWSW + lumaNESE;
    float lengthSign = fxaaQualityRcpFrame.x;
    bool horzSpan = edgeHorz >= edgeVert;
    float subpixA = subpixNSWE * 2.0 + subpixNWSWNESE;

    if (!horzSpan)
        lumaN = lumaW;
    if (!horzSpan)
        lumaS = lumaE;
    if (horzSpan)
        lengthSign = fxaaQualityRcpFrame.y;
    float subpixB = (subpixA * (1.0 / 12.0)) - lumaM_PC;

    float gradientN = lumaN - lumaM_PC;
    float gradientS = lumaS - lumaM_PC;
    float lumaNN = lumaN + lumaM_PC;
    float lumaSS = lumaS + lumaM_PC;
    bool pairN = abs(gradientN) >= abs(gradientS);
    float gradient = max(abs(gradientN), abs(gradientS));
    if (pairN)
        lengthSign = -lengthSign;
    float subpixC = FxaaSat(abs(subpixB) * subpixRcpRange);

    float2 posB;
    posB.x = posM.x;
    posB.y = posM.y;
    float2 offNP;
    offNP.x = (!horzSpan) ? 0.0 : fxaaQualityRcpFrame.x;
    offNP.y = (horzSpan) ? 0.0 : fxaaQualityRcpFrame.y;
    if (!horzSpan)
        posB.x += lengthSign * 0.5;
    if (horzSpan)
        posB.y += lengthSign * 0.5;

    float2 posN;
    posN.x = posB.x - offNP.x * FXAA_QUALITY__P0;
    posN.y = posB.y - offNP.y * FXAA_QUALITY__P0;
    float2 posP;
    posP.x = posB.x + offNP.x * FXAA_QUALITY__P0;
    posP.y = posB.y + offNP.y * FXAA_QUALITY__P0;
    float subpixD = ((-2.0) * subpixC) + 3.0;
    float lumaEndN = FxaaLuma(FxaaTexTop(tex, posN));
    float subpixE = subpixC * subpixC;
    float lumaEndP = FxaaLuma(FxaaTexTop(tex, posP));

    if (!pairN)
        lumaNN = lumaSS;
    float gradientScaled = gradient * 1.0 / 4.0;
    float lumaMM = lumaM_PC - lumaNN * 0.5;
    float subpixF = subpixD * subpixE;
    bool lumaMLTZero = lumaMM < 0.0;

    lumaEndN -= lumaNN * 0.5;
    lumaEndP -= lumaNN * 0.5;
    bool doneN = abs(lumaEndN) >= gradientScaled;
    bool doneP = abs(lumaEndP) >= gradientScaled;
    if (!doneN)
        posN.x -= offNP.x * FXAA_QUALITY__P1;
    if (!doneN)
        posN.y -= offNP.y * FXAA_QUALITY__P1;
    bool doneNP = (!doneN) || (!doneP);
    if (!doneP)
        posP.x += offNP.x * FXAA_QUALITY__P1;
    if (!doneP)
        posP.y += offNP.y * FXAA_QUALITY__P1;

    // decide on quality

    if (doneNP)
    {
        if (!doneN)
            lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
        if (!doneP)
            lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
        if (!doneN)
            lumaEndN = lumaEndN - lumaNN * 0.5;
        if (!doneP)
            lumaEndP = lumaEndP - lumaNN * 0.5;
        doneN = abs(lumaEndN) >= gradientScaled;
        doneP = abs(lumaEndP) >= gradientScaled;
        if (!doneN)
            posN.x -= offNP.x * FXAA_QUALITY__P2;
        if (!doneN)
            posN.y -= offNP.y * FXAA_QUALITY__P2;
        doneNP = (!doneN) || (!doneP);
        if (!doneP)
            posP.x += offNP.x * FXAA_QUALITY__P2;
        if (!doneP)
            posP.y += offNP.y * FXAA_QUALITY__P2;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 3)
        if (doneNP)
        {
            if (!doneN)
                lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
            if (!doneP)
                lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
            if (!doneN)
                lumaEndN = lumaEndN - lumaNN * 0.5;
            if (!doneP)
                lumaEndP = lumaEndP - lumaNN * 0.5;
            doneN = abs(lumaEndN) >= gradientScaled;
            doneP = abs(lumaEndP) >= gradientScaled;
            if (!doneN)
                posN.x -= offNP.x * FXAA_QUALITY__P3;
            if (!doneN)
                posN.y -= offNP.y * FXAA_QUALITY__P3;
            doneNP = (!doneN) || (!doneP);
            if (!doneP)
                posP.x += offNP.x * FXAA_QUALITY__P3;
            if (!doneP)
                posP.y += offNP.y * FXAA_QUALITY__P3;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 4)
            if (doneNP)
            {
                if (!doneN)
                    lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                if (!doneP)
                    lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                if (!doneN)
                    lumaEndN = lumaEndN - lumaNN * 0.5;
                if (!doneP)
                    lumaEndP = lumaEndP - lumaNN * 0.5;
                doneN = abs(lumaEndN) >= gradientScaled;
                doneP = abs(lumaEndP) >= gradientScaled;
                if (!doneN)
                    posN.x -= offNP.x * FXAA_QUALITY__P4;
                if (!doneN)
                    posN.y -= offNP.y * FXAA_QUALITY__P4;
                doneNP = (!doneN) || (!doneP);
                if (!doneP)
                    posP.x += offNP.x * FXAA_QUALITY__P4;
                if (!doneP)
                    posP.y += offNP.y * FXAA_QUALITY__P4;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 5)
                if (doneNP)
                {
                    if (!doneN)
                        lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                    if (!doneP)
                        lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                    if (!doneN)
                        lumaEndN = lumaEndN - lumaNN * 0.5;
                    if (!doneP)
                        lumaEndP = lumaEndP - lumaNN * 0.5;
                    doneN = abs(lumaEndN) >= gradientScaled;
                    doneP = abs(lumaEndP) >= gradientScaled;
                    if (!doneN)
                        posN.x -= offNP.x * FXAA_QUALITY__P5;
                    if (!doneN)
                        posN.y -= offNP.y * FXAA_QUALITY__P5;
                    doneNP = (!doneN) || (!doneP);
                    if (!doneP)
                        posP.x += offNP.x * FXAA_QUALITY__P5;
                    if (!doneP)
                        posP.y += offNP.y * FXAA_QUALITY__P5;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 6)
                    if (doneNP)
                    {
                        if (!doneN)
                            lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                        if (!doneP)
                            lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                        if (!doneN)
                            lumaEndN = lumaEndN - lumaNN * 0.5;
                        if (!doneP)
                            lumaEndP = lumaEndP - lumaNN * 0.5;
                        doneN = abs(lumaEndN) >= gradientScaled;
                        doneP = abs(lumaEndP) >= gradientScaled;
                        if (!doneN)
                            posN.x -= offNP.x * FXAA_QUALITY__P6;
                        if (!doneN)
                            posN.y -= offNP.y * FXAA_QUALITY__P6;
                        doneNP = (!doneN) || (!doneP);
                        if (!doneP)
                            posP.x += offNP.x * FXAA_QUALITY__P6;
                        if (!doneP)
                            posP.y += offNP.y * FXAA_QUALITY__P6;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 7)
                        if (doneNP)
                        {
                            if (!doneN)
                                lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                            if (!doneP)
                                lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                            if (!doneN)
                                lumaEndN = lumaEndN - lumaNN * 0.5;
                            if (!doneP)
                                lumaEndP = lumaEndP - lumaNN * 0.5;
                            doneN = abs(lumaEndN) >= gradientScaled;
                            doneP = abs(lumaEndP) >= gradientScaled;
                            if (!doneN)
                                posN.x -= offNP.x * FXAA_QUALITY__P7;
                            if (!doneN)
                                posN.y -= offNP.y * FXAA_QUALITY__P7;
                            doneNP = (!doneN) || (!doneP);
                            if (!doneP)
                                posP.x += offNP.x * FXAA_QUALITY__P7;
                            if (!doneP)
                                posP.y += offNP.y * FXAA_QUALITY__P7;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 8)
                            if (doneNP)
                            {
                                if (!doneN)
                                    lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                                if (!doneP)
                                    lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                                if (!doneN)
                                    lumaEndN = lumaEndN - lumaNN * 0.5;
                                if (!doneP)
                                    lumaEndP = lumaEndP - lumaNN * 0.5;
                                doneN = abs(lumaEndN) >= gradientScaled;
                                doneP = abs(lumaEndP) >= gradientScaled;
                                if (!doneN)
                                    posN.x -= offNP.x * FXAA_QUALITY__P8;
                                if (!doneN)
                                    posN.y -= offNP.y * FXAA_QUALITY__P8;
                                doneNP = (!doneN) || (!doneP);
                                if (!doneP)
                                    posP.x += offNP.x * FXAA_QUALITY__P8;
                                if (!doneP)
                                    posP.y += offNP.y * FXAA_QUALITY__P8;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 9)
                                if (doneNP)
                                {
                                    if (!doneN)
                                        lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                                    if (!doneP)
                                        lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                                    if (!doneN)
                                        lumaEndN = lumaEndN - lumaNN * 0.5;
                                    if (!doneP)
                                        lumaEndP = lumaEndP - lumaNN * 0.5;
                                    doneN = abs(lumaEndN) >= gradientScaled;
                                    doneP = abs(lumaEndP) >= gradientScaled;
                                    if (!doneN)
                                        posN.x -= offNP.x * FXAA_QUALITY__P9;
                                    if (!doneN)
                                        posN.y -= offNP.y * FXAA_QUALITY__P9;
                                    doneNP = (!doneN) || (!doneP);
                                    if (!doneP)
                                        posP.x += offNP.x * FXAA_QUALITY__P9;
                                    if (!doneP)
                                        posP.y += offNP.y * FXAA_QUALITY__P9;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 10)
                                    if (doneNP)
                                    {
                                        if (!doneN)
                                            lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                                        if (!doneP)
                                            lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                                        if (!doneN)
                                            lumaEndN = lumaEndN - lumaNN * 0.5;
                                        if (!doneP)
                                            lumaEndP = lumaEndP - lumaNN * 0.5;
                                        doneN = abs(lumaEndN) >= gradientScaled;
                                        doneP = abs(lumaEndP) >= gradientScaled;
                                        if (!doneN)
                                            posN.x -= offNP.x * FXAA_QUALITY__P10;
                                        if (!doneN)
                                            posN.y -= offNP.y * FXAA_QUALITY__P10;
                                        doneNP = (!doneN) || (!doneP);
                                        if (!doneP)
                                            posP.x += offNP.x * FXAA_QUALITY__P10;
                                        if (!doneP)
                                            posP.y += offNP.y * FXAA_QUALITY__P10;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 11)
                                        if (doneNP)
                                        {
                                            if (!doneN)
                                                lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                                            if (!doneP)
                                                lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                                            if (!doneN)
                                                lumaEndN = lumaEndN - lumaNN * 0.5;
                                            if (!doneP)
                                                lumaEndP = lumaEndP - lumaNN * 0.5;
                                            doneN = abs(lumaEndN) >= gradientScaled;
                                            doneP = abs(lumaEndP) >= gradientScaled;
                                            if (!doneN)
                                                posN.x -= offNP.x * FXAA_QUALITY__P11;
                                            if (!doneN)
                                                posN.y -= offNP.y * FXAA_QUALITY__P11;
                                            doneNP = (!doneN) || (!doneP);
                                            if (!doneP)
                                                posP.x += offNP.x * FXAA_QUALITY__P11;
                                            if (!doneP)
                                                posP.y += offNP.y * FXAA_QUALITY__P11;
/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 12)
                    if(doneNP) {
                        if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                        if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                        if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                        if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                        doneN = abs(lumaEndN) >= gradientScaled;
                        doneP = abs(lumaEndP) >= gradientScaled;
                        if(!doneN) posN.x -= offNP.x * FXAA_QUALITY__P12;
                        if(!doneN) posN.y -= offNP.y * FXAA_QUALITY__P12;
                        doneNP = (!doneN) || (!doneP);
                        if(!doneP) posP.x += offNP.x * FXAA_QUALITY__P12;
                        if(!doneP) posP.y += offNP.y * FXAA_QUALITY__P12;
/*--------------------------------------------------------------------------*/
                    }
#endif
/*--------------------------------------------------------------------------*/
                                        }
#endif
/*--------------------------------------------------------------------------*/
                                    }
#endif
/*--------------------------------------------------------------------------*/
                                }
#endif
/*--------------------------------------------------------------------------*/
                            }
#endif
/*--------------------------------------------------------------------------*/
                        }
#endif
/*--------------------------------------------------------------------------*/
                    }
#endif
/*--------------------------------------------------------------------------*/
                }
#endif
/*--------------------------------------------------------------------------*/
            }
#endif
/*--------------------------------------------------------------------------*/
        }
#endif
/*--------------------------------------------------------------------------*/
    }

    float dstN = posM.x - posN.x;
    float dstP = posP.x - posM.x;
    if (!horzSpan)
        dstN = posM.y - posN.y;
    if (!horzSpan)
        dstP = posP.y - posM.y;

    bool goodSpanN = (lumaEndN < 0.0) != lumaMLTZero;
    float spanLength = (dstP + dstN);
    bool goodSpanP = (lumaEndP < 0.0) != lumaMLTZero;
    float spanLengthRcp = 1.0 / spanLength;

    bool directionN = dstN < dstP;
    float dst = min(dstN, dstP);
    bool goodSpan = directionN ? goodSpanN : goodSpanP;
    float subpixG = subpixF * subpixF;
    float pixelOffset = (dst * (-spanLengthRcp)) + 0.5;
    float subpixH = subpixG * fxaaQualitySubpix;

    float pixelOffsetGood = goodSpan ? pixelOffset : 0.0;
    float pixelOffsetSubpix = max(pixelOffsetGood, subpixH);
    if (!horzSpan)
        posM.x += pixelOffsetSubpix * lengthSign;
    if (horzSpan)
        posM.y += pixelOffsetSubpix * lengthSign;

    return float4(FxaaTexTop(tex, posM).xyz, lumaM_PC);
}

/*=======FXAA3 CONSOLE - PC VERSION=========================================*/

float4 FxaaPixelShader_Console(
    float2 pos, // texture coordinate
    float4 fxaaConsolePosPos, // console only, turn off perspective interpolation
    sampler2D tex, // texture to sample
    float4 fxaaConsoleRcpFrameOpt, // This effects sub-pixel AA quality and inversely sharpness.
    float4 fxaaConsoleRcpFrameOpt2, // only for PC
    float fxaaConsoleEdgeSharpness, // sharpness parameter
    float fxaaConsoleEdgeThreshold, // again sharpness
    float fxaaConsoleEdgeThresholdMin) // Trims the algorithm from processing darks.
{
    // luminosity of surrounding pixels
    float lumaNw = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.xy));
    float lumaSw = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.xw));
    float lumaNe = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.zy));
    float lumaSe = FxaaLuma(FxaaTexTop(tex, fxaaConsolePosPos.zw));
    /*--------------------------------------------------------------------------*/
    float4 rgbyMC = FxaaTexTop(tex, pos.xy);
#if (FXAA_GREEN_AS_LUMA == 0)
        float lumaM = rgbyMC.w;
#else
    float lumaM = rgbyMC.y;
#endif
    // determine maximum and minimum luminosity
    float lumaMaxNwSw = max(lumaNw, lumaSw);
    lumaNe += 1.0 / 384.0;
    float lumaMinNwSw = min(lumaNw, lumaSw);
    float lumaMaxNeSe = max(lumaNe, lumaSe);
    float lumaMinNeSe = min(lumaNe, lumaSe);
    float lumaMax = max(lumaMaxNeSe, lumaMaxNwSw);
    float lumaMin = min(lumaMinNeSe, lumaMinNwSw);
    float lumaMaxScaled = lumaMax * fxaaConsoleEdgeThreshold;
    float lumaMinM = min(lumaMin, lumaM);
    float lumaMaxScaledClamped = max(fxaaConsoleEdgeThresholdMin, lumaMaxScaled);
    float lumaMaxM = max(lumaMax, lumaM);
    // determine directional luminosity
    float dirSwMinusNe = lumaSw - lumaNe;
    float lumaMaxSubMinM = lumaMaxM - lumaMinM;
    float dirSeMinusNw = lumaSe - lumaNw;
    if (lumaMaxSubMinM < lumaMaxScaledClamped)
        return rgbyMC;
    /*--------------------------------------------------------------------------*/
    float2 dir;
    dir.x = dirSwMinusNe + dirSeMinusNw;
    dir.y = dirSwMinusNe - dirSeMinusNw;
    /*--------------------------------------------------------------------------*/
    float2 dir1 = normalize(dir.xy);
    float4 rgbyN1 = FxaaTexTop(tex, pos.xy - dir1 * fxaaConsoleRcpFrameOpt.zw);
    float4 rgbyP1 = FxaaTexTop(tex, pos.xy + dir1 * fxaaConsoleRcpFrameOpt.zw);
    /*--------------------------------------------------------------------------*/
    float dirAbsMinTimesC = min(abs(dir1.x), abs(dir1.y)) * fxaaConsoleEdgeSharpness;
    float2 dir2 = clamp(dir1.xy / dirAbsMinTimesC, -2.0, 2.0);
    /*--------------------------------------------------------------------------*/
    float4 rgbyN2 = FxaaTexTop(tex, pos.xy - dir2 * fxaaConsoleRcpFrameOpt2.zw);
    float4 rgbyP2 = FxaaTexTop(tex, pos.xy + dir2 * fxaaConsoleRcpFrameOpt2.zw);
    /*--------------------------------------------------------------------------*/
    float4 rgbyA = rgbyN1 + rgbyP1;
    float4 rgbyB = ((rgbyN2 + rgbyP2) * 0.25) + (rgbyA * 0.25);
    /*--------------------------------------------------------------------------*/
#if (FXAA_GREEN_AS_LUMA == 0)
        bool twoTap = (rgbyB.w < lumaMin) || (rgbyB.w > lumaMax);
#else
    bool twoTap = (rgbyB.y < lumaMin) || (rgbyB.y > lumaMax);
#endif
    if (twoTap)
        rgbyB.xyz = rgbyA.xyz * 0.5;
    return rgbyB;
}

/*=======PIXELSHADER========================================================*/

float4 PixelShaderFunction_PC(float4 position : SV_Position, float4 color : COLOR0, float2 texCoords : TEXCOORD0) : SV_Target0
{
    float4 value = FxaaPixelShader_PC(
        texCoords,
        splScreen,
        float2(invViewportWidth, invViewportHeight),
        fxaaQualitySubpix,
        fxaaQualityEdgeThreshold,
        fxaaQualityEdgeThresholdMin
        );

    return value;
}

float4 PixelShaderFunction_Console(float4 position : SV_Position, float4 color : COLOR0, float2 texCoords : TEXCOORD0) : SV_Target0
{
    // upper left xy and lower right zw of pixel
    float4 pixelBorder = float4(texCoords.x, texCoords.y, texCoords.x + invViewportWidth, texCoords.y + invViewportHeight);
    
    float4 value = FxaaPixelShader_Console(
		texCoords,
		pixelBorder,
		splScreen,
		ConsoleOpt1,
		ConsoleOpt2,
		ConsoleEdgeSharpness,
		ConsoleEdgeThreshold,
		ConsoleEdgeThresholdMin
		);

    return value;
}

/*=======TECHNIQUES=========================================================*/

technique ppfxaa_PC
{
    pass Pass1
    {
#if SM4
		PixelShader = compile ps_5_0 PixelShaderFunction_PC();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction_Console();
#endif
    }
}

technique ppfxaa_Console
{
    pass Pass1
    {
#if SM4
		PixelShader = compile ps_5_0 PixelShaderFunction_Console();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction_Console();
#endif
    }
}