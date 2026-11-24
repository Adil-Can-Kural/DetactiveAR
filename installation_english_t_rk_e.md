**🌐 Language / Dil:** [English](#-installation-guide-for-developers) | [Türkçe](#-kurulum-rehberi-geliştiriciler-i̇çin)

---

# 🔧 Installation Guide (For Developers)

This guide includes the necessary steps for you to set up and run the "Detective Paw" project on your own computer.

## Requirements

*   **Unity Hub**
*   **Unity 2022.3.4f1** (or the specific version the project was developed in)
*   **Git**
*   **Android Build Support** and/or **iOS Build Support** modules (added via Unity Hub)
*   **Android SDK/NDK** or **Xcode** (for iOS) for testing on a mobile device

## Installation Steps

1.  **Clone the Project:**
    Open a terminal or command prompt to clone this repository to your computer and run the following command:
    ```bash
    git clone https://github.com/kullanici-adiniz/repo-adiniz.git
    ```
    *(Note: Replace `kullanici-adiniz/repo-adiniz.git` with your own GitHub repository address.)*

2.  **Open the Project in Unity Hub:**
    *   Open Unity Hub.
    *   Select "Open" -> "Add project from disk".
    *   Locate and select the project folder you cloned in step 1.
    *   Make sure the project is opened with the correct Unity version.

3.  **Install Required Packages:**
    *   Once the project opens in Unity, go to `Window` -> `Package Manager`.
    *   In the Package Manager window, make sure the following packages are installed and up to date:
        *   `AR Foundation`
        *   `ARCore XR Plugin` (for Android)
        *   `ARKit XR Plugin` (for iOS)
        *   `Input System`
        *   `TextMeshPro`
    *   If any package is missing, you can search for it via Package Manager and install it by clicking the "Install" button.

## Build and Run

1.  **Open Build Settings:**
    *   Go to `File` -> `Build Settings...`.

2.  **Select the Platform:**
    *   Select the platform you want to build for (Android or iOS) and click the "Switch Platform" button.

3.  **Add Scenes:**
    *   Make sure the "Scenes In Build" list is not empty. If necessary, drag and drop your game scenes (Intro, MainMenu, ARScene, etc.) from the `Scenes` folder in the Project panel into this list. Ensure the starting scene (usually Intro) is at the very top (index 0).

4.  **Configure Player Settings (Android Example):**
    *   Click the "Player Settings..." button.
    *   Fill in the `Player` -> `Company Name` and `Product Name` fields.
    *   Under `Other Settings`:
        *   `Graphics APIs`: Make sure only `OpenGLES3` is present (recommended for mobile AR compatibility).
        *   `Package Name`: Enter a unique package name (e.g., `com.yourcompany.detectivepaw`).
        *   `Minimum API Level`: Select a level that supports ARCore (e.g., API Level 24 or higher).
    *   Under `XR Plug-in Management`, check that the `ARCore` (Android) or `ARKit` (iOS) box is checked for your selected platform.

5.  **Build:**
    *   Connect your test device to your computer via USB.
    *   In the Build Settings window, click the "Build And Run" button. Unity will compile the project, install it directly onto your device, and run it.

---
**[Back to Home Page](./README.md)**

---
---

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