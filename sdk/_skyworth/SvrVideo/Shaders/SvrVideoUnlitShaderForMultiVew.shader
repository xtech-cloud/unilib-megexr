// Copyright 2018 Skyworth VR. All rights reserved.
Shader "Svr/SvrVideoUnlitShaderForMultiView" {
	Properties{
		_Gamma("Video gamma", Range(0.01,3.0)) = 1.0
		_MainTex("Base (RGB)", 2D) = "white" {}
		[KeywordEnum(None, TopBottom, LeftRight)] _StereoMode("Stereo mode", Float) = 0
		[Toggle(FLIP_X)] _FlipX("Flip X", Float) = 0
	}

	SubShader {
		Pass {
			Tags{ "RenderType" = "Opaque" }

			Lighting Off
			Cull Off

		GLSLPROGRAM
		#pragma only_renderers gles gles3
		#extension GL_OVR_multiview2 : require
		#extension GL_OES_EGL_image_external : require
		#extension GL_OES_EGL_image_external_essl3 : enable

		#pragma multi_compile ___ _STEREOMODE_TOPBOTTOM _STEREOMODE_LEFTRIGHT
		#pragma multi_compile ___ FLIP_X

		precision mediump int;
		precision mediump float;

#ifdef VERTEX
		uniform mat4 video_matrix;
		uniform vec4 hlslcc_mtx4x4unity_ObjectToWorld[4];
		layout(std140) uniform UnityStereoGlobals{
		vec4 hlslcc_mtx4x4unity_StereoMatrixP[8];
		vec4 hlslcc_mtx4x4unity_StereoMatrixV[8];
		vec4 hlslcc_mtx4x4unity_StereoMatrixInvV[8];
		vec4 hlslcc_mtx4x4unity_StereoMatrixVP[8];
		vec4 hlslcc_mtx4x4unity_StereoCameraProjection[8];
		vec4 hlslcc_mtx4x4unity_StereoCameraInvProjection[8];
		vec4 hlslcc_mtx4x4unity_StereoWorldToCamera[8];
		vec4 hlslcc_mtx4x4unity_StereoCameraToWorld[8];
		vec3 unity_StereoWorldSpaceCameraPos[2];
		vec4 unity_StereoScaleOffset[2];
		};
		layout(num_views = 2) in;
		in highp vec4 in_POSITION0;
		in highp vec2 in_TEXCOORD0;
		out highp vec2 vs_TEXCOORD0;
		vec4 u_xlat0;
		int u_xlati1;
		vec4 u_xlat2;
		//varying vec2 uv;

		void main()
		{
			vec4 bendVertex = in_POSITION0;

			u_xlat0 = bendVertex.yyyy * hlslcc_mtx4x4unity_ObjectToWorld[1];
			u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[0] * bendVertex.xxxx + u_xlat0;
			u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[2] * bendVertex.zzzz + u_xlat0;
			u_xlat0 = u_xlat0 + hlslcc_mtx4x4unity_ObjectToWorld[3];
			u_xlati1 = int(gl_ViewID_OVR) << 2;
			u_xlat2 = u_xlat0.yyyy * hlslcc_mtx4x4unity_StereoMatrixVP[(u_xlati1 + 1)];
			u_xlat2 = hlslcc_mtx4x4unity_StereoMatrixVP[u_xlati1] * u_xlat0.xxxx + u_xlat2;
			u_xlat2 = hlslcc_mtx4x4unity_StereoMatrixVP[(u_xlati1 + 2)] * u_xlat0.zzzz + u_xlat2;
			gl_Position = hlslcc_mtx4x4unity_StereoMatrixVP[(u_xlati1 + 3)] * u_xlat0.wwww + u_xlat2;

			vec4 untransformedUV = gl_MultiTexCoord0;
#ifdef FLIP_X
			untransformedUV.x = 1.0 - untransformedUV.x;
#endif  // FLIP_X
#ifdef _STEREOMODE_TOPBOTTOM
			untransformedUV.y *= 0.5;
			if (u_xlati1 == 0) {
				untransformedUV.y += 0.5;
			}
#endif  // _STEREOMODE_TOPBOTTOM
#ifdef _STEREOMODE_LEFTRIGHT
			untransformedUV.x *= 0.5;
			if (u_xlati1 != 0) {
				untransformedUV.x += 0.5;
			}
#endif  // _STEREOMODE_LEFTRIGHT
			vs_TEXCOORD0 = (video_matrix * untransformedUV).xy;
		}
#endif  // VERTEX

	#ifdef FRAGMENT
		#version 300 es

		precision highp int;
		uniform lowp samplerExternalOES _MainTex;
		uniform float _Gamma;
		in highp vec2 vs_TEXCOORD0;
		layout(location = 0) out mediump vec4 SV_Target0;
		lowp vec4 u_xlat10_0;

		vec3 gammaCorrect(vec3 v, float gamma) {
			return pow(v, vec3(1.0 / gamma));
		}

		// Apply the gamma correction.  One possible optimization that could
		// be applied is if _Gamma == 2.0, then use gammaCorrectApprox since sqrt will be faster.
		// Also, if _Gamma == 1.0, then there is no effect, so this call could be skipped all together.
		vec4 gammaCorrect(vec4 v, float gamma) {
			return vec4(gammaCorrect(v.xyz, gamma), v.w);
		}

		void main()
		{
			u_xlat10_0 = gammaCorrect(texture(_MainTex, vs_TEXCOORD0.xy), _Gamma);
			SV_Target0 = u_xlat10_0;
			return;
		}
	#endif  // FRAGMENT
		ENDGLSL
		}
	}
	Fallback "Unlit/Texture"
}
