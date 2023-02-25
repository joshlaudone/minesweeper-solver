using UnityEngine;
using UnityEngine.Tilemaps;

public class StaticCamera : MonoBehaviour
{

    public Tilemap tilemap;

    Vector3 tilemapSize;
    Vector3 cameraPos;
    float orthoWidth, orthoHeight;
    float maxOrthoSize;
    float zoomRate;
    float minOrthoSize;
    float translationRate;

    // Start is called before the first frame update
    void Start()
    {
        zoomRate = 2f;
        translationRate = 1f;

        tilemapSize = tilemap.size;

        orthoHeight = tilemapSize.y * 0.5f;
        orthoWidth  = tilemapSize.x * Screen.height / Screen.width * 0.5f;

        maxOrthoSize = Mathf.Max(orthoWidth, orthoHeight);
        minOrthoSize = 5;

        cameraPos.x = tilemap.size.x * 0.5f;
        cameraPos.y = tilemap.size.y * 0.5f;
        cameraPos.z = Camera.main.transform.position.z;

        Camera.main.orthographicSize = maxOrthoSize;
        Camera.main.transform.position = cameraPos;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize - 
                                                     Camera.main.orthographicSize * zoomRate * Time.deltaTime, minOrthoSize);
        }

        if (Input.GetKey(KeyCode.X))
        {
            Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize +
                                                     Camera.main.orthographicSize * zoomRate * Time.deltaTime, maxOrthoSize);
        }

        if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position = Camera.main.transform.position +
                Time.deltaTime * translationRate * Camera.main.orthographicSize * Vector3.up;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.position = Camera.main.transform.position +
                Time.deltaTime * translationRate * Camera.main.orthographicSize * Vector3.left;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position = Camera.main.transform.position +
                Time.deltaTime * translationRate * Camera.main.orthographicSize * Vector3.down;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.position = Camera.main.transform.position +
                Time.deltaTime * translationRate * Camera.main.orthographicSize * Vector3.right;
        }

    }
}
