Shader "LYGame/Baked"
{
	Properties
	{
		_Tint ("Tint (RGB)", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		[Enum(LessEqual,4,Always,8,NotEqual,6)] _ZTest("ZTest", float) = 4
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"IgnoreProjector"="True"
			"ForceNoShadowCasting"="True"
		}
		LOD 100

		Pass
		{
			ZTest [_ZTest]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Tint;
				col.a = 1;
				return col;
			}

			ENDCG
		}
	}
}
