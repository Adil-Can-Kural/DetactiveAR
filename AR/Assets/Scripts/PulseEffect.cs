using UnityEngine;
using UnityEngine.UI;

public class PulseEffect : MonoBehaviour
{
    public float speed = 1.5f;
    public Color baseColor = Color.white; // Inspector'dan efekt görselinin ana rengini atayýn
    public Color highlightColor = Color.white; // Titreþimin ulaþacaðý en parlak renk (beyaz veya açýk sarý iyi olabilir)
    [Range(0f, 1f)] // Alfa'nýn ne kadar deðiþeceðini kontrol etmek için
    public float minAlpha = 0.3f;
    [Range(0f, 1f)]
    public float maxAlpha = 0.5f;

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("PulseEffect: Image bileþeni bulunamadý!", gameObject);
            enabled = false;
            return;
        }
        // Base color'ý Inspector'dan al ama Alfasýný baþlangýç min alfasý yap
        baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, minAlpha);
        img.color = baseColor;
    }

    void Update()
    {
        if (img == null) return;

        // Sinüs dalgasýný 0 ile 1 arasýna getir
        float sinValue = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // 0..1

        // Renkler arasýnda interpolasyon yap
        Color targetColor = Color.Lerp(baseColor, highlightColor, sinValue);

        // Alfa arasýnda interpolasyon yap (isteðe baðlý, sabit de tutabilirsiniz)
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, sinValue);

        // Son rengi ata
        img.color = new Color(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
    }
}