using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGrid : MonoBehaviour
{
    private static TilemapGrid instance;
    public Tilemap tilemap;

    public static TilemapGrid Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TilemapGrid>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<TilemapGrid>();
                    singletonObject.name = typeof(TilemapGrid).ToString() + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public Vector3Int GetCellPosition(Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }

    public Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        return tilemap.CellToWorld(cellPosition) + tilemap.tileAnchor;
    }
}
