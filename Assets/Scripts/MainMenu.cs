using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject[] StartSequence;
    int startIdx = -1;

    [SerializeField]
    GameObject MenuRoot;

    [SerializeField]
    MenuAction[] menuActions;

    [SerializeField, Range(0, 2)]
    float timeBetween = 0.6f;
    
    string[] menuTexts;
    int selectedIndex = 0;

    [SerializeField]
    string NoGamePad;

    [SerializeField]
    TextMeshProUGUI UseGamepad;

    [SerializeField]
    string PluginGamepad;

    GameObject showing;

    private void Start()
    {
        if (SimpleUnifiedInput.HasGamepad)
        {
            UseGamepad.text += string.Join("\n", NoGamePad.Split('|'));
        } else
        {
            UseGamepad.text += string.Join("\n", PluginGamepad.Split('|'));
        }
        menuTexts = new string[menuActions.Length];
        for (int i = 0; i<menuActions.Length; i++)
        {
            menuTexts[i] = menuActions[i].GetComponent<TextMeshProUGUI>()?.text ?? "";
        }
        SetSelected(selectedIndex);
        SetNextStartSequence();
    }

    bool SetNextStartSequence()
    {
        startIdx++;
        if (startIdx > StartSequence.Length - 1) return false;
        if (showing != null)
        {
            showing.SetActive(false);
        }
        MenuRoot.SetActive(false);
        showing = StartSequence[startIdx];
        showing.SetActive(true);
        return true;
    }

    void SetSelected(int selected)
    {
        if (selected < 0) selected = menuActions.Length - 1;
        if (selected > menuActions.Length - 1) selected = 0;
        
        var prevTextField = menuActions[selectedIndex].GetComponent<TextMeshProUGUI>();
        if (prevTextField != null) prevTextField.text = menuTexts[selectedIndex];

        selectedIndex = selected;

        var textField = menuActions[selectedIndex].GetComponent<TextMeshProUGUI>();
        if (textField != null) textField.text = string.Format("> {0} <", menuTexts[selectedIndex]);
    }

    float lastMove = 0;
    bool isMoving = false;
    bool confirming = false;
    private void Update()
    {
        if (!confirming && SimpleUnifiedInput.Confirm)
        {
            confirming = true;
            if (SetNextStartSequence())
            {
                // Do nothing extra
            } else if (showing != null)
            {
                showing.SetActive(false);
                showing = null;
                MenuRoot.SetActive(true);
            } else
            {
                var active = menuActions[selectedIndex];
                if (!string.IsNullOrEmpty(active.LoadLevel)) {
                    SceneManager.LoadScene(active.LoadLevel);
                } else if (active.ShowGameObject != null)
                {
                    MenuRoot.SetActive(false);
                    showing = active.ShowGameObject;
                    showing.SetActive(true);
                }
            }
            return;
        } else
        {
            confirming = SimpleUnifiedInput.Confirm;
        }

        if (!MenuRoot.activeSelf) return;

        bool notCare;
        Vector2 move = SimpleUnifiedInput.VirtualPrimaryStick(0.3f, out notCare);
        if (move.y == 0)
        {
            isMoving = false;
            return;
        }
        if (!isMoving || Time.timeSinceLevelLoad - lastMove > timeBetween)
        {
            int nextActive = selectedIndex + (move.y > 0 ? -1 : 1);
            SetSelected(nextActive);
            isMoving = true;
            lastMove = Time.timeSinceLevelLoad;
        }
    }
}
