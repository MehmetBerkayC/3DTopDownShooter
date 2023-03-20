using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    /// Notes
    /// Fisher Yates Shuffle -> Used Algorithm
    /// navmeshmask calculation "https://youtu.be/vQgLdFNrCN8?t=398"

    [SerializeField] Transform navMeshFloor;
    [SerializeField] Transform navMeshMaskPrefab;

    [SerializeField] Transform tilePrefab;
    [SerializeField] Transform obstaclePrefab;

    [SerializeField] Vector2 mapSize;
    [SerializeField] Vector2 maxMapSize;

    [SerializeField] float tileSize;
    [SerializeField, Range(0,1)] float outlinePercent;

    [SerializeField] int seed = 10;
    [SerializeField, Range(0,1)] float obstaclePercentage;

    List<Coordinate> allTileCoordinates;
    Queue<Coordinate> shuffledCoordinates;

    Coordinate mapCenter;

    private void Start()
    {
        GenerateMap();
    }
    public struct Coordinate
    {
        public int x, y;
        
        public Coordinate (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator == (Coordinate c1, Coordinate c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator != (Coordinate c1, Coordinate c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoordinate = shuffledCoordinates.Dequeue(); // FIFO
        shuffledCoordinates.Enqueue(randomCoordinate);
        return randomCoordinate;
    }

    Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
    }
    
        
    public void GenerateMap()
    {
        allTileCoordinates = new List<Coordinate>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoordinates.Add(new Coordinate(x, y));            
            } 
        }

        shuffledCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(allTileCoordinates.ToArray(), seed));

        mapCenter = new Coordinate((int)mapSize.x / 2, (int)mapSize.y / 2);

        string holderName = "Generated Map";
        
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;

        mapHolder.parent = transform;

        for(int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilepPosition = CoordinateToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilepPosition, Quaternion.Euler(90,0,0));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercentage);
        int currentObstacleCount = 0;


        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoordinate = GetRandomCoordinate();

            obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
            currentObstacleCount++;

            if (randomCoordinate != mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordinateToPosition(randomCoordinate.x, randomCoordinate.y);
           
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newObstacle.parent = mapHolder;
            }
            else
            {
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
                currentObstacleCount--;
            }
           
        }

        // Left Mask
        Transform maskLeft = Instantiate(navMeshMaskPrefab, Vector3.left * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - mapSize.x)/2, 1f, mapSize.y) * tileSize;

        // Right Mask
        Transform maskRight = Instantiate(navMeshMaskPrefab, Vector3.right * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1f, mapSize.y) * tileSize;
       
        // Right Mask
        Transform maskTop = Instantiate(navMeshMaskPrefab, Vector3.forward * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - mapSize.y) / 2) * tileSize;
        
        // Right Mask
        Transform maskDown = Instantiate(navMeshMaskPrefab, Vector3.back * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity);
        maskDown.parent = mapHolder;
        maskDown.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        // NavMeshFloor
        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 0f) * tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x, mapCenter.y] = true;

        int accessibleTileCount = 1; // 1 for center tile (accessible at all times)

        // Flood fill algorithm
        while (queue.Count > 0)
        {
            Coordinate tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++) 
            {
                for (int y = -1; y <= 1; y++) 
                {
                    int neigbourX = tile.x + x;
                    int neigbourY = tile.y + y;
                    if(x == 0|| y == 0) // don't check diagonals (both shouldn't be 1/-1)
                    {
                        // Check if tile is valid (inside the boundaries of the board) (remember edges and corners dangerous)
                        if(neigbourX >= 0 && neigbourX < obstacleMap.GetLength(0) && neigbourY >= 0 && neigbourY < obstacleMap.GetLength(1)) 
                        {
                            // Pass if not already checked or not a wall/obstacle
                            if (!mapFlags[neigbourX, neigbourY] && !obstacleMap[neigbourX, neigbourY])
                            {
                                mapFlags[neigbourX, neigbourY] = true;  // mark it as done
                                queue.Enqueue(new Coordinate(neigbourX, neigbourY)); // enqueue adjacent tiles for next check
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }
}
