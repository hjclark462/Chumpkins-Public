#ifndef FUR_LIT_HLSL
#define FUR_LIT_HLSL

#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

int _ShellCount;
float _FurHeight;
float _AlphaThreshold;
float _Occlusion;
float _FurScale;

float4 _BaseMove;
float4 _WindFreq;
float4 _WindMove;

float _RimLightPower;
float _RimLightIntensity;

TEXTURE2D(_FurMap);
SAMPLER(sampler_FurMap);
float4 _FurMap_ST;

TEXTURE2D(_NormalMap); 
SAMPLER(sampler_NormalMap);
float4 _NormalMap_ST;
float _NormalScale;

struct attr {
  float4 positionOS : POSITION;
  float3 normalOS   : NORMAL;
  float4 tangentOS  : TANGENT;
  float2 uv         : TEXCOORD0;
  float2 lightmapUV : TEXCOORD1;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
  float4 positionCS              : SV_POSITION;
  float3 positionWS              : TEXCOORD0;
  float3 normalWS                : TEXCOORD1;
  float3 tangentWS               : TEXCOORD2;
  float2 uv                      : TEXCOORD4;
  DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
  float4 fogFactorAndVertexLight : TEXCOORD6; // x: fogFactor, yzw: vertex light
  float layer                    : TEXCOORD7;
};

float softlight(float a, float b, float opacity) {
  float localA = a;
  float localB = lerp(a, b, opacity);

  return ((1-(2*localB))*localA*localA) + (2*localB*localA);
}

float3 softlight(float3 a, float3 b, float opacity) {
  return float3(softlight(a.x, b.x, opacity), softlight(a.y, b.y, opacity), softlight(a.z, b.z, opacity));
}

void ApplyRimLight(inout float3 color, float3 posWS, float3 viewDirWS, float3 normalWS) {
  float viewDotNormal = abs(dot(viewDirWS, normalWS));
  float normalFactor = pow(abs(1.0 - viewDotNormal), _RimLightPower);

  Light light = GetMainLight();
  float lightDirDotView = dot(light.direction, viewDirWS);
  float intensity = pow(max(-lightDirDotView, 0.0), _RimLightPower);
  intensity *= _RimLightIntensity * normalFactor;
  #ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord = TransformWorldToShadowCoord(posWS);
    intensity *= MainLightRealtimeShadow(shadowCoord);
  #endif
  color += ((1-(1.0/pow(color + 1, 8))) * 1.00392156863) * intensity * light.color;

  #ifdef _ADDITIONAL_LIGHTS
    int additionalLightsCount = GetAdditionalLightsCount();
    for (int i = 0; i < additionalLightsCount; ++i) {
      int index = GetPerObjectLightIndex(i);
      Light light = GetAdditionalPerObjectLight(index, posWS);
      float lightDirDotView = dot(light.direction, viewDirWS);
      float intensity = max(-lightDirDotView, 0.0);
      intensity *= _RimLightIntensity * normalFactor;
      intensity *= light.distanceAttenuation;
      #ifdef _MAIN_LIGHT_SHADOWS
        intensity *= AdditionalLightRealtimeShadow(index, posWS);
      #endif
      color += ((1-(1.0/pow(color + 1, 8))) * 1.00392156863) * intensity * light.color;
    }
  #endif
}

attr vert(attr input) {
  return input;
}

void AppendShellVertex(inout TriangleStream<v2f> stream, attr input, int ind) {
  v2f output = (v2f)0;

  VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
  VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

  float layer = (float)ind / _ShellCount;
  float3 posOS = input.positionOS.xyz;
  
  float moveFactor = pow(layer, _BaseMove.w);

  float3 windAngle = _Time.w * _WindFreq.xyz;
  float3 windMove = moveFactor * _WindMove.xyz * sin(windAngle + posOS * _WindMove.w);
  float3 move = moveFactor * _BaseMove.xyz;
  float3 shellDir = SafeNormalize(normalInput.normalWS + move + windMove);

  output.positionWS = vertexInput.positionWS + shellDir * (_FurHeight * layer);
  output.positionCS = TransformWorldToHClip(output.positionWS);
  output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
  output.normalWS = TransformObjectToWorldNormal(input.normalOS);
  output.tangentWS = normalInput.tangentWS;
  output.layer = layer;

  float3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
  float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
  output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

  OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
  OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

  stream.Append(output);
}

[maxvertexcount(42)]
void geo(triangle attr input[3], inout TriangleStream<v2f> stream) {
  [loop] for (float i = 0; i < _ShellCount; ++i) {
    [unroll] for (float j = 0; j < 3; ++j) {
      AppendShellVertex(stream, input[j], i);
    }
    stream.RestartStrip();
  }
}

float3 TransformHClipToWorld(float4 positionCS) {
  return mul(UNITY_MATRIX_I_VP, positionCS).xyz;
}

float4 frag(v2f input) : SV_Target {
  float2 furUV = input.uv / _FurMap_ST.xy * _FurScale;
  float4 furColor = SAMPLE_TEXTURE2D(_FurMap, sampler_FurMap, furUV);
  float alpha = furColor.r * (1.0 - input.layer);
  if (input.layer > 0.0 && alpha < _AlphaThreshold) discard;

  float3 viewDirWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);
  float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, furUV), _NormalScale);
  float3 bitangent = SafeNormalize(-cross(input.normalWS, input.tangentWS));
  float3 normalWS = SafeNormalize(TransformTangentToWorld(normalTS, float3x3(input.tangentWS, bitangent, input.normalWS)));

  SurfaceData surfaceData = (SurfaceData)0;
  InitializeStandardLitSurfaceData(input.uv, surfaceData);
  surfaceData.occlusion = lerp(1.0 - _Occlusion, 1.0, input.layer);

  InputData inputData = (InputData)0;
  inputData.positionWS = input.positionWS;
  inputData.normalWS = normalWS;
  inputData.viewDirectionWS = viewDirWS;
  #if (defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
  #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
  #endif
  inputData.fogCoord = input.fogFactorAndVertexLight.x;
  inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
  inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);

  float4 color = UniversalFragmentPBR(inputData, surfaceData);
  ApplyRimLight(color.rgb, input.positionWS, viewDirWS, input.normalWS);
  color.rgb = MixFog(color.rgb, inputData.fogCoord);

  return color;
}

#endif