using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeMeshGenerator : MonoBehaviour
{
    public float width;
    public float height;
    public Vector3 center;

    List<Vector3> vertices = new List<Vector3>();
    private MeshFilter meshFilter;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
        meshFilter = GetComponent<MeshFilter>();
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        Vector2 bottomLeft = new Vector2(center.x - width / 2, center.z - height / 2);
        Vector2 bottomRight = new Vector2(center.x + width / 2, center.z - height / 2);
        Vector2 topRight = new Vector2(center.x + width / 2, center.x + height / 2);
        Vector2 topLeft = new Vector2(center.x - width / 2, center.z + height / 2);

        Vector2[] vertices2D = new Vector2[] { bottomLeft, bottomRight, topRight, topLeft };
        Triangular tr = new Triangular(vertices2D);
        int[] indices = tr.Triangulate();

        Vector3[] vertices = new Vector3[vertices2D.Length];
        for(int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
    {
        var j = polyPoints.Length - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Length; j = i++)
        {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }
}

class Triangular
{
    private List<Vector2> m_vertices;
    public Triangular(Vector2[] vertices)
    {
        m_vertices = new List<Vector2>();
        m_vertices.AddRange(vertices);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();
        int n = m_vertices.Count;
        if(n < 3)
        {
            return indices.ToArray();
        }

        int[] V = new int[n];
        if(Area() > 0)
        {
            for(int v = 0; v < n; ++v)
            {
                V[v] = v;
            }
        }
        else
        {
            for(int v = 0; v < n; ++v)
            {
                V[v] = (n - 1) - v;
            }
        }

        int nv = n;
        int count = 2 * nv;
        for(int v = nv - 1; nv > 2;)
        {
            if((count--) <= 0)
            {
                return indices.ToArray();
            }
            int u =v;
            if(nv <= u)
            {
                u = 0;
            }
            v = u + 1;
            if(nv <= v)
            {
                v = 0;
            }
            int w = v + 1;
            if(nv <= w)
            {
                w = 0;
            }

            if(Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for(s = v, t = v + 1; t < nv; ++s, ++t)
                {
                    V[s] = V[t];
                }
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_vertices.Count;
        float A = 0;

        for(int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_vertices[p];
            Vector2 qval = m_vertices[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A / 2.0f;
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_vertices[V[u]];
        Vector2 B = m_vertices[V[v]];
        Vector2 C = m_vertices[V[w]];
        if(Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
        {
            return false;
        }
        for(p = 0; p < n; p++)
        {
            if((p == u) || (p == v) || (p == w))
            {
                continue;
            }
            Vector2 P = m_vertices[V[p]];
            if(InsideTriangle(A, B, C, P))
            {
                return false;
            }
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}