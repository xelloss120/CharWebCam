using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Vector3 MousePosBuf;

    void Update()
    {
        Camera camera = GetComponent<Camera>();

        Move(camera);
        Angle(camera);
    }

    /// <summary>
    /// カメラ移動
    /// </summary>
    /// <param name="camera">対象カメラ</param>
    void Move(Camera camera)
    {
        Vector3 pos = camera.transform.position;

        // XY
        if (Input.GetMouseButtonDown(0))
        {
            MousePosBuf = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            pos.x += (MousePosBuf.x - Input.mousePosition.x) * 0.001f;
            pos.y += (MousePosBuf.y - Input.mousePosition.y) * 0.001f;
            MousePosBuf = Input.mousePosition;
        }

        // Z
        pos.z += Input.GetAxis("Mouse ScrollWheel");

        camera.transform.position = pos;
    }

    /// <summary>
    /// カメラ角度
    /// </summary>
    /// <param name="camera">対象カメラ</param>
    void Angle(Camera camera)
    {
        Vector3 Ang = camera.transform.eulerAngles;

        if (Input.GetMouseButtonDown(1))
        {
            MousePosBuf = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            Ang.x += (MousePosBuf.y - Input.mousePosition.y) * 0.05f;
            Ang.y += (MousePosBuf.x - Input.mousePosition.x) * 0.05f;
            MousePosBuf = Input.mousePosition;
        }

        camera.transform.eulerAngles = Ang;
    }
}
