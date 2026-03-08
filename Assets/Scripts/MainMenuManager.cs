using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }



    public void QuitGame()
    {
        Debug.Log("Quit Game solicitado...");

// En el Editor de Unity detenemos el modo Play.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

// En WebGL no es posible cerrar la pestaña desde el código del juego.
#elif UNITY_WEBGL
        
        // Application.OpenURL("about:blank"); // Ejemplo (no cierra la pestaña, solo redirige).

// Para builds de escritorio/móvil normales:
#else
        Application.Quit();
#endif

    }
}
