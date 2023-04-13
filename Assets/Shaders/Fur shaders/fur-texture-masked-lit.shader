Shader "Custom/fur-texture-masked-lit" {
  Properties {
    _BaseMap ("Base Map", 2D) = "white" {}
    _MaskMap ("Mask Map", 2D) = "white" {}
    _FurMap ("Fur Map", 2D) = "white" {}
    [Normal] _NormalMap("Normal", 2D) = "bump" {}
    _NormalScale("Normal Scale", Range(0.0, 2.0)) = 1.0
    [Normal] _FurNormals("Fur Normals", 2D) = "bump" {}
    _FurNormalScale("Normal Scale", Range(0.0, 2.0)) = 1.0
    [IntRange] _ShellCount ("Shell Count", Range(1, 14)) = 14
    _FurHeight ("Shell Max Height", Range(0.0, 1.0)) = 0.016
    _AlphaThreshold ("Alpha Threshold", Range(0.0, 1.0)) = 0.1
    _Occlusion ("Occlusion", Range(0.0, 1.0)) = 0.1
    _FurScale ("Fur Scale", Range(0.001, 10)) = 1.0
    _BaseMove ("Base Move", Vector) = (0.0, -0.0, 0.0, 3.0)
    _WindFreq ("Wind Freq", Vector) = (0.5, 0.7, 0.9, 1.0)
    _WindMove ("Wind Move", Vector) = (0.2, 0.3, 0.2, 1.0)

    _ColorMain ("Main Swatch", Color) = (1, 1, 1, 1)
    _ColorHigh ("Highlight Swatch", Color) = (1, 1, 1, 1)
    _ColorDarker ("Darker Swatch", Color) = (1, 1, 1, 1)
    _ColorDarkest ("Darkest Swatch", Color) = (1, 1, 1, 1)
    
    _RimLightPower ("Rim Light Power", Range(0.0, 20.0)) = 6.0
    _RimLightIntensity ("Rim Light Intensity", Range(0.0, 1.0)) = 0.5

    _BlockFresnelColor ("Block Fresnel Color", Color) = (1, 1, 1, 1)
    _BlockFresnelPower ("Block Fresnel Power", Range(0.0, 20.0)) = 6
    _BlockFresnelVis ("Block Fresnel Visibility", Range(0, 1)) = 1
    
    _ShadowExtraBias ("Shadow Extra Bias", Range(-50.0, 50.0)) = 0.0
  }
  SubShader {
    Tags { 
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
      "UniversalMaterialType" = "Lit"
      "IgnoreProjector" = "True"
    }

    LOD 100

    ZWrite On
    Cull Back

    Pass {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM

      #if (UNITY_VERSION >= 202111)
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
      #else
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
      #endif
      #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
      #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
      #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
      #pragma multi_compile _ _SHADOWS_SOFT
      #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
      #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

      #pragma multi_compile _ DIRLIGHTMAP_COMBINED
      #pragma multi_compile _ LIGHTMAP_ON
      #pragma multi_compile_fog

      #pragma prefer_hlslcc gles
      #pragma exclude_renderers d3d11_9x
      #pragma target 2.0

      #include "./fur-texture-masked-lit.hlsl"

      #pragma vertex vert
      #pragma require geometry
      #pragma geometry geo
      #pragma fragment frag
      ENDHLSL
    }

    Pass {
      Name "DepthOnly"
      Tags {"LightMode" = "DepthOnly"}

      ZWrite On
      ColorMask 0

      HLSLPROGRAM
      #pragma exclude_renderers gles gles3 glcore
      #include "./fur-depth.hlsl"
      #pragma vertex vert
      #pragma require geometry
      #pragma geometry geo
      #pragma fragment frag
      ENDHLSL
    }

    Pass {
      Name "ShadowCaster"
      Tags {"LightMode" = "ShadowCaster"}

      ZWrite On
      ZTest LEqual
      ColorMask 0

      HLSLPROGRAM
      #pragma exclude_renderers gles gles3 glcore
      // #include "./fur-shadow.hlsl"
      // #pragma vertex vert
      // #pragma require geometry
      // #pragma geometry geo
      // #pragma fragment frag
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
      #include "./fur-shadow-alt.hlsl"
      ENDHLSL
    }
  }
}
