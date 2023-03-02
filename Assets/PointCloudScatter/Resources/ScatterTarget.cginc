#ifndef SCATTER_TARGET
#define SCATTER_TARGET

struct SampleTarget
{
    float3 vertexA, vertexB, vertexC;
    float3 normalA, normalB, normalC;
    float width, height;
    float wRatio, hratio;
    float area;
};

float4x4 _LocalToWorldMat;

//Sample SampleTarget
StructuredBuffer<int> trianglesBuffer;
StructuredBuffer<float3> verticesBuffer;
StructuredBuffer<float3> normalsBuffer;

SampleTarget GetSurface(int index)
{
    index *= 3;

    int indexA = trianglesBuffer[index + 0];
    int indexB = trianglesBuffer[index + 1];
    int indexC = trianglesBuffer[index + 2];

    float3 offet = _LocalToWorldMat._m03_m13_m23;
    float3 vertexA = mul(_LocalToWorldMat, float4(verticesBuffer[indexA], 1)).xyz;
    float3 vertexB = mul(_LocalToWorldMat, float4(verticesBuffer[indexB], 1)).xyz;
    float3 vertexC = mul(_LocalToWorldMat, float4(verticesBuffer[indexC], 1)).xyz;
    //why matrix multiply offset not working ? because vector.w 0 well cancel matrix column 3 multiply
    
    // float4x4 rotMatrix = _LocalToWorldMat;
    // rotMatrix._m03_m13_m23 = 0;
    float3 normalA = mul(_LocalToWorldMat, float4(normalsBuffer[indexA], 0)).xyz;
    float3 normalB = mul(_LocalToWorldMat, float4(normalsBuffer[indexB], 0)).xyz;
    float3 normalC = mul(_LocalToWorldMat, float4(normalsBuffer[indexC], 0)).xyz;

    float rectArea = length(cross(vertexA - vertexB, vertexA - vertexC));
    // float width = length(vertexA - vertexB);
    // float height = rectArea / width;
    // float wRatio = width / (height + width);
    // float hRatio = height / (height + width);

    SampleTarget tri;
    tri.vertexA = vertexA;
    tri.vertexB = vertexB;
    tri.vertexC = vertexC;
    tri.normalA = normalA;
    tri.normalB = normalB;
    tri.normalC = normalC;
    tri.area = length(cross(vertexA - vertexB, vertexA - vertexC)) / 2;
  
    return tri;
}

#endif