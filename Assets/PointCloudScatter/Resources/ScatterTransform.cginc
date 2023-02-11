#ifndef SCATTER_TRANSFORM
#define SCATTER_TRANSFORM

//Base
bool _AlignDirection;
float3 _BaseOffset;
float3 _BaseRotate;
float3 _BaseSize;
float3 _BaseExtrude;

//Randomize
float3 _RndOffset;
float3 _RndRotate;
float3 _RndScale;
float3 _RndExtrude;

void ApplyPosition(inout SampleResult result)
{
    float3 offset = _BaseOffset.xyz;
    float3 estrude = mul(result.directionMatrix, float4(_BaseExtrude, 0));

    float3 rndOffset = lerp(-_RndOffset, _RndOffset, result.randomize);
    float3 rndShift = mul(result.directionMatrix, float4(lerp(-_RndExtrude, _RndExtrude, result.randomize), 0)).xyz;

    result.position += offset + estrude + rndOffset + rndShift;
}
void ApplyRotation(inout SampleResult result)
{
    float3 rndRotate = lerp(-_RndRotate, _RndRotate, result.randomize);
    result.rotation = _BaseRotate + rndRotate;
}
void ApplyScale(inout SampleResult result)
{
    result.scale = _BaseSize.xyz + _BaseSize.xyz * _RndScale.xyz * result.randomize;
}

void ApplyTransform(inout SampleResult result)
{
    ApplyPosition(result);
    ApplyRotation(result);
    ApplyScale(result);
}

float4x4 GetMatrix(SampleResult result)
{
    float4x4 translateMat = TranslateMatrix(result.position);
    float4x4 rotateMat = Rotate(result.rotation);
    float4x4 scaleMat = ScaleMatrix(result.scale);

    float4x4 mat;
    if(_AlignDirection)
    {
        mat = TransformMul(translateMat, mul(result.directionMatrix, rotateMat), scaleMat);
    }
    else mat = TransformMul(translateMat, rotateMat, scaleMat);

    return mat;
}

#endif