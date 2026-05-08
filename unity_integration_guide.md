# Unity Integration Guide: Winamp Skin API

Ten dokument opisuje, jak zintegrować backend `skins-library` z Twoją aplikacją w Unity.

## Podstawowe informacje
- **Base URL**: `http://twój-serwer:8000/api`
- **Format danych**: JSON
- **Format skórek**: `.wsz` (de facto plik ZIP)

---

## Endpointy API

### 1. Pobieranie listy skórek
`GET /skins?page=1&limit=20`

Zwraca listę skórek z paginacją.
**Pole `thumbnail_url` zawiera bezpośredni link do obrazka podglądu (PNG/JPG).**

### 2. Pobieranie losowej skórki
`GET /skins/random`

### 3. Pobieranie pliku skórki
`GET /skins/{id}/download`

Ten endpoint streamuje bezpośrednio plik `.wsz`.

---

## Przykład implementacji w Unity (C#)

### Pobieranie i wyświetlanie listy skórek

```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SkinData {
    public string id;
    public string title;
    public string thumbnail_url;
    public string download_url;
}

[System.Serializable]
public class SkinListResponse {
    public List<SkinData> items;
    public int total;
    public int page;
}

public class SkinGallery : MonoBehaviour {
    private string baseUrl = "http://localhost:8000/api/skins";

    IEnumerator Start() {
        yield return FetchSkins(1);
    }

    IEnumerator FetchSkins(int page) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{baseUrl}?page={page}")) {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                SkinListResponse response = JsonUtility.FromJson<SkinListResponse>(webRequest.downloadHandler.text);
                foreach (var skin in response.items) {
                    Debug.Log($"Found skin: {skin.title}");
                    StartCoroutine(DownloadPreview(skin.thumbnail_url));
                }
            }
        }
    }

    IEnumerator DownloadPreview(string url) {
        using (UnityWebRequest loader = UnityWebRequestTexture.GetTexture(url)) {
            yield return loader.SendWebRequest();
            if (loader.result == UnityWebRequest.Result.Success) {
                Texture2D texture = DownloadHandlerTexture.GetContent(loader);
                // Przypisz teksturę do UI (np. RawImage)
            }
        }
    }
}
```

### Pobieranie i zapisywanie pliku .wsz

```csharp
IEnumerator DownloadSkin(string skinId) {
    string url = $"{baseUrl}/{skinId}/download";
    string savePath = Path.Combine(Application.persistentDataPath, "skins", $"{skinId}.wsz");

    using (UnityWebRequest www = UnityWebRequest.Get(url)) {
        string dir = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        www.downloadHandler = new DownloadHandlerFile(savePath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            Debug.Log($"Skin saved to: {savePath}");
            // Tutaj możesz rozpakować plik za pomocą SharpZipLib lub System.IO.Compression
        }
    }
}
```

## Tips dla Mobile Dev'a
1. **Caching miniatur**: Nie pobieraj miniatur za każdym razem. Unity's `UnityWebRequestTexture` nie cache'uje plików na dysku automatycznie. Rozważ własny system cache'owania obrazków.
2. **Rozpakowywanie**: Pliki `.wsz` to standardowe archiwa ZIP. W Unity najlepiej użyć `System.IO.Compression` (dostępne w nowszych wersjach .NET) lub wtyczki `SharpZipLib`.
3. **Persistent Path**: Zapisuj skórki w `Application.persistentDataPath`, aby nie zostały usunięte przy aktualizacji aplikacji.
4. **Fallback**: Zawsze miej jedną "domyślną" skórkę zaszytą w folderze `StreamingAssets` na wypadek braku połączenia z siecią.

---

## Known Issues & Workarounds

### [TEMP] Archive.org 404 Error
Obecnie API zwraca błędne adresy URL dla niektórych skórek z Archive.org (powtórzony prefix w nazwie pliku). 
**Status**: Załatane tymczasowo w `SkinService.cs`. 
**Zalecenie**: Po poprawieniu backendu należy usunąć sekcję `FIXME: [QUICK-FIX]` z kodu Unity.
