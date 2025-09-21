Shader "Custom/GridHighlightShader"
{
    Properties
    {
        _Center ("Circle Center (Object Space)", Vector) = (0, 0, 0, 0)
        _Radius ("Circle Radius", Float) = 0.01
        _OutlineThickness ("Outline Thickness", Float) = 0.002
        _HighlightColor ("Highlight Color", Color) = (1, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objPos : TEXCOORD0;
            };

            float3 _Center;
            float _Radius;
            float _OutlineThickness;
            float4 _HighlightColor;
            float4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.objPos.xy, _Center.xy);

                if (dist <= _Radius + _OutlineThickness && dist > _Radius)
                    return _OutlineColor;
                else if (dist <= _Radius)
                    return _HighlightColor;

                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
