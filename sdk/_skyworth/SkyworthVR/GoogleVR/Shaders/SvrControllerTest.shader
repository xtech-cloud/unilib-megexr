Shader "Svr/SvrControllerTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#define BATTERY_FULL = 0;
			#define BATTERY_ALMOST_FULL = .205f;
			#define BATTERY_MEDIUM = .3295f;
			#define BATTERY_LOW = .455f;
			#define BATTERY_CRITICAL = .605f;
			#define BATTERY_HIDDEN = .765f;
			// How opaque is the battery indicator when illuminated
#define _GVR_BATTERY_ACTIVE_ALPHA 0.9

			//How opaque is the battery indicator when not illuminated
#define _GVR_BATTERY_OFF_ALPHA 0.25

#define TRIGGER float4(1,1,0,1);
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half4 vertex_color : COLOR0;
				float4 nomorColor: COLOR1;
				half alpha : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex_color = v.color;
				/*o.color = half4(1, 0, 0, 1);*/
				//if (o.uv.x > 0.357 && o.uv.x < 0.466 && o.uv.y > 0.885 && o.uv.y < 0.995)
				//{
				//	o.vertex.y += 0.001;//Home
				//}
				/*if (v.color.a >= .25f)
				{
					o.vertex.x += 0.04f;
				}*/
				//o.color = v.color;
				o.nomorColor = float4(v.normal, 1);

				half batteryOrController = saturate(10.0 * (v.color.a - 0.6));
				/*half batteryMask = saturate(10.0 * (1 - v.color.a));
				half batteryLevelMask = saturate(20.0 * (v.color.a - 0));*/
				o.alpha = batteryOrController;
				/*o.color.a = 1 * batteryMask * (batteryLevelMask * _GVR_BATTERY_ACTIVE_ALPHA + (1 - batteryLevelMask)*_GVR_BATTERY_OFF_ALPHA);
				o.color.rgb = batteryMask * (batteryLevelMask * fixed3(1,0,0));*/


				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 batterycolor = 1;
				if (i.vertex_color.a >= .3295f)
				{
					batterycolor.a = 0;
				}
				return col * batterycolor;
				//if (i.uv.x > 0.357 && i.uv.x < 0.466 && i.uv.y > 0.885 && i.uv.y < 0.995 )
				//{
				//	col *= fixed4(1, 0, 0, 1);//Home
				//}
				//return fixed4(i.uv.x,i.uv.y,0,1);
				//return i.color;
				//显示顶点颜色
				/*fixed4 alphcolor = fixed4(1-i.color.a, 1-i.color.a, 1-i.color.a,1);
				if (i.color.r != 1 && i.color.g != 1 && i.color.b != 1)
					return i.color+alphcolor;
				else
					return i.color;*/

			//if (i.color.a == .525f)
			
			
			/*fixed test = 0;
			fixed4 testcolor = 0;
			if (i.color.a <= .525f)
			{
				test = 1;
				testcolor = fixed4(0, 0, test, 1);
				half3 vectos = col.rgb;
				if (length(vectos) < 1.0f)
				{
					col.a = 0;
				}
			}*/
			//if (i.color.a >= .425f && i.color.a < .525f)
			//{
			//	testcolor = fixed4(0,1,0,1);
			//}
			//else
				//testcolor = i.color;
			//return col + testcolor;
			//i.color.a = 0;
			//col.a = i.alpha;
			//return col;
			/*if (i.color.a <= .525f)
			{
				return i.nomorColor;
			}
			else
			{
				return col;
			}*/
				//显示trigger
			/*if (i.color.r == 1 && i.color.g == 1)
				return fixed4(1, 0, 0, 1);
			else
				return fixed4(0, 0, 0, 1);*/
			}
			ENDCG
		}
	}
}
