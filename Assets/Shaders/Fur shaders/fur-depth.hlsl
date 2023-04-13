#ifndef FUR_DEPTH_HLSL
#define FUR_DEPTH_HLSL

#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"

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

attr vert(attr i)
{
    return i;
}

void AppendShellVertex(inout TriangleStream<v2f> stream, attr i, int index)
{
    v2f output = (v2f)0;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(i.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

    float layer = (float)index / _ShellCount;

    float moveFactor = pow(abs(layer), _BaseMove.w);
    float3 posOS = i.positionOS.xyz;
    float3 windAngle = _Time.w * _WindFreq.xyz;
    float3 windMove = moveFactor * _WindMove.xyz * sin(windAngle + posOS * _WindMove.w);
    float3 move = moveFactor * _BaseMove.xyz;

    float3 shellDir = normalize(normalInput.normalWS + move + windMove);
    float3 posWS = vertexInput.positionWS + shellDir * (_FurHeight * layer);
    float4 posCS = TransformWorldToHClip(posWS);
    
    output.vertex = posCS;
    output.uv = TRANSFORM_TEX(i.uv, _BaseMap);
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