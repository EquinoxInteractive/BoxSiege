using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Samples.RebindUI;

public class ExitScript : MonoBehaviour
{
    // Semua perpindahan scene di bawah ini lewat SceneTransitionManager supaya ada
    // animasi fade out -> fade in yang sama/konsisten dengan scene lainnya.
    public void ExitGame()
    {
        SaveBindingsSafely();
        SceneTransitionManager.GoToScene(0);
    }

    public void TheEarth()
    {
        SaveBindingsSafely();
        SceneTransitionManager.GoToScene(2);
    }

    public void TheHell()
    {
        SaveBindingsSafely();
        SceneTransitionManager.GoToScene(3);
    }

    private void SaveBindingsSafely()
    {
        try
        {
            // Simpan semua binding untuk semua InputActionAsset
            RebindActionUI.SaveAllBindings();
            Debug.Log("All bindings saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save bindings: " + e.Message);
        }
    }

    // Memastikan binding dimuat kembali saat scene dimulai
    private void Awake()
    {
        try
        {
            RebindActionUI.LoadAllBindingsFromPlayerPrefs();
            Debug.Log("All bindings loaded successfully on scene start.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load bindings on scene start: " + e.Message);
        }
    }
}