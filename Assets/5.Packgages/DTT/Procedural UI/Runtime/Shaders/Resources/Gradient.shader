Shader "UI/RoundedCorners/Gradient" 
{
	Properties 
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" { }
		[HideInInspector] _GradientTex ("Texture", 2D) = "white" { }
		[HideInInspector] _Type ("Type", int) = 0

		// Mask support.
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Int) = 8
		[EightBit] _Stencil("Stencil ID", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Operation", Int) = 0
		[EightBit] _StencilReadMask("Stencil Read Mask", Int) = 255
		[EightBit] _StencilWriteMask("Stencil Write Mask", Int) = 255
		[Enum(None, 0, Alpha, 1, Red, 8, Green, 4, Blue, 2, RGB, 14, RGBA, 15)] _ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		[HideInInspector] _MaskTex("MaskTexture", 2D) = "white"{}
	}
	
	SubShader 
	{
		Tags 
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

		// Mask support.
		Stencil 
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		Cull Off
		Lighting Off
		ZTest [unity_GUIZTestMode]
		ColorMask [_ColorMask]

		// Standard blending.
		Blend SrcAlpha OneMinusSrcAlpha

		// No zwrite.
		ZWrite Off 

		Pass 
		{
			CGPROGRAM
			
			// Include setup file.
			#include "ShaderSetup.cginc"
			#include "ShaderGradient.cginc"
			
			#pragma vertex vert
			#pragma fragment frag

			// Main texture to sample.
			sampler2D _MainTex;
			uint _Type;
			sampler2D _GradientTex;
			float4 _GradientTex_ST;

			// Fragment shader used to determine the gradient.
			fixed4 frag(v2f i) : SV_Target {
				// Mix gradient with color.
				
				float2 gradientUV = i.uv1.xy;
				return Gradient(_GradientTex, gradientUV, _GradientTex_ST, _Type) * i.color * tex2D(_MainTex,i.uv);
			}
			
			ENDCG
		}
	}
	CustomEditor "DTT.UI.ProceduralUI.Editor.RoundedCornersShaderEditor"
}
