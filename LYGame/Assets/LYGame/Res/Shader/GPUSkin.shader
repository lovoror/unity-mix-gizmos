Shader "LYGame/GPUSkin"
{
	Properties
	{
		_Dynamic("Dynamic", Vector) = (1.0,1.0,1.0,1.0)
		_MainTex("Texture", 2D) = "white" {}
		_AnimationMap("AnimationMap", 2D) = "white" {}
		_TotalTime("TotalTime", Float) = 0
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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Dynamic)
			UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _AnimationMap;
			float4 _AnimationMap_TexelSize; // 此值的两个分量分别为1 / width和1 / height

			float _TotalTime;
			
			v2f vert (appdata v, uint vid : SV_VertexID)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				// 四个分量，x为起始帧，y为终止帧，z为起始时间偏移，w为倍速
				float4 dynamic = UNITY_ACCESS_INSTANCED_PROP(Props, _Dynamic);

				// 计算纹理采样的位置
				float section_frame = (dynamic.y - dynamic.x + 1);
				float elapse_time = _Time.y * dynamic.w;
				float elapse_frame = dynamic.z + elapse_time / (_TotalTime * _AnimationMap_TexelSize.y);
				int final_elapse_frame = fmod(elapse_frame, section_frame);

				// 采样获得顶点位置
				float map_x = (vid + 0.5) * _AnimationMap_TexelSize.x;
				float map_y = (dynamic.x + final_elapse_frame + 0.5) * _AnimationMap_TexelSize.y;
				float4 pos = tex2Dlod(_AnimationMap, float4(map_x, map_y, 0, 0));

				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex = UnityObjectToClipPos(pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}

			ENDCG
		}
	}
}
