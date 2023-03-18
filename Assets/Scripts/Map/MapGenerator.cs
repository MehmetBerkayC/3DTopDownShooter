using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    /// Notes
    /// Fisher Yates Shuffle -> Used Algorithm
    
    [SerializeField] Transform tilePrefab;
    [SerializeField] Transform obstaclePrefab;

    [SerializeField] Vector2 mapSize;

    [SerializeField, Range(0,1)] float outlinePercent;

    [SerializeField] int seed = 10;

    List<Coordinate> allTileCoordinates;
    Queue<Coordinate> shuffledCoordinates;

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
    }
    
    Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoordinate = shuffledCoordinates.Dequeue(); // FIFO
        shuffledCoordinates.Enqueue(randomCoordinate);
        return randomCoordinate;
    }

    Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
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
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }

        int obstacleCount = 10;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoordinate = GetRandomCoordinate();
            Vector3 obstaclePosition = CoordinateToPosition(randomCoordinate.x, randomCoordinate.y);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
            newObstacle.parent = mapHolder;
        }
    }
}
