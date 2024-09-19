using System.Collections;
using Pathfinding;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("Values")]
    [SerializeField, Range(1, 50)] private int _Width = 10;
    [SerializeField, Range(1, 50)] private int _Height = 10;
    [Space(20)]
    [SerializeField] private float _Size = 1f;

    [Header("Prefabs")]
    [SerializeField] private Transform _WallPrefab = null;
    [SerializeField] private Transform _FloorPrefab = null;
    [SerializeField] private Transform _CeilingPrefab = null;
    [SerializeField] private Transform _StartBox = null;
    [SerializeField] private Transform _FinishBox = null;
    [SerializeField] private Transform _PlayerSpawnPoint = null;
    [SerializeField] private Transform _PlayerRespawnPoint = null;

    private Transform _floorObject;
    private Transform _ceilingObject;

    private AIPath _aiPath;
    private GameObject _monster;
    private GridGraph _currentGraph;
    private MiniMapData _miniMapData;
    private Transform _playerPosition;
    private LevelManager _levelManager;
    private InteractibleSpawner _interactibleSpawner;
    private Collecting _playerCollecting;

    private void Start()
    {
        _miniMapData = FindObjectOfType<MiniMapData>();
        _playerPosition = FindObjectOfType<PlayerMovement>().transform;
        _levelManager = FindObjectOfType<LevelManager>();
        _interactibleSpawner = FindObjectOfType<InteractibleSpawner>();
        _playerCollecting = FindObjectOfType<Collecting>();

        var maze = MazeGenerator.Generate(_Width, _Height);
        Draw(maze);

        SetUpAstarPathfinding();
    }

    private void Draw(EWallState[,] maze)
    {
        _levelManager.SetCurrentDrawnLevel();

        _floorObject = null;
        _ceilingObject = null;

        // Instantiate the floor object and set its scale
        _floorObject = Instantiate(_FloorPrefab, transform);
        _floorObject.localScale = new Vector3(_Size * _Width / 10, 1, _Size * _Height / 10);

        // Instantiate the ceiling object and set its position and scale
        Vector3 ceilingPos = new Vector3(transform.position.x, transform.position.y + _WallPrefab.localScale.y * 2, transform.position.z);
        _ceilingObject = Instantiate(_CeilingPrefab, ceilingPos, _CeilingPrefab.transform.rotation);
        _ceilingObject.localScale = new Vector3(_Size * _Width / 10, 1, _Size * _Height / 10);
        _ceilingObject.transform.parent = transform;

        // Set the positions for the start and finish boxes
        float xPosBox = -_Width * _Size / 2 + _Width / 2 * _Size + _Size / 2;
        float yPosBox = _WallPrefab.transform.localScale.y / 2 - 0.804f;
        float zPosStartBox = -_Height * _Size / 2 + _Size / 2;
        float zPosFinishBox = -_Height * _Size / 2 + (_Height - 1) * _Size + _Size / 2;

        // Set the position for the start box and instantiate its top wall
        _StartBox.position = new Vector3(xPosBox, yPosBox, zPosStartBox) + new Vector3(0, 0, -_Size / 2) + new Vector3(0, 0, -10.2f);
        var startBoxTopWall = Instantiate(_WallPrefab, transform) as Transform;
        startBoxTopWall.position = new Vector3(xPosBox, startBoxTopWall.localScale.y * 1.5f, zPosStartBox + -_Size / 2);
        startBoxTopWall.localScale = new Vector3(_Size, startBoxTopWall.localScale.y, startBoxTopWall.localScale.z);

        // Set the position for the finish box and instantiate its top wall
        _FinishBox.position = new Vector3(xPosBox, yPosBox, zPosFinishBox) + new Vector3(0, 0, _Size / 2) + new Vector3(0, 0, 10.2f);
        var finishBoxTopWall = Instantiate(_WallPrefab, transform) as Transform;
        finishBoxTopWall.position = new Vector3(xPosBox, finishBoxTopWall.localScale.y * 1.5f, zPosFinishBox + _Size / 2);
        finishBoxTopWall.localScale = new Vector3(_Size, finishBoxTopWall.localScale.y, finishBoxTopWall.localScale.z);

        _PlayerSpawnPoint.position = _StartBox.position;
        _PlayerRespawnPoint.position = _StartBox.position;

        for (int i = 0; i < _Width; i++)
        {
            for (int j = 0; j < _Height; j++)
            {
                var cell = maze[i, j];

                // Calculate the starting position for the current cell
                float startXPos = -_Width * _Size / 2 + i * _Size + _Size / 2;
                float startYPos = _WallPrefab.transform.localScale.y / 2;
                float startZPos = -_Height * _Size / 2 + j * _Size + _Size / 2;
                var position = new Vector3(startXPos, startYPos, startZPos);

                // Instantiate the walls based on if the current cell has the flag
                if (cell.HasFlag(EWallState.Up))
                {
                    var topWall = Instantiate(_WallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0, 0, _Size / 2);
                    topWall.localScale = new Vector3(_Size, topWall.localScale.y, topWall.localScale.z);
                }

                if (cell.HasFlag(EWallState.Left))
                {
                    var leftWall = Instantiate(_WallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-_Size / 2, 0, 0);
                    leftWall.localScale = new Vector3(_Size, leftWall.localScale.y, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0, 90, 0);
                }

                if (i == _Width - 1)
                {
                    if (cell.HasFlag(EWallState.Right))
                    {
                        var rightWall = Instantiate(_WallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(_Size / 2, 0, 0);
                        rightWall.localScale = new Vector3(_Size, rightWall.localScale.y, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }

                if (j == 0)
                {
                    if (cell.HasFlag(EWallState.Down))
                    {
                        var bottomWall = Instantiate(_WallPrefab, transform) as Transform;
                        bottomWall.position = position + new Vector3(0, 0, -_Size / 2);
                        bottomWall.localScale = new Vector3(_Size, bottomWall.localScale.y, bottomWall.localScale.z);
                    }
                }
            }
        }
    }

    public void ReDrawMaze()
    {
        DestroyAllChildren(transform);

        var maze = MazeGenerator.Generate(_Width, _Height);
        Draw(maze);

        _aiPath.canMove = false;
        SetUpAstarPathfinding();
    }

    /// <summary>
    /// Redrawing the maze for next level
    /// </summary>
    public void ReDrawMazeWithRespawn()
    {
        DestroyAllChildren(transform);

        var maze = MazeGenerator.Generate(_Width, _Height);
        Draw(maze);

        _aiPath.canMove = false;
        SetUpAstarPathfinding();

        _playerPosition.position = _StartBox.position;
    }

    public void SetMazeSize(int width, int height)
    {
        _Width = width;
        _Height = height;
    }

    private Transform DestroyAllChildren(Transform transform)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        return transform;
    }

    private void SetUpAstarPathfinding()
    {
        _miniMapData.RemoveMonster();

        // This holds all graph data
        AstarData data = AstarPath.active.data;

        if (_currentGraph != null)
        {
            data.RemoveGraph(_currentGraph);
        }

        _aiPath = FindObjectOfType<AIPath>();
        _monster = FindObjectOfType<Seeker>().gameObject;

        _monster.SetActive(false);

        // This creates a Grid Graph
        _currentGraph = data.AddGraph(typeof(GridGraph)) as GridGraph;
        // Setup a grid graph with some values
        int width = _Width * (int)_Size / 2;
        int depth = _Height * (int)_Size / 2;
        float nodeSize = 2;
        _currentGraph.center = new Vector3(0, 0, 0);
        // Updates internal size from the above values
        _currentGraph.SetDimensions(width, depth, nodeSize);
        // Set obstacle layer mask
        _currentGraph.collision.mask = LayerMask.GetMask("Obstacle", "Platform");
        // Minus for detecting platforms downwards
        _currentGraph.collision.height = -25;

        StartCoroutine(ReScanGridCoroutine());
    }

    private IEnumerator ReScanGridCoroutine()
    {
        if (_interactibleSpawner.ShouldSpawn)
        {
            _interactibleSpawner.CanSpawnCheckRenderer = true;
        }

        yield return new WaitForSeconds(0.5f);

        ReScanPathGraph();

        yield return new WaitForSeconds(0.5f);

        _monster.SetActive(true);

        _miniMapData.SetUpMonster();
    }

    private void ReScanPathGraph()
    {
        AstarPath.active.Scan();
    }

    public void ReturnMinimapValues(out int width, out int height, out float size)
    {
        width = _Width;
        height = _Height;
        size = _Size;
    }

    /// <summary>
    /// Resetting player when jumpscared
    /// </summary>
    public void ResetPlayerPos()
    {
        _playerPosition.position = _StartBox.position;
        _miniMapData.ReAssignValues();
        _miniMapData.RespawnMonster();
        _interactibleSpawner.RespawnCurrentLevelPoints();
        _playerCollecting.HasKey = false;
    }

    /// <summary>
    /// Resetting player when retrying level
    /// </summary>
    public void ResetPlayerPosAndTime()
    {
        _playerPosition.position = _StartBox.position;
        _miniMapData.ReAssignValues();
        _miniMapData.RespawnMonster();
        _playerCollecting.HasKey = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_Size * _Width, 1, _Size * _Height));
        var startPos = new Vector3(-_Width * _Size / 2 + _Width / 2 * _Size + _Size / 2, _WallPrefab.transform.localScale.y / 2, -_Height * _Size / 2 + _Size / 2) + new Vector3(0, 0, -_Size / 2);
        Gizmos.DrawWireCube(startPos, new Vector3(_Size, _WallPrefab.localScale.y, _WallPrefab.localScale.z));
        var finishPos = new Vector3(-_Width * _Size / 2 + _Width / 2 * _Size + _Size / 2, _WallPrefab.transform.localScale.y / 2, -_Height * _Size / 2 + (_Height - 1) * _Size + _Size / 2) + new Vector3(0, 0, _Size / 2);
        Gizmos.DrawWireCube(finishPos, new Vector3(_Size, _WallPrefab.localScale.y, _WallPrefab.localScale.z));
    }
}