using UnityEngine;

public class DotSpawner : MonoBehaviour
{
    public GameObject dotPrefab;
    public int rows = 10;
    public int columns = 10;
    public float spacing = 1f;
    public Vector2 startPos = new Vector2(-4, 4);

    void Start()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector2 spawnPos = startPos + new Vector2(x * spacing, -y * spacing);
                Instantiate(dotPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
