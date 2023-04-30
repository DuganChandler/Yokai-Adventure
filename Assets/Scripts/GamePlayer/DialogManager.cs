using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject dialogBox;

    [Header("Dialog")]
    [SerializeField] TextMeshProUGUI dialogText;

    [Header("Dialog Speed")]
    [SerializeField] int letterPerSecond;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;

    //singleton, since it is static, can reference any class since it will be used frequently
    public static DialogManager Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true) {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        AudioManager.i.PlaySfx(AudioID.UISelect);
        yield return TypeDialog(text);
        if (waitForInput) {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        if (autoClose) {
            CloseDialog();
        }

        OnDialogFinished?.Invoke();
    }

    public void CloseDialog() {
        dialogBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog) {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            AudioManager.i.PlaySfx(AudioID.UISelect);
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogFinished?.Invoke();
    }

    public void HandleUpdate() {
        
    }

    public IEnumerator TypeDialog(string line) {
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
    }
}