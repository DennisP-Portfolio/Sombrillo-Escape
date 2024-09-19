using System;
using TMPro;
using UnityEngine;

public class MazeMenuManager : MonoBehaviour
{
    [NonSerialized] public bool _regenIsOnCooldown;

    [Header("Values")]
    [SerializeField] private int _Width;
    [SerializeField] private int _Height;
    [SerializeField] private int _MinimumSize = 5;
    [SerializeField] private int _MaximumSize = 45;

    [Header("Text")]
    [SerializeField] private TextMeshPro _HeightText;
    [SerializeField] private TextMeshPro _WidthText;

    [Header("End Menu's")]
    [SerializeField] private GameObject _LevelMenu;
    [SerializeField] private GameObject _CustomMenu;

    private MazeRenderer _mazeRenderer;
    private TrailRenderer _trailRenderer;

    void Start()
    {
        _mazeRenderer = FindObjectOfType<MazeRenderer>();
        _trailRenderer = FindObjectOfType<TrailRenderer>();
        UpdateText();
    }

    public void ChangeHeight(int amount)
    {
        _Height += amount;
        if (_Height < _MinimumSize)
        {
            _Height = _MinimumSize;
        }
        if (_Height > _MaximumSize)
        {
            _Height = _MaximumSize;
        }
        UpdateText();
    }

    public void ChangeWidth(int amount)
    {
        _Width += amount;
        if (_Width < _MinimumSize)
        {
            _Width = _MinimumSize;
        }
        if (_Width > _MaximumSize)
        {
            _Width = _MaximumSize;
        }
        UpdateText();
    }

    public void SetValues()
    {
        _mazeRenderer.SetMazeSize(_Width, _Height);
        _mazeRenderer.ReDrawMaze();
        _trailRenderer.Clear();
    }

    public void ReturnSizeValues(out int width, out int height)
    {
        width = _Width;
        height = _Height;
    }

    /// <summary>
    /// Set the proper finish box menu based on if the player is playing a level or custom maze
    /// </summary>
    public void SwitchEndMenu(string type)
    {
        if (type == "Level")
        {
            _CustomMenu.SetActive(false);
            _LevelMenu.SetActive(true);
        }
        else if (type == "Custom")
        {
            _LevelMenu.SetActive(false);
            _CustomMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("You assigned the wrong string for end menu");
        }
    }

    private void UpdateText()
    {
        _WidthText.text = _Width.ToString();
        _HeightText.text = _Height.ToString();
    }
}
