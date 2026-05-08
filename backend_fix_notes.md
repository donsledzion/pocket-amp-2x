# Bug Report: Broken Archive.org Download URLs

## Opis błędu
API generuje niepoprawne adresy URL w polu `download_url` dla skórek hostowanych na Archive.org. Powoduje to błąd **404 Not Found** podczas próby pobrania pliku przez klienta (Unity).

## Przyczyna
Błąd polega na błędnym konstruowaniu nazwy pliku w ścieżce URL. Backend niepotrzebnie powtarza prefix identyfikatora (np. `winampskin_` lub `winampskins_`) w nazwie samego pliku `.wsz`.

## Przykłady

### Przykład 1 (winampskin_)
*   **Obecny (BŁĘDNY) URL:** `https://archive.org/download/winampskin_kaliber10000/winampskin_kaliber10000.wsz`
*   **Oczekiwany (POPRAWNY) URL:** `https://archive.org/download/winampskin_kaliber10000/kaliber10000.wsz`

### Przykład 2 (winampskins_)
*   **Obecny (BŁĘDNY) URL:** `https://archive.org/download/winampskins_pd3_jennifer/winampskins_pd3_jennifer.wsz`
*   **Oczekiwany (POPRAWNY) URL:** `https://archive.org/download/winampskins_pd3_jennifer/pd3_jennifer.wsz`

## Rekomendacja naprawy
W logice generującej `download_url` należy upewnić się, że nazwa pliku (ostatni człon adresu) nie zawiera prefixu biblioteki Archive.org (`winampskin_` / `winampskins_`), jeśli jest on już obecny w nazwie folderu (identyfikatorze zasobu).

**Pseudokod poprawki:**
```php
$id = $skin->archive_id; // np. "winampskin_kaliber10000"
$cleanName = str_replace(['winampskin_', 'winampskins_'], '', $id);
$downloadUrl = "https://archive.org/download/{$id}/{$cleanName}.wsz";
```

## Status w Unity
W pliku `SkinService.cs` została zaimplementowana tymczasowa łata (`FIXME: [QUICK-FIX]`), która naprawia te linki po stronie klienta. **Po naprawieniu API prosimy o informację, abyśmy mogli usunąć ten nadmiarowy kod z aplikacji Unity.**
