Shader "2DVLS/Light" {
    Properties {
        _MainTex ("SelfIllum Color (RGB) Alpha (A)", 2D) = "white"
    }
    Category {
	
       Lighting Off
       ZWrite Off
       
	   //Tags { "Queue" = "Geometry-10" }

       Blend One One
       
	   BindChannels {
              Bind "Color", color
              Bind "Vertex", vertex
              Bind "TexCoord", texcoord
       }
       
	   SubShader 
	   {
            Pass 
			{
               SetTexture [_MainTex] 
			   {
                    Combine texture * primary
               }
            }
        } 
    }
}