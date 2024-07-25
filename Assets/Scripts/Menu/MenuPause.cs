using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPause : MonoBehaviour
{
    public static MenuPause Instance;

    public bool IsPaused => _isPaused;

    [SerializeField] private GameObject _menuFrame;

    private bool _isPaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _isPaused = false;
        _menuFrame.SetActive(false);
    }

    public void Pause()
    {
        _isPaused = !_isPaused;

        _menuFrame.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1F;
    }

    public void Continue()
    {
        _isPaused = false;
        _menuFrame.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
