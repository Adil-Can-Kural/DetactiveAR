using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections; // Coroutine i�in
using static UnityEngine.UIElements.UxmlAttributeDescription;


#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class AR_Experience_Manager : MonoBehaviour
{
    [Header("AR Sistem Referanslar�")]
    public ARSession arSession;
    public GameObject xrOrigin; // ARPlaceObjectOnPlane bunun �zerinde veya bir �ocu�u olmal�

    // 3D G�r�nt�leyici ve mod de�i�tirme ile ilgili t�m de�i�kenler kald�r�ld�.
    // UI Manager referans� da art�k burada gerekmeyebilir, e�er sadece AR ise.

    IEnumerator Start()
    {
        Debug.Log("AR_Experience_Manager (Basit): Start �A�RILDI.");

        if (arSession == null || xrOrigin == null)
        {
            Debug.LogError("AR_Experience_Manager (Basit): ARSession veya XR Origin atanmam��! Script d�zg�n �al��mayabilir.");
            enabled = false;
            yield break;
        }

        // Ba�lang��ta XR Origin'i deaktif edelim, sonra izin ve uygunluk kontrol�nden sonra aktif edelim
        xrOrigin.SetActive(false);
      
#if UNITY_ANDROID
if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("Kamera izni yok, isteniyor...");
            Permission.RequestUserPermission(Permission.Camera);
            float startTime = Time.time;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && (Time.time - startTime < 15f))
            {
                Debug.Log("Kamera izni bekleniyor...");
                yield return new WaitForSeconds(0.5f);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.LogError("Kamera izni VER�LMED�! AR Modu ba�lat�lamayacak.");
                // Burada kullan�c�ya bir UI mesaj� g�sterip ana men�ye d�nd�rmek veya uygulamay� kapatmak d���n�lebilir.
                // �imdilik sadece AR'� ba�latmamay� tercih ediyoruz.
                yield break; // AR ba�lat�lamaz, ��k
            }
            else Debug.Log("Kamera izni verildi.");
        }
        else Debug.Log("Kamera izni zaten var.");
#endif

        Debug.Log("AR Kullan�labilirlik kontrol� ba�lat�l�yor...");
        // XR Origin'i ve ARSession'� aktif et ki state kontrol edilebilsin
        if (!arSession.gameObject.activeSelf) arSession.gameObject.SetActive(true);
        if (!xrOrigin.activeSelf) xrOrigin.SetActive(true);

        yield return null; // Bir frame bekle ki ARSession state'i g�ncellenebilsin

        if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.LogError($"Cihaz AR desteklemiyor veya kurulum/g�ncelleme gerekli. ARSession.state: {ARSession.state}. AR Modu ba�lat�lam�yor.");
            // Burada da kullan�c�ya bir mesaj g�sterip ana men�ye d�nmek iyi olur.
            xrOrigin.SetActive(false); // AR'� tekrar kapat
                                       // AR_UIManager �zerinden bir hata mesaj� g�sterme fonksiyonu �a�r�labilir.
            yield break; // AR ba�lat�lamaz, ��k
        }
        else
        {
            Debug.Log($"Cihaz AR destekliyor g�r�n�yor. ARSession.state: {ARSession.state}. AR Modu etkinle�tiriliyor.");
            // XR Origin zaten aktif edildi. ARPlaceObjectOnPlane scripti �al��maya ba�lamal�.
            // ARSession.Reset() �a�r�s� genellikle XR Origin aktif edildi�inde ve session haz�rsa ARFoundation taraf�ndan y�netilir.
            // Gerekiyorsa, spesifik bir durumda resetlemek i�in ko�ullu olarak �a�r�labilir.
            // �rne�in, bir hatadan sonra veya modu tekrar ba�lat�rken. �imdilik gerekmeyebilir.
        }
        Debug.Log("AR_Experience_Manager (Basit): Start TAMAMLANDI. AR Sistemi aktif olmal�.");
    }

    // Sahneden ��karken AR'� d�zg�nce kapatmak i�in
    public void PrepareToLeaveScene()
    {
        Debug.Log("Sahneden ��k��a haz�rlan�l�yor, AR kapat�l�yor...");
        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false); // Bu, i�indeki ARPlaneManager, ARPlaceObjectOnPlane vb. deaktif eder.
        }
        // ARSession'� da deaktif etmek veya durdurmak iyi bir pratik olabilir.
        // if (arSession != null && arSession.subsystem != null && arSession.subsystem.running)
        // {
        //     arSession.subsystem.Stop();
        // }
        // veya
        // if (arSession != null) arSession.gameObject.SetActive(false);
    }
   
}