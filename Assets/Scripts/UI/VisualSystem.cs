using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class VisualSystem : MonoBehaviour
{
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Color32[] stateColors; // Usar Color32 es inmensamente más barato en RAM y bus
    [SerializeField] private Color32[] colorsGreen;

    private Texture2D _texture;
    private Color32[] _pixelBuffer; // Buffer de bytes (R,G,B,A) en lugar de floats
    private bool _needsApply;

    private void Awake()
    {
        int w = gridSystem.Width;
        int h = gridSystem.Height;

        _texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        rawImage.texture = _texture;

        _pixelBuffer = new Color32[w * h];

        // Inicialización (Ruido procedural optimizado)
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int index = gridSystem.GetIndex(i, j);
                float noise = Mathf.Round(Mathf.PerlinNoise(i * 0.15f, j * 0.15f) * 2f) * 0.5f;
                _pixelBuffer[index] = Color32.Lerp(colorsGreen[0], colorsGreen[1], noise);
            }
        }

        _texture.SetPixels32(_pixelBuffer);
        _texture.Apply(false);
    }

    private void OnEnable() => gridSystem.OnHectareChanged += HandleHectareChanged;
    private void OnDisable() => gridSystem.OnHectareChanged -= HandleHectareChanged;
    private void OnDestroy() => Destroy(_texture);

    private void HandleHectareChanged(int index, StateHectare newState)
    {
        _pixelBuffer[index] = stateColors[(int)newState];
        _needsApply = true;
        // NOTA ARQUITECTÓNICA: La animación de brasas fue delegada a un Shader.
        // Asigna un Material al RawImage con un shader que haga parpadear (Lerp usando _Time.y)
        // cualquier pixel cuyo color coincida con stateColors[(int)StateHectare.Burned].
        // Esto salva la CPU completamente.
    }

    private void LateUpdate() // Más limpio que una corrutina Yield return null
    {
        if (_needsApply)
        {
            // SetPixels32 es ~3 veces más rápido que SetPixels
            _texture.SetPixels32(_pixelBuffer);
            _texture.Apply(false); // false = no regenerar mipmaps (vital para pixel art)
            _needsApply = false;
        }
    }
}