using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MenuLogic : MonoBehaviour
{
    UIDocument _menuDocument;
    VisualElement _root;
    public GameOfLife gameOfLife;
    public GameOfLifeEditor gameOfLifeEditor;

    Button _resumeButton;
    Button _quitButton;
    Slider _speedSlider;
    Foldout _resolutionFoldout;
    const int resolutionCount = 10;
    List<ResolutionSelector> _resolutionSelectors = new List<ResolutionSelector>();
    void Start()
    {
        _menuDocument = GetComponent<UIDocument>();
        _root = _menuDocument.rootVisualElement;
        _resumeButton = _root.Q<Button>("ResumeButton");
        _quitButton = _root.Q<Button>("QuitButton");
        _speedSlider = _root.Q<Slider>("SimSpeed");
        _speedSlider.lowValue = 0.0f;
        _speedSlider.highValue = 4.5f;
        _resolutionFoldout = _root.Q<Foldout>("ResolutionFoldout");

        _resumeButton.clicked += Resume;
        _quitButton.clicked += Quit;

        InitResolutionFoldout();

        Resume();
    }

    void InitResolutionFoldout()
    {
        for (int i = 0;i < resolutionCount;i++)
        {
            int res = (int)Mathf.Pow(2,i + 5);
            Button button = new Button();
            button.text = res.ToString();
            button.AddToClassList("resolution_selector_button");
            _resolutionFoldout.contentContainer.Add(button);
            ResolutionSelector selector = new ResolutionSelector(this, res);
            _resolutionSelectors.Add(selector);
            button.clicked += selector.OnSelected;
        }
        gameOfLife.settings.resolution = int.Parse(_resolutionFoldout.text);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_root.style.visibility == Visibility.Visible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        gameOfLife.settings.simulationSpeed = Mathf.Pow(10,_speedSlider.value);
    }
    void Resume()
    {
        gameOfLifeEditor.enabled = true;
        _root.style.visibility = Visibility.Hidden;
    }
    void Pause()
    {
        gameOfLifeEditor.enabled = false;
        _root.style.visibility = Visibility.Visible;
    }
    void Quit()
    {
        Application.Quit();
    }


    private class ResolutionSelector{
        private MenuLogic _menu;
        private int _resolution;

        public ResolutionSelector(MenuLogic menu, int resolution)
        {
            _menu = menu;
            _resolution = resolution;
        }

        public void OnSelected()
        {
            _menu.gameOfLife.settings.resolution = _resolution;
            _menu._resolutionFoldout.text = _resolution.ToString();
            _menu._resolutionFoldout.value = false;
        }
    }

}
