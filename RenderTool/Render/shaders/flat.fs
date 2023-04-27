
#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// material parameters
uniform sampler2D albedoMap;


// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];
uniform float lightPow[4];
uniform float lightAtten[4];
uniform bool useAlpha;


uniform vec3 camPos;

const float PI = 3.14159265359;

void main()
{
    FragColor =  texture(albedoMap, TexCoords);
    if ( useAlpha && FragColor.r > 0.9 && FragColor.g > 0.9  && FragColor.b > 0.9){ discard; }
}



