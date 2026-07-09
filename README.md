# Windows Right-Click Video Trimmer

Select Language:
* [English](#english)
* [Türkçe](#türkçe)
* [Español](#español)
* [Deutsch](#deutsch)
* [Français](#français)
* [Русский](#русский)
* [简体中文](#简体中文)

---

## English

A **lightweight, blazing-fast, and system-friendly** video cutting/trimming application designed to integrate directly into the Windows right-click context menu. 

### Key Features
* ⚡ **Lossless Trimming:** Trims videos in **under a second** without re-encoding (`-c copy`), preserving 100% original quality.
* 🟢 **Resolution Auto-Correction:** Automatically detects non-compliant custom resolutions (like 1070px heights) which cause green/white line visual artifacts in decoders, and crops them to the nearest multiple of 8 (with hardware NVENC or fast CPU re-encoding).
* 🎨 **Modern Dark UI:** Premium, fluent WPF UI featuring rounded corners, dark aesthetics, violet accents, and smooth animations.
* 🔗 **Shell Integration:** One-click integration adds a "Videoyu Kes (Hızlı)" / "Trim Video" option to your Windows Explorer right-click menu (no admin rights required).
* ⌨️ **Editor Shortcuts:** Professional shortcuts (Space, I, O, Arrow Keys) for precise frame seeking and marker positioning.
* 🎥 **Codec-free Playback Fallback:** Includes a fallback mode using `ffplay.exe` to preview high-end codecs (HEVC/H.265/MKV) even if Windows doesn't have the codecs installed.

### Installation
1. Download the latest **`TrimApp.exe`** from the Releases page.
2. Run it. If `ffmpeg.exe` and `ffplay.exe` are missing, simply click **"Otomatik Yükle" (Auto Install)** in the app to download them instantly.
3. Click **"Sağ Tıka Ekle" (Add to Right-Click)** to register it in your Windows explorer.

### Shortcuts
* **Space**: Play / Pause
* **I**: Set Start (In Point)
* **O**: Set End (Out Point)
* **Arrow keys (← / →)**: Scrub 2 seconds backward / forward

### Compilation
Compile this single-file C# application directly using Windows' built-in .NET compiler (`csc.exe`):
```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /r:"C:\Windows\Microsoft.NET\assembly\GAC_64\PresentationCore\v4.0_4.0.0.0__31bf3856ad364e35\PresentationCore.dll" /r:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\PresentationFramework\v4.0_4.0.0.0__31bf3856ad364e35\PresentationFramework.dll" /r:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\WindowsBase\v4.0_4.0.0.0__31bf3856ad364e35\WindowsBase.dll" /r:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Xaml\v4.0_4.0.0.0__b77a5c561934e089\System.Xaml.dll" /r:System.dll /r:System.Core.dll /target:winexe /out:TrimApp.exe TrimApp.cs
```

---

## Türkçe

Windows işletim sisteminde videolara sağ tıkladığınızda hızlıca kırpma/kesme yapabilmeniz için geliştirilmiş **hafif, hızlı ve sistem dostu** bir araçtır.

### Özellikler
- ⚡ **Kayıpsız (Lossless) Kesim:** Yeniden kodlama (re-encoding) yapmadığı için video boyutu ne olursa olsun **1 saniyenin altında** işlemi tamamlar ve görüntü kalitesini korur.
- 🟢 **Yeşil Çizgi ve Çözünürlük Düzeltme:** 1070px gibi 8 veya 16'nın katı olmayan standart dışı çözünürlüklerde oynatıcılarda oluşan yeşil/beyaz çizgi piksellenme hatasını otomatik tespit eder ve kırparak (`crop`) yeniden kodlar.
- 🎨 **Modern Karanlık Tema (WPF):** Akıcı animasyonlar, yuvarlatılmış köşeler ve modern karanlık arayüz.
- 🔗 **Sağ Tık Entegrasyonu:** Herhangi bir video dosyasına sağ tıkladığınızda "Videoyu Kes (Hızlı)" menüsü belirir (Yönetici yetkisi gerektirmez).
- ⌨️ **Klavye Kısayolları:** Hızlı önizleme ve kesim noktalarını belirleme için profesyonel video kurgu kısayolları.
- 🎥 **Kodeksiz Önizleme Yardımı:** HEVC (H.265) veya MKV gibi bilgisayarda kodeki olmayan videoları `ffplay.exe` kullanarak kayıpsız ve hızlı bir şekilde önizler.

### Kurulum
1. Sürümler (Releases) sayfasından güncel **`TrimApp.exe`** dosyasını indirin.
2. Uygulamayı çalıştırın. Eğer `ffmpeg.exe` ve `ffplay.exe` eksikse, arayüzdeki **"Otomatik Yükle"** butonuna basarak dosyaları saniyeler içinde otomatik kurun.
3. **"Sağ Tıka Ekle"** butonuna tıklayarak Windows sağ tık menünüze entegre edin.

### Kısayollar
- **Boşluk (Space)**: Oynat / Duraklat
- **I**: Başlangıç noktasını (In Point) ayarla
- **O**: Bitiş noktasını (Out Point) ayarla
- **Yön Tuşları (← / →)**: 2 saniye geri / ileri sar

---

## Español

Una aplicación de corte y recorte de video **ligera, ultrarrápida y amigable con el sistema**, diseñada para integrarse directamente en el menú contextual de clic derecho de Windows.

### Características Principales
* ⚡ **Recorte sin Pérdida:** Recorta videos en **menos de un segundo** sin volver a codificar (`-c copy`), preservando el 100% de la calidad original.
* 🟢 **Autocorrección de Resolución:** Detecta resoluciones personalizadas no estándar (como 1070px de alto) que causan líneas verdes/blancas en los reproductores y las recorta al múltiplo de 8 más cercano (con NVENC por hardware o CPU).
* 🎨 **UI Oscura Moderna:** Interfaz de usuario WPF premium con esquinas redondeadas, estética oscura, acentos violetas y animaciones fluidas.
* 🔗 **Integración con el Explorador:** Integración con un solo clic para añadir la opción "Videoyu Kes (Hızlı)" / "Recortar Video" al menú de clic derecho.
* ⌨️ **Atajos de Teclado:** Atajos profesionales (Espacio, I, O, Flechas) para una navegación precisa de fotogramas.

### Atajos
* **Espacio**: Reproducir / Pausar
* **I**: Definir Inicio (In Point)
* **O**: Definir Fin (Out Point)
* **Flechas (← / →)**: Retroceder / Avanzar 2 segundos

---

## Deutsch

Eine **leichte, extrem schnelle und systemschonende** Anwendung zum Schneiden von Videos, die sich direkt in das Windows-Kontextmenü (Rechtsklick) integriert.

### Hauptmerkmale
* ⚡ **Verlustfreies Schneiden:** Schneidet Videos in **weniger als einer Sekunde** ohne Neucodierung (`-c copy`), wobei die Originalqualität zu 100% erhalten bleibt.
* 🟢 **Automatische Auflösungskorrektur:** Erkennt nicht-standardisierte Auflösungen (z.B. 1070px Höhe), die grüne/weiße Linienartefakte verursachen, und schneidet sie auf das nächste Vielfache von 8 zu.
* 🎨 **Moderne dunkle Benutzeroberfläche:** Premium WPF-UI mit abgerundeten Ecken, dunkler Ästhetik und flüssigen Animationen.
* 🔗 **Kontextmenü-Integration:** Fügt dem Windows Explorer-Rechtsklick-Menü die Option "Videoyu Kes (Hızlı)" hinzu.
* ⌨️ **Tastatur-Kurzbefehle:** Professionelle Shortcuts (Leertaste, I, O, Pfeiltasten) für präzise Markerplatzierung.

### Tastaturkürzel
* **Leertaste**: Wiedergabe / Pause
* **I**: Startpunkt setzen (In Point)
* **O**: Endpunkt setzen (Out Point)
* **Pfeiltasten (← / →)**: 2 Sekunden zurück / vor springen

---

## Français

Une application de découpe vidéo **légère, ultra-rapide et respectueuse du système**, conçue pour s'intégrer directement dans le menu contextuel du clic droit de Windows.

### Caractéristiques Principales
* ⚡ **Découpe sans Perte:** Découpe les vidéos en **moins d'une seconde** sans réencodage (`-c copy`), préservant 100% de la qualité d'origine.
* 🟢 **Correction Auto de Résolution:** Détecte les résolutions non standard (comme 1070px de hauteur) provoquant des lignes vertes ou blanches, et les recadre au multiple de 8 le plus proche.
* 🎨 **Interface Sombre Moderne:** UI WPF élégante avec des coins arrondis, un design sombre haut de gamme et des animations fluides.
* 🔗 **Intégration Windows:** Ajoute l'option "Videoyu Kes (Hızlı)" au menu clic droit de l'Explorateur Windows en un clic.
* ⌨️ **Raccourcis Clavier:** Raccourcis professionnels (Espace, I, O, Flèches) pour définir précisément les points de coupe.

### Raccourcis
* **Espace**: Lecture / Pause
* **I**: Définir le début (Point In)
* **O**: Définir la fin (Point Out)
* **Flèches (← / →)**: Reculer / Avancer de 2 secondes

---

## Русский

**Легкое, сверхбыстрое и не требовательное к системе** приложение для обрезки видео, интегрируемое прямо в контекстное меню проводника Windows (правый клик).

### Ключевые особенности
* ⚡ **Обрезка без потери качества:** Выполняет нарезку видео **быстрее секунды** без перекодирования (`-c copy`), сохраняя 100% оригинального качества.
* 🟢 **Автокоррекция разрешения:** Обнаруживает нестандартные разрешения (например, высоту 1070px), вызывающие зеленые или белые полосы на видео, и автоматически обрезает их до ближайшего кратного 8 значения.
* 🎨 **Современный темный интерфейс:** Премиальный WPF-интерфейс со скругленными углами, плавной анимацией и фиолетовыми акцентами.
* 🔗 **Интеграция в контекстное меню:** Добавление опции «Videoyu Kes (Hızlı)» в контекстное меню проводника Windows одним кликом.
* ⌨️ **Горячие клавиши:** Профессиональные сочетания клавиш (Пробел, I, O, стрелки) для точного позиционирования.

### Сочетания клавиш
* **Пробел**: Воспроизведение / Пауза
* **I**: Установить точку начала (In)
* **O**: Установить точку конца (Out)
* **Стрелки (← / →)**: Назад / Вперед на 2 секунды

---

## 简体中文

一款**轻量、极速且系统友好**的的视频裁剪/剪切工具，直接集成至 Windows 右键上下文菜单中。

### 核心功能
* ⚡ **无损剪切：** 无需重新编码即可在**一秒内**完成视频裁剪 (`-c copy`)，100% 保留原始画质。
* 🟢 **分辨率自动修正：** 自动检测导致播放器中出现绿色或白色线条伪影的非标准分辨率（如 1070px 高度），并将其裁剪为最接近的 8 的倍数（支持 NVENC 硬件加速或 CPU 编码）。
* 🎨 **现代暗黑 UI：** 高端、流畅的 WPF 界面，具有圆角、暗黑美学、紫罗兰配色和细腻的动效。
* 🔗 **右键菜单集成：** 一键添加“Videoyu Kes (Hızlı)”（快速剪切视频）选项至 Windows 资源管理器右键菜单。
* ⌨️ **快捷键支持：** 专业的快捷键组合（空格键、I、O、方向键）可实现精确的帧定位和标记设置。

### 快捷键
* **空格键**：播放 / 暂停
* **I**：设置入点（起点）
* **O**：设置出点（终点）
* **方向键 (← / →)**：向后 / 向前快进 2 秒
