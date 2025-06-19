# 🔧 Kurulum Rehberi (Geliştiriciler İçin)

Bu rehber, "Dedektif Pati" projesini kendi bilgisayarınızda kurup çalıştırmanız için gerekli adımları içerir.

## Gereksinimler

*   **Unity Hub**
*   **Unity 2022.3.4f1** (veya projenin geliştirildiği spesifik versiyon)
*   **Git**
*   **Android Build Support** ve/veya **iOS Build Support** modülleri (Unity Hub üzerinden eklenir)
*   Mobil cihazda test için **Android SDK/NDK** veya **Xcode** (iOS için)

## Kurulum Adımları

1.  **Projeyi Klonlama:**
    Bu repoyu bilgisayarınıza klonlamak için bir terminal veya komut istemi açın ve aşağıdaki komutu çalıştırın:
    ```bash
    git clone https://github.com/kullanici-adiniz/repo-adiniz.git
    ```
    *(Not: `kullanici-adiniz/repo-adiniz.git` kısmını kendi GitHub repo adresinizle değiştirin.)*

2.  **Projeyi Unity Hub'da Açma:**
    *   Unity Hub'ı açın.
    *   "Open" -> "Add project from disk" seçeneğini seçin.
    *   1. adımda klonladığınız proje klasörünü bulun ve seçin.
    *   Projenin doğru Unity versiyonuyla açıldığından emin olun.

3.  **Gerekli Paketleri Yükleme:**
    *   Proje Unity'de açıldığında, `Window` -> `Package Manager` menüsüne gidin.
    *   Package Manager penceresinde, aşağıdaki paketlerin yüklü ve güncel olduğundan emin olun:
        *   `AR Foundation`
        *   `ARCore XR Plugin` (Android için)
        *   `ARKit XR Plugin` (iOS için)
        *   `Input System`
        *   `TextMeshPro`
    *   Eğer eksik bir paket varsa, Package Manager üzerinden aratıp "Install" butonuna basarak yükleyebilirsiniz.

## Derleme ve Çalıştırma (Build and Run)

1.  **Build Settings'i Açın:**
    *   `File` -> `Build Settings...` menüsüne gidin.

2.  **Platformu Seçin:**
    *   Derleme yapmak istediğiniz platformu (Android veya iOS) seçin ve "Switch Platform" butonuna tıklayın.

3.  **Sahneleri Ekleyin:**
    *   "Scenes In Build" listesinin boş olmadığından emin olun. Gerekirse, Project panelindeki `Scenes` klasöründen oyun sahnelerinizi (Intro, MainMenu, ARScene vb.) sürükleyip bu listeye bırakın. Başlangıç sahnesinin (genellikle Intro) en üstte (index 0) olduğundan emin olun.

4.  **Player Settings'i Yapılandırın (Android Örneği):**
    *   "Player Settings..." butonuna tıklayın.
    *   `Player` -> `Company Name` ve `Product Name` alanlarını doldurun.
    *   `Other Settings` altında:
        *   `Graphics APIs`: Sadece `OpenGLES3`'ün olduğundan emin olun (mobil AR uyumluluğu için önerilir).
        *   `Package Name`: Benzersiz bir paket adı girin (örn: `com.sirketadiniz.dedektifpati`).
        *   `Minimum API Level`: ARCore destekleyen bir seviye seçin (örn: API Level 24 veya üstü).
    *   `XR Plug-in Management` altında, seçtiğiniz platform için `ARCore` (Android) veya `ARKit` (iOS) kutucuğunun işaretli olduğunu kontrol edin.

5.  **Derleyin:**
    *   Test cihazınızı USB ile bilgisayarınıza bağlayın.
    *   Build Settings penceresinde "Build And Run" butonuna tıklayın. Unity, projeyi derleyip doğrudan cihazınıza yükleyecek ve çalıştıracaktır.

---
**[Ana Sayfaya Dön](./README.md)**
