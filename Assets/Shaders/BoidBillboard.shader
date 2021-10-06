Shader "Unlit/BoidBillboard"
{
    Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        _Freq("Frequency",Range(0,1)) = 0
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
                float uvFreq : TEXCOORD1;
				float4 pos : SV_POSITION;
			};

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Freq)
            UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;

            fixed4 _Color;
            uniform float _MusicBeat;
            uniform sampler2D _MusicSpectrumTex;
			
			v2f vert (appdata v)
			{
				v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
                o.uvFreq = UNITY_ACCESS_INSTANCED_PROP(Props, _Freq);

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);

				o.pos = outPos;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                float2 mask = tex2D(_MainTex, i.uv);
				// sample the texture
				fixed4 col = _Color;
                col.a = mask.g;

                float time = tex2D(_MusicSpectrumTex, i.uvFreq)+0.5f;

                col.a *= step(mask.r,time);
                
				return col;
			}
			ENDCG
		}
	}
}