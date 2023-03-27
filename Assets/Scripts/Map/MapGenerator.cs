using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AI;

public class MapGenerator : MonoBehaviour
{
    /// Notes
    /// Fisher Yates Shuffle -> Used Algorithm
    /// navmeshmask calculation "https://youtu.be/vQgLdFNrCN8?t=398"

    [SerializeField] Transform navMeshFloor;
    [SerializeField] Transform navMeshMaskPrefab;

    [SerializeField] Transform tilePrefab;
    [SerializeField] Transform obstaclePrefab;

    [SerializeField] Vector2 maxMapSize;

    [SerializeField] float tileSize;
    [SerializeField, Range(0,1)] float outlinePercent;

    List<Coordinate> allTileCoordinates;
    Queue<Coordinate> shuffledCoordinates;
    
    Queue<Coordinate> shuffledOpenTileCoordinates;
    Transform[,] tileMap;

    [SerializeField] Map[] maps;
    [SerializeField] int mapIndex;
    Map currentMap;

    private void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }
    
    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
        NavMeshBuilder.BuildNavMesh();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        System.Random prng = new System.Random(currentMap.seed); // prng -> pseudo random number generation/generator

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f, currentMap.mapSize.y * tileSize);

        // Generating Coordinates
        allTileCoordinates = new List<Coordinate>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoordinates.Add(new Coordinate(x, y));            
            } 
        }

        shuffledCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(allTileCoordinates.ToArray(), currentMap.seed));

        // Create map holder object
        string holderName = "Generated Map";
        
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilepPosition = CoordinateToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilepPosition, Quaternion.Euler(90,0,0));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        // Obstacle generation
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercentage);
        int currentObstacleCount = 0;

        List<Coordinate> allOpentileCoordinates = new List<Coordinate>(allTileCoordinates);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoordinate = GetRandomCoordinate();

            obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
            currentObstacleCount++;

            if (randomCoordinate != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordinateToPosition(randomCoordinate.x, randomCoordinate.y);
           
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colourPercentage = randomCoordinate.y / (float)currentMap.mapSize.y; // (turn operation to float) if both integer it rounds down to 0
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colourPercentage);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpentileCoordinates.Remove(randomCoordinate);
            }
            else
            {
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(allOpentileCoordinates.ToArray(), currentMap.seed));

        // Creating Navmesh mask
        // Left Mask
        Transform maskLeft = Instantiate(navMeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f, 1f, currentMap.mapSize.y) * tileSize;

        // Right Mask
        Transform maskRight = Instantiate(navMeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1f, currentMap.mapSize.y) * tileSize;
       
        // Right Mask
        Transform maskTop = Instantiate(navMeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        
        // Right Mask
        Transform maskDown = Instantiate(navMeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskDown.parent = mapHolder;
        maskDown.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        // NavMeshFloor
        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 0f) * tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

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

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }


    [System.Serializable]
    public struct Coordinate
    {
        public int x, y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
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

    public Transform GetTileFromPosition (Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoordinate = shuffledCoordinates.Dequeue(); // FIFO
        shuffledCoordinates.Enqueue(randomCoordinate);
        return randomCoordinate;
    }

    public Transform GetRandomOpenTile()
    {
        Coordinate randomCoordinate = shuffledOpenTileCoordinates.Dequeue(); // FIFO
        shuffledOpenTileCoordinates.Enqueue(randomCoordinate);
        return tileMap[randomCoordinate.x, randomCoordinate.y];
    }

    Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    [System.Serializable]
    public class Map
    {
        public Coordinate mapSize;
        
        [Range(0,1)] 
        public float obstaclePercentage;
        
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coordinate mapCenter
        {
            get
            {
                return new Coordinate(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }

}
