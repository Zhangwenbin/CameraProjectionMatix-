using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraProjectionMatrix : MonoBehaviour
{
    public Camera camera;

    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        var start = Gizmos.matrix;
        Gizmos.matrix=Matrix4x4.TRS(transform.position,transform.rotation,transform.localScale);
        Gizmos.color = Color.yellow;
        Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView,camera.farClipPlane,0,camera.aspect);
        Gizmos.color=Color.red;
        Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView,camera.farClipPlane,camera.nearClipPlane,camera.aspect);
        Gizmos.matrix = start;
        
        
        Gizmos.DrawLine(transform.position, transform.position+transform.right*10);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position+transform.forward*10);
        Gizmos.color=Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position+transform.up*10);

        var vertices = GetLocation();
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.color=Color.red;
            var pos =new Vector4(vertices[i].x,vertices[i].y,vertices[i].z,1) ;
            Gizmos.DrawSphere(pos,0.3f);
            Gizmos.color=Color.yellow;
            var viewPos=camera.worldToCameraMatrix*(pos);
            Gizmos.DrawSphere(viewPos,0.3f);
            
            var proPos= camera.projectionMatrix*viewPos;
            proPos = proPos / proPos.w;
            Gizmos.color=Color.blue;
            Gizmos.DrawSphere(proPos,0.3f);
        }
        
        Gizmos.color=Color.red;

        if (CheckPointIsInCamera(target.transform.position,camera))
        {
            Gizmos.DrawSphere(target.transform.position,1);
        }
        
        if (CheckBoundIsInCamera(target.transform.GetComponent<BoxCollider>().bounds,camera))
        {
            Gizmos.DrawCube(target.transform.position,Vector3.one);
        }

    }

    public List<Vector3> GetLocation()
    {
        List<Vector3> ret = new List<Vector3>();
        for (int z = 0; z < 2; z++)
        {
            for (int i = -1; i < 2; i+=2)
            {
                for (int j = -1; j < 2; j+=2)
                {
                    var pos = GetPos(z, new Vector2Int(i, j));
                    ret.Add(pos);
                }
            }
        }

        return ret;
    }

    Vector3 GetPos(int type, Vector2Int posType)
    {
        var z = camera.nearClipPlane;
        if (type==1)
        {
            z = camera.farClipPlane;
        }

        var verticalAngle = Mathf.Deg2Rad* camera.fieldOfView/2;
        var horizonAngle = Mathf.Deg2Rad* Camera.VerticalToHorizontalFieldOfView(camera.fieldOfView,camera.aspect)/2;

        var xOffset = Mathf.Tan(horizonAngle) * z*posType.x;
        var yOffset = Mathf.Tan(verticalAngle) * z*posType.y;
        return camera.transform.position + new Vector3(xOffset, yOffset, z);

    }
    
    public static bool CheckPointIsInCamera(Vector3 worldPoint, Camera camera)
    {
        Vector4 projectionPos = camera.projectionMatrix * camera.worldToCameraMatrix * new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, 1);
        if (projectionPos.x < -projectionPos.w) return false;
        if (projectionPos.x > projectionPos.w) return false;
        if (projectionPos.y < -projectionPos.w) return false;
        if (projectionPos.y > projectionPos.w) return false;
        if (projectionPos.z < -projectionPos.w) return false;
        if (projectionPos.z > projectionPos.w) return false;
        return true;
    }
    
    public static bool CheckBoundIsInCamera( Bounds bound, Camera camera)
    {
        System.Func<Vector4, int> ComputeOutCode = (projectionPos) =>
        {
            int _code = 0;
            if (projectionPos.x < -projectionPos.w) _code |= 1;
            if (projectionPos.x > projectionPos.w) _code |= 2;
            if (projectionPos.y < -projectionPos.w) _code |= 4;
            if (projectionPos.y > projectionPos.w) _code |= 8;
            if (projectionPos.z < -projectionPos.w) _code |= 16;
            if (projectionPos.z > projectionPos.w) _code |= 32;
            return _code;
        };
        Vector4 worldPos = Vector4.one;
        int code = 63;
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    worldPos.x = bound.center.x + i * bound.extents.x;
                    worldPos.y = bound.center.y + j * bound.extents.y;
                    worldPos.z = bound.center.z + k * bound.extents.z;
                    code &= ComputeOutCode(camera.projectionMatrix * camera.worldToCameraMatrix * worldPos);
                }
            }
        }
        return code == 0 ? true : false;
    }
}
