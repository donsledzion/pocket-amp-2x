# Winamp 2.x Skin Runtime Importer (Unity Android)

## Cel

Umożliwić użytkownikowi: - wybór pliku `.wsz` - pobranie skina z URL -
automatyczne rozpakowanie - konwersję BMP - wycięcie sprite w runtime -
zastosowanie do UI

Bez Unity Editor. 100% runtime.

------------------------------------------------------------------------

# Architektura

    SkinManager
     ├── PickLocalFile()
     ├── DownloadFromUrl(string url)
     ├── ImportWsz(string path)
     ├── Unpack()
     ├── LoadTextures()
     ├── Slice()
     ├── Apply()

------------------------------------------------------------------------

# 1. Wybór pliku (.wsz) -- Android

Rekomendowany plugin: UnityNativeFilePicker

Przykład użycia:

``` csharp
using UnityEngine;

public void PickSkinFile()
{
    NativeFilePicker.PickFile((path) =>
    {
        if (path == null)
            return;

        ImportWsz(path);
    }, new string[] { "wsz" });
}
```

------------------------------------------------------------------------

# 2. Pobieranie z URL

``` csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public IEnumerator DownloadSkin(string url)
{
    string fileName = Path.GetFileName(url);
    string savePath = Path.Combine(Application.persistentDataPath, fileName);

    using (UnityWebRequest www = UnityWebRequest.Get(url))
    {
        www.downloadHandler = new DownloadHandlerFile(savePath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ImportWsz(savePath);
        }
        else
        {
            Debug.LogError("Download failed: " + www.error);
        }
    }
}
```

------------------------------------------------------------------------

# 3. Import i rozpakowanie .wsz

``` csharp
using System.IO;
using System.IO.Compression;

public void ImportWsz(string wszPath)
{
    string skinName = Path.GetFileNameWithoutExtension(wszPath);
    string outputDir = Path.Combine(Application.persistentDataPath, "Skins", skinName);

    if (Directory.Exists(outputDir))
        Directory.Delete(outputDir, true);

    Directory.CreateDirectory(outputDir);

    ZipFile.ExtractToDirectory(wszPath, outputDir);

    StartCoroutine(LoadSkin(outputDir));
}
```

------------------------------------------------------------------------

# 4. Ładowanie BMP w runtime

``` csharp
public IEnumerator LoadTexture(string path, System.Action<Texture2D> onDone)
{
    UnityWebRequest req = UnityWebRequestTexture.GetTexture("file://" + path);
    yield return req.SendWebRequest();

    if (req.result == UnityWebRequest.Result.Success)
    {
        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        RemoveMagenta(tex);
        onDone(tex);
    }
}
```

------------------------------------------------------------------------

# 5. Maskowanie koloru #FF00FF (magenta)

``` csharp
private void RemoveMagenta(Texture2D tex)
{
    Color[] pixels = tex.GetPixels();

    for (int i = 0; i < pixels.Length; i++)
    {
        if (pixels[i].r == 1f && pixels[i].g == 0f && pixels[i].b == 1f)
        {
            pixels[i].a = 0f;
        }
    }

    tex.SetPixels(pixels);
    tex.Apply();
}
```

------------------------------------------------------------------------

# 6. Wycinanie sprite

Stałe recty (layout Winamp 2.x):

``` csharp
public static class WinampRects
{
    public static Rect MainPanel = new Rect(0, 0, 275, 116);
    public static Rect PlayButton = new Rect(23, 88, 23, 18);
}
```

Tworzenie sprite:

``` csharp
public Sprite Slice(Texture2D tex, Rect rect)
{
    return Sprite.Create(
        tex,
        rect,
        new Vector2(0, 1),
        1f
    );
}
```

------------------------------------------------------------------------

# Struktura katalogów Android

    /storage/emulated/0/Android/data/your.app/files/

     ├── Skins/
     │    ├── classic/
     │    ├── darkmetal/
     │
     ├── classic.wsz

------------------------------------------------------------------------

# Dobre praktyki

-   Nie zapisuj PNG -- trzymaj tekstury w RAM
-   Nie używaj SpriteAtlas w runtime
-   Obsługuj wielkość liter nazw plików
-   Cache'uj rozpakowane skiny
-   Rozważ parsowanie viscolor.txt i pledit.txt

------------------------------------------------------------------------

# Gotowe

User: - wybiera plik LUB - podaje URL LUB - wrzuca ręcznie do folderu

App: - rozpakowuje - ładuje BMP - wycina sprite - aplikuje skin

100% runtime.
