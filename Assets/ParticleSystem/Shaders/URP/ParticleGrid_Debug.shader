Shader "Custom/ParticleGrid_Debug"
{
    Properties {
        _BaseColor("Color", Color) = (0,1,0,0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<uint> gridIDs;
            int debugItem;
            float4 _BaseColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                uint instanceID : SV_InstanceID;
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.instanceID = v.instanceID;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.instanceID == gridIDs[debugItem]
                    ? _BaseColor * 10
                    : float4(1,1,1,0.2);
            }
            ENDCG
        }
    }
}
