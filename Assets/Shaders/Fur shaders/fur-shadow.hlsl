#ifndef FUR_SHADOW_HLSL
#define FUR_SHADOW_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

int _ShellCount;
float _FurHeight;
float _AlphaThreshold;
float _Occlusion;
float _FurScale;

float4 _BaseMove;
float4 _WindFreq;
float4 _WindMove;

TEXTURE2D(_FurMap);
SAMPLER(sampler_FurMap);
float4 _FurMap_ST;

float3 _LightPosition;
float3 _LightDirection;
float _ShadowExtraBias;

struct attr {
  float4 positionOS : POSITION;
  float3 normalOS   : NORMAL;
  float4 tangentOS  : TANGENT;
  float2 uv         : TEXCOORD0;
};

struct v2f {
  float4 vertex  : SV_POSITION;
  float2 uv      : TEXCOORD0;
  float fogCoord : TEXCOORD1;
  float layer    : TEXCOORD2;
};

attr vert(attr input)
{
    return input;
}

inline float3 CustomApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    positionWS += lightDirection * (_ShadowBias.x + _ShadowExtraBias);
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;
    positionWS += normalWS * scale.xxx;

    return positionWS;
}

inline float4 GetShadowPositionHClip(float3 positionWS, float3 normalWS)
{

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif
    return positionCS;
}

void AppendShellVertex(inout TriangleStream<v2f> stream, attr input, int index)
{
    v2f output = (v2f)0;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    float layer = (float)index / _ShellCount;

    float moveFactor = pow(abs(layer), _BaseMove.w);
    float3 posOS = input.positionOS.xyz;
    float3 windAngle = _Time.w * _WindFreq.xyz;
    float3 windMove = moveFactor * _WindMove.xyz * sin(windAngle + posOS * _WindMove.w);
    float3 move = moveFactor * _BaseMove.xyz;

    float3 shellDir = normalize(normalInput.normalWS + move + windMove);
    float3 posWS = vertexInput.positionWS + shellDir * (_FurHeight * layer);
    float4 posCS = GetShadowPositionHClip(posWS, normalInput.normalWS);
    
    output.vertex = posCS;
    output.uv = TRANSFORM_TEX(input.uv, _FurMap);
    output.fogCoord = ComputeFogFactor(posCS.z);
    output.layer = layer;

    stream.Append(output);
}

[maxvertexcount(128)]
void geo(triangle attr input[3], inout TriangleStream<v2f> stream)
{
    [loop] for (float i = 0; i < _ShellCount; ++i)
    {
        [unroll] for (float j = 0; j < 3; ++j)
        {
            AppendShellVertex(stream, input[j], i);
        }
        stream.RestartStrip();
    }
}

void frag(
    v2f input, 
    out float4 outColor : SV_Target, 
    out float outDepth : SV_Depth)
{
    float4 furColor = SAMPLE_TEXTURE2D(_FurMap, sampler_FurMap, input.uv / _FurMap_ST.xy * _FurScale);
    float alpha = furColor.r * (1.0 - input.layer);
    if (input.layer > 0.0 && alpha < _AlphaThreshold) discard;

    outColor = outDepth = input.vertex.z / input.vertex.w;
}

#endif