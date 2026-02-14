# Audio Visualizer -- Unity (Android + ANA)

## Cel

Zaimplementowanie wizualizera audio (spectrum / waveform / efekty
shaderowe typu MilkDrop) w aplikacji Unity na Androidzie z
wykorzystaniem ANA (Android Native Audio).

Dokument opisuje wszystkie możliwe warianty w zależności od dostępu do
próbek PCM.

------------------------------------------------------------------------

# 1. Warianty architektoniczne

## ✅ Wariant A (REKOMENDOWANY) -- Dostęp do PCM przed wysłaniem do ANA

### Architektura

Audio Source → PCM Buffer → ├─\> ANA (Playback) └─\> FFT Analyzer → GPU
(Shader/VFX)

### Zalety

-   Najniższa latencja
-   Pełna kontrola
-   Możliwość beat detection
-   Możliwość efektów klasy MilkDrop
-   Brak zależności od Android Visualizer API

------------------------------------------------------------------------

## ⚠ Wariant B -- Brak dostępu do PCM (modyfikacja ANA)

### Rozwiązanie

Dodać callback w warstwie natywnej (C++ / Java):

onAudioBuffer(float\* data, int size)

Próbki kopiować do ring buffera i przekazywać do Unity przez JNI.

------------------------------------------------------------------------

## ❌ Wariant C -- Android Visualizer API

android.media.audiofx.Visualizer

Ograniczenia: - Ograniczona rozdzielczość FFT - Większa latencja - Może
nie działać z AAudio/OpenSL - Brak pełnej kontroli

Stosować tylko w ostateczności.

------------------------------------------------------------------------

# 2. Analiza Audio

## FFT

Rekomendowane rozmiary: - 512 -- lekki spectrum - 1024 -- standard
mobile - 2048 -- wyższa jakość

Pipeline:

PCM → Window (Hann) → FFT → Magnitude → Log Scale → Smoothing

------------------------------------------------------------------------

# 3. Beat Detection

Metoda energy-based:

1.  RMS z okna (np. 1024 próbek)
2.  Rolling average z historii
3.  Jeśli current_energy \> average_energy \* threshold → Beat

Typowe parametry: - threshold: 1.3 -- 1.6 - historia: \~1 sekunda

------------------------------------------------------------------------

# 4. Renderowanie w Unity

## Opcja 1 -- LineRenderer

Prosty spectrum analyzer.

## Opcja 2 -- Texture2D + Shader (REKOMENDOWANE)

FFT → 64 band → Texture2D → Shader generuje efekt.

## Opcja 3 -- Compute Shader / VFX Graph

Efekty cząsteczkowe i zaawansowane deformacje.

------------------------------------------------------------------------

# 5. Optymalizacja (Android)

Nie wolno: - Alokować tablic co frame - Tworzyć Texture2D w Update() -
Generować GC

Zawsze: - Reuse bufferów - Fixed FFT size - Pre-allocate arrays -
Smoothing przez Lerp

------------------------------------------------------------------------

# 6. Minimalny Pipeline Produkcyjny

CPU: 1. PCM (1024) 2. Hann 3. FFT 4. 64 pasma 5. Smoothing 6. Texture2D
update

GPU: Shader reaguje na: - \_BassLevel - \_MidLevel - \_HighLevel -
\_BeatStrength - \_Time

------------------------------------------------------------------------

# 7. Latencja

-   PCM direct: \~0--10 ms
-   ANA callback: \~10--20 ms
-   Android Visualizer API: 40--100 ms

------------------------------------------------------------------------

# 8. Decyzja

Jeśli masz dostęp do PCM → użyj Wariantu A. To jedyne rozwiązanie dające
pełną kontrolę i jakość.
