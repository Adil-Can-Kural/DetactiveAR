using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections; // Coroutine için
using static UnityEngine.UIElements.UxmlAttributeDescription;


#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class AR_Experience_Manager : MonoBehaviour
{
    [Header("AR Sistem Referanslarý")]
    public ARSession arSession;
    public GameObject xrOrigin; // ARPlaceObjectOnPlane bunun üzerinde veya bir çocuðu olmalý

    // 3D Görüntüleyici ve mod deðiþtirme ile ilgili tüm deðiþkenler kaldýrýldý.
    // UI Manager referansý da artýk burada gerekmeyebilir, eðer sadece AR ise.

    IEnumerator Start()
    {
        Debug.Log("AR_Experience_Manager (Basit): Start ÇAÐRILDI.");

        if (arSession == null || xrOrigin == null)
        {
            Debug.LogError("AR_Experience_Manager (Basit): ARSession veya XR Origin atanmamýþ! Script düzgün çalýþmayabilir.");
            enabled = false;
            yield break;
        }

        // Baþlangýçta XR Origin'i deaktif edelim, sonra izin ve uygunluk kontrolünden sonra aktif edelim
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
                Debug.LogError("Kamera izni VERÝLMEDÝ! AR Modu baþlatýlamayacak.");
                // Burada kullanýcýya bir UI mesajý gösterip ana menüye döndürmek veya uygulamayý kapatmak düþünülebilir.
                // Þimdilik sadece AR'ý baþlatmamayý tercih ediyoruz.
                yield break; // AR baþlatýlamaz, çýk
            }
            else Debug.Log("Kamera izni verildi.");
        }
        else Debug.Log("Kamera izni zaten var.");
#endif

        Debug.Log("AR Kullanýlabilirlik kontrolü baþlatýlýyor...");
        // XR Origin'i ve ARSession'ý aktif et ki state kontrol edilebilsin
        if (!arSession.gameObject.activeSelf) arSession.gameObject.SetActive(true);
        if (!xrOrigin.activeSelf) xrOrigin.SetActive(true);

        yield return null; // Bir frame bekle ki ARSession state'i güncellenebilsin

        if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.LogError($"Cihaz AR desteklemiyor veya kurulum/güncelleme gerekli. ARSession.state: {ARSession.state}. AR Modu baþlatýlamýyor.");
            // Burada da kullanýcýya bir mesaj gösterip ana menüye dönmek iyi olur.
            xrOrigin.SetActive(false); // AR'ý tekrar kapat
                                       // AR_UIManager üzerinden bir hata mesajý gösterme fonksiyonu çaðrýlabilir.
            yield break; // AR baþlatýlamaz, çýk
        }
        else
        {
            Debug.Log($"Cihaz AR destekliyor görünüyor. ARSession.state: {ARSession.state}. AR Modu etkinleþtiriliyor.");
            // XR Origin zaten aktif edildi. ARPlaceObjectOnPlane scripti çalýþmaya baþlamalý.
            // ARSession.Reset() çaðrýsý genellikle XR Origin aktif edildiðinde ve session hazýrsa ARFoundation tarafýndan yönetilir.
            // Gerekiyorsa, spesifik bir durumda resetlemek için koþullu olarak çaðrýlabilir.
            // Örneðin, bir hatadan sonra veya modu tekrar baþlatýrken. Þimdilik gerekmeyebilir.
        }
        Debug.Log("AR_Experience_Manager (Basit): Start TAMAMLANDI. AR Sistemi aktif olmalý.");
    }

    // Sahneden çýkarken AR'ý düzgünce kapatmak için
    public void PrepareToLeaveScene()
    {
        Debug.Log("Sahneden çýkýþa hazýrlanýlýyor, AR kapatýlýyor...");
        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false); // Bu, içindeki ARPlaneManager, ARPlaceObjectOnPlane vb. deaktif eder.
        }
        // ARSession'ý da deaktif etmek veya durdurmak iyi bir pratik olabilir.
        // if (arSession != null && arSession.subsystem != null && arSession.subsystem.running)
        // {
        //     arSession.subsystem.Stop();
        // }
        // veya
        // if (arSession != null) arSession.gameObject.SetActive(false);
    }
   
}