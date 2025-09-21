Shader "Custom/Red2"
{
    Properties
    {
        _CellMin ("Cell Min (UV)", Vector) = (0.3, 0.3, 0, 0)
        _CellMax ("Cell Max (UV)", Vector) = (0.4, 0.4, 0, 0)
        _HighlightColor ("Highlight Color", Color) = (1, 0, 0, 1) // Red
        _Offset ("Offset (UV)", Vector) = (0, 0, 0, 0) // Public offset to move the highlighted cell
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float2 _CellMin;
            float2 _CellMax;
            float4 _HighlightColor;
            float2 _Offset; // New offset for the UV coordinates

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Apply the offset to the UV coordinates with proper adjustments for X and Y
                float2 uvWithOffset = i.uv + _Offset;

                // Check if the UV is inside the selected grid cell with offset applied
                if (uvWithOffset.x >= _CellMin.x && uvWithOffset.x <= _CellMax.x &&
                    uvWithOffset.y >= _CellMin.y && uvWithOffset.y <= _CellMax.y)
                {
                    return _HighlightColor; // Red color with transparency blending
                }
                
                // Fully transparent elsewhere, but blending is enabled
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
