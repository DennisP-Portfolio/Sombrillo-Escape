using System.Collections;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Values")]
    public bool _IsPlayingLevel; // Check if player is playing a level instead of a custom maze
    [SerializeField] private int _CurrentSelectedLevel;
    [SerializeField] private int _MaximumLevel;
    [SerializeField] private int _LevelsUnlocked = 1;
    [Space(20)]
    [SerializeField] private int _Width = 5;
    [SerializeField] private int _Height = 5;

    [Header("Text")]
    [SerializeField] private TextMeshPro _CurrentLevelText;
    [SerializeField] private TextMeshPro _HeightText;
    [SerializeField] private TextMeshPro _WidthText;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _NotUnlockedSprite;

    private int _currentDrawnLevel;
    private bool _blinking;

    private TrailRenderer _trailRenderer;
    private MazeRenderer _mazeRenderer;
    private MonsterController _monster;

    void Start()
    {
        _trailRenderer = FindObjectOfType<TrailRenderer>();
        _mazeRenderer = FindObjectOfType<MazeRenderer>();

        IsSelectedLevelUnlocked();
        UpdateTextAndSize();
    }

    void Update()
    {
        if (_monster == null)
        {
            // Finding the monster after it has been put in the scene
            _monster = FindObjectOfType<MonsterController>();
        }
    }

    public void SetLevelSize()
    {
        _mazeRenderer.SetMazeSize(_Width, _Height);
        _mazeRenderer.ReDrawMaze();
        _trailRenderer.Clear();
    }

    public void StartNextLevel()
    {
        _CurrentSelectedLevel = _LevelsUnlocked;
        IsSelectedLevelUnlocked();
        UpdateTextAndSize();

        _monster.ResetMonsterSpeed();

        _mazeRenderer.SetMazeSize(_Width, _Height);
        _mazeRenderer.ReDrawMazeWithRespawn();
        _trailRenderer.Clear();
    }

    public void RetryLevel()
    {
        _CurrentSelectedLevel = _currentDrawnLevel;
        _mazeRenderer.ResetPlayerPosAndTime();
        _trailRenderer.Clear();

        _monster.ResetMonsterSpeed();

        IsSelectedLevelUnlocked();
        UpdateTextAndSize();
    }

    /// <summary>
    /// Remembers which level is drawn for knowing if finishing will unlock the next level
    /// </summary>
    public void SetCurrentDrawnLevel()
    {
        _currentDrawnLevel = _CurrentSelectedLevel;
    }

    public void ChangeLevel(int amount)
    {
        if (!_blinking)
        {
            _CurrentSelectedLevel += amount;
        }

        if (_CurrentSelectedLevel < 1)
        {
            _CurrentSelectedLevel = 1;
        }
        if (_CurrentSelectedLevel > _MaximumLevel)
        {
            _CurrentSelectedLevel = _MaximumLevel;
        }

        IsSelectedLevelUnlocked();
        UpdateTextAndSize();
    }

    /// <summary>
    /// Only unlocks a new level when the player is playing the highest level possible
    /// </summary>
    public bool ShouldUnlockNewLevel()
    {
        if (_currentDrawnLevel == _LevelsUnlocked)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddUnlockedLevel()
    {
        if (_IsPlayingLevel && _LevelsUnlocked < _MaximumLevel && _CurrentSelectedLevel < _MaximumLevel)
        {
            _LevelsUnlocked++;
        }
        IsSelectedLevelUnlocked();
        UpdateTextAndSize();
    }

    public bool IsSelectedLevelUnlocked()
    {
        if (!_blinking)
        {
            if (_CurrentSelectedLevel > _LevelsUnlocked)
            {
                _NotUnlockedSprite.enabled = true;
                return false;
            }
            else
            {
                _NotUnlockedSprite.enabled = false;
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Indicating that the player needs to play the highest unlocked level first to unlock a new level
    /// </summary>
    public IEnumerator BlinkNotUnlockedIndicator()
    {
        if (!_blinking)
        {
            _blinking = true;

            _NotUnlockedSprite.enabled = false;

            yield return new WaitForSeconds(0.5f);

            _NotUnlockedSprite.enabled = true;

            yield return new WaitForSeconds(0.5f);

            _NotUnlockedSprite.enabled = false;

            yield return new WaitForSeconds(0.5f);

            _NotUnlockedSprite.enabled = true;

            yield return new WaitForSeconds(0.5f);

            _NotUnlockedSprite.enabled = false;
            _CurrentSelectedLevel = _LevelsUnlocked;
            UpdateTextAndSize();

            _blinking = false;
        }
        else
        {
            yield return null;
        }
    }

    public void ReturnSizeValues(out int width, out int height)
    {
        width = _Width;
        height = _Height;
    }

    private void UpdateTextAndSize()
    {
        int size = 5 + (_CurrentSelectedLevel - 1) * 2;
        _Width = size;
        _Height = size;

        _CurrentLevelText.text = _CurrentSelectedLevel.ToString();
        _WidthText.text = _Width.ToString();
        _HeightText.text = _Height.ToString();
    }
}