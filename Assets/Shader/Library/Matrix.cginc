#ifndef MatrixLibrary
#define MatrixLibrary

float4x4 DirectionMatrix(float3 direction, float3 left)
{
    float3 forwrad = cross(direction, -left);
    float4x4 mat = 0;

    mat[0] = float4(left.x, direction.x, forwrad.x, 0);
    mat[1] = float4(left.y, direction.y, forwrad.y, 0);
    mat[2] = float4(left.z, direction.z, forwrad.z, 0);
    mat[3] = float4(0, 0, 0, 1);

    return mat;
}
float4x4 RotateX(float rad)
{
    float4x4 m = 0;                                     
    m[0] = float4(1, 0, 0, 0);
    m[1] = float4(0, cos(rad), -sin(rad), 0);
    m[2] = float4(0, sin(rad), +cos(rad), 0);
    m[3] = float4(0, 0, 0, 1);
    return m;
}
float4x4 RotateY(float rad)
{
    float4x4 m = 0;
    m[0] = float4(+cos(rad), 0, sin(rad), 0);  
    m[1] = float4(0, 1, 0, 0);  
    m[2] = float4(-sin(rad), 0, cos(rad), 0);  
    m[3] = float4(0, 0, 0, 1);                   
    return m;
}
float4x4 RotateZ(float rad)
{
    float4x4 m = 0;
    m[0] = float4(cos(rad), -sin(rad), 0, 0);  
    m[1] = float4(sin(rad), +cos(rad), 0, 0);  
    m[2] = float4(0, 0, 1, 0);  
    m[3] = float4(0, 0, 0, 1);                       
    return m;
}
float4x4 Rotate(float3 rad)
{
    return mul(mul(RotateY(rad.y), RotateX(rad.x)), RotateZ(rad.z));
}
float4x4 ScaleMatrix(float3 scale)
{
    float4x4 mat = 0;

    mat[0] = float4(scale.x, 0, 0, 0);
    mat[1] = float4(0, scale.y, 0, 0);
    mat[2] = float4(0, 0, scale.z, 0);
    mat[3] = float4(0, 0, 0, 1);

    return mat;
}
float4x4 TranslateMatrix(float3 translate)
{
    float4x4 mat = 0;

    mat[0] = float4(1, 0, 0, translate.x);
    mat[1] = float4(0, 1, 0, translate.y);
    mat[2] = float4(0, 0, 1, translate.z);
    mat[3] = float4(0, 0, 0, 1);

    return mat;
}
float4x4 TransformMul(float4x4 translate, float4x4 rotate, float4x4 scale)
{
	return mul(mul(translate, rotate), scale);
}

#endif



