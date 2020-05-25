Shader "PromiseCode/MapBorder"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MapSize("Map Size", Range(16, 512)) = 256
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200
        ZTest Always

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        #pragma target 3.0

        sampler2D _MainTex;
        float _MapSize;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 screenPos;
        };

        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)

        UNITY_INSTANCING_BUFFER_END(Props)

		float random(in float2 st)
		{
			return frac(sin(dot(st.xy,
				float2(12.9898, 78.233)))
				* 43758.5453123);
		}

		float noise(in float2 st)
		{
			float2 i = floor(st);
			float2 f = frac(st);

			float a = random(i);
			float b = random(i + float2(1.0, 0.0));
			float c = random(i + float2(0.0, 1.0));
			float d = random(i + float2(1.0, 1.0));

			float2 u = f * f*(3.0 - 2.0*f);

			return lerp(a, b, u.x) +
				(c - a)* u.y * (1.0 - u.x) +
				(d - b) * u.x * u.y;
		}

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float mapSize = _MapSize;
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            float3 pos = IN.worldPos;

            float distanceFromBorderX = 0;
            float distanceFromBorderZ = 0;

            if(pos.x <= 0)
                distanceFromBorderX = abs(pos.x);
            else if(pos.x >= mapSize)
                distanceFromBorderX = abs(mapSize - pos.x);

            if(pos.z <= 0)
                distanceFromBorderZ = abs(pos.z);
            else if(pos.z >= mapSize)
                distanceFromBorderZ = abs(mapSize - pos.z);

            float alpha = 0;

            if(distanceFromBorderX > 0 && alpha == 0)
                alpha = lerp(0, 1, distanceFromBorderX / 3);

            if(distanceFromBorderZ > 0)
            {
                float newAlpha = lerp(0, 1, distanceFromBorderZ / 3);
                if(newAlpha > alpha)
                    alpha = newAlpha;
            }

            float2 worldUV = IN.worldPos.xz + _Time.y * 1;
            float xCoordUnscaled = noise(worldUV * 0.1);

            alpha = clamp(alpha, 0, 1);
            c.a = alpha;
            c.rgb = xCoordUnscaled * 0.03;

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
