using UnityEngine;

public class temptest : MonoBehaviour
{
    [SerializeField] Transform[] vertex;
    Vector3 a { get => vertex[0].position; }
    Vector3 b { get => vertex[1].position; }
    Vector3 c { get => vertex[2].position; }

    [SerializeField] int amount = 50;
    [SerializeField] bool triIndex;

    void OnDrawGizmos()
    {
        Trirandom();
    }

    void DirAxis()
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, a);

        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
        Vector3 right = (b - a).normalized;
        Vector3 forward = Vector3.Cross(normal, right).normalized;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(a, normal * 5);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(a, right * 5);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(a, forward * 5);
    }
    void Trirandom()
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, a);

        float rectArea = Vector3.Cross(b - a, c - a).magnitude;
        float width = (a - b).magnitude;
        float height = rectArea / width;
        //float whRatio = (height / width);
        float widthSplit = Mathf.Sqrt(amount) * (width / height);
        float heightSplit =  Mathf.Sqrt(amount) * (height / width);
        Debug.LogError(widthSplit);
        Debug.LogError(heightSplit);

        for (int i = 0; i < amount - (widthSplit / 2); i++)
        {
            float sqrtCount = Mathf.Sqrt(amount);
            float abMapping = ((i % sqrtCount)) / sqrtCount;
            float acMapping = ((i / sqrtCount) / 2) / sqrtCount;

            //Debug.LogError(acMapping);
            //Debug.LogError(abMapping);
            abMapping += 1f / sqrtCount * 0.5f;
            acMapping += 1f / sqrtCount * 0.5f;

            if (acMapping + abMapping > 1)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawCube(LerpOnTriangle(abMapping, acMapping), Vector3.one * 3);

                abMapping = 1 - abMapping;
                acMapping = 1 - acMapping + 1f / sqrtCount * 0.5f;
                //continue;
            }

            Vector3 point = LerpOnTriangle(abMapping, acMapping);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(point, Vector3.one * 3);
        }

        //int check = 0;
        //for (float h = 0; h < heightSplit; h++)
        //{
        //    float t = h / heightSplit;

        //    Vector3 lineDir = b - a;
        //    Vector3 lineStart = Vector3.Lerp(a, c, t);
        //    Vector3 lineEnd = lineStart + lineDir * (1 - t);

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(lineStart, lineEnd);

        //    float count = Mathf.Floor(widthSplit * (1 - t));
        //    for (int i = 0; i < count; i++)
        //    {
        //        float t2 = i / count;

        //        Vector3 point = Vector3.Lerp(lineStart, lineEnd, t2);

        //        Gizmos.color = Color.yellow;
        //        Gizmos.DrawSphere(point, .5f);
        //        check++;
        //    }
        //}
        //Debug.Log("Check :" + check);

    }
    Vector3 LerpOnTriangle(float abValue, float acValue)
    {
        Vector3 abShift = Vector3.Lerp(Vector3.zero, b - a, abValue);
        Vector3 acShift = Vector3.Lerp(Vector3.zero, c - a, acValue);
        Vector3 position = a + abShift + acShift;

        return position;


    }
}
