using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaatParcaMover : MonoBehaviour
{
    public Transform hedefNokta;
    public float hiz = 2f;
    private bool hareketEt = false;
    private bool hedefeUlasti = false;

    public void BaslatHareket(Transform hedef)
    {
        hedefNokta = hedef;
        hareketEt = true;
        Debug.Log("🚀 Hedefe gitme başladı...");
    }

    void Update()
    {
        if (!hareketEt || hedefNokta == null || hedefeUlasti) return;

        transform.position = Vector3.Lerp(transform.position, hedefNokta.position, Time.deltaTime * hiz);
        transform.rotation = Quaternion.Slerp(transform.rotation, hedefNokta.rotation, Time.deltaTime * hiz);

        if (Vector3.Distance(transform.position, hedefNokta.position) < 0.01f)
        {
            hareketEt = false;
            hedefeUlasti = true;
            Debug.Log("✅ Parça hedefe ulaştı! 3 saniye sonra sahne geçilecek.");
            StartCoroutine(GecikmeliTebrik());
        }
    }

    IEnumerator GecikmeliTebrik()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("TebrikSahnesi");
    }
}
