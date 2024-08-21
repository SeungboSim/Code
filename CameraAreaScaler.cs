using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class CameraAreaScaler : MonoBehaviour
{
    public GameObject[] reflects;
    public GameObject enemyRespawn;
    public Spawner spawner;

    private void Awake()
    {
        float cameraY = Camera.main.orthographicSize;
        float cameraX = Camera.main.orthographicSize * Camera.main.aspect;

        reflects[0].transform.position = new Vector3(0, cameraY, 0);
        reflects[1].transform.position = new Vector3(0, -cameraY, 0);
        reflects[2].transform.position = new Vector3(cameraX, 0, 0);
        reflects[3].transform.position = new Vector3(-cameraX, 0, 0);

        reflects[0].GetComponent<BoxCollider2D>().size = new Vector2(cameraX * 2, 0.1f);
        reflects[1].GetComponent<BoxCollider2D>().size = new Vector2(cameraX * 2, 0.1f);
        reflects[2].GetComponent<BoxCollider2D>().size = new Vector2(0.1f, cameraY * 2);
        reflects[3].GetComponent<BoxCollider2D>().size = new Vector2(0.1f, cameraY * 2);

        enemyRespawn.transform.localScale = new Vector2(cameraX * 4, cameraY * 4);
    }
}
