using UnityEngine;
using UnityEngine.UI;

public class PulseEffect : MonoBehaviour
{
    public float speed = 1.5f;
    public Color baseColor = Color.white; // Inspector'dan efekt g�rselinin ana rengini atay�n
    public Color highlightColor = Color.white; // Titre�imin ula�aca�� en parlak renk (beyaz veya a��k sar� iyi olabilir)
    [Range(0f, 1f)] // Alfa'n�n ne kadar de�i�ece�ini kontrol etmek i�in
    public float minAlpha = 0.3f;
    [Range(0f, 1f)]
    public float maxAlpha = 0.5f;

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("PulseEffect: Image bile�eni bulunamad�!", gameObject);
            enabled = false;
            return;
        }
        // Base color'� Inspector'dan al ama Alfas�n� ba�lang�� min alfas� yap
        baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, minAlpha);
        img.color = baseColor;
    }

    void Update()
    {
        if (img == null) return;

        // Sin�s dalgas�n� 0 ile 1 aras�na getir
        float sinValue = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // 0..1

        // Renkler aras�nda interpolasyon yap
        Color targetColor = Color.Lerp(baseColor, highlightColor, sinValue);

        // Alfa aras�nda interpolasyon yap (iste�e ba�l�, sabit de tutabilirsiniz)
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, sinValue);

        // Son rengi ata
        img.color = new Color(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
    }
}