using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] int letterPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    //singleton, since it is static, can reference any class since it will be used frequently
    public static DialogManager Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    Dialog dialog;
    Action onDialogFinished;
    int currentLine = 0;
    // So user cannot skip the dialog
    bool isTyping;
    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null) {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public void HandleUpdate() {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping) {
            ++currentLine; 
             if (currentLine < dialog.Lines.Count) {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
             } else {
                currentLine = 0;
                IsShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog();
             }
        }
    }

    public IEnumerator TypeDialog(string line) {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
        isTyping = false;
    }
}