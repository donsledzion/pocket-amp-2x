using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Threading;
using PrimeTween;

namespace SoftAware.PocketAmp.SystemMenus.Skins.UI
{
    public class SkinsLibraryWindow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkinItemView skinItemPrefab;
        [SerializeField] private Transform listContent;
        
        [Header("Buttons")]
        [SerializeField] private Button importButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button closeButton;

        [Header("Web Mode")]
        [SerializeField] private Toggle localToggle;
        [SerializeField] private Toggle webToggle;
        [SerializeField] private TMPro.TMP_InputField searchField;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private GameObject previewPlaceholder;
        [SerializeField] private Button loadMoreButtonPrefab;
        [SerializeField] private Button closePreviewButton;
        [SerializeField] private Button previewOverlayButton;
        [SerializeField] private Button webDownloadButton;
        [SerializeField] private TMPro.TextMeshProUGUI webDownloadButtonText;
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private GameObject errorOverlay;
        [SerializeField] private TMPro.TextMeshProUGUI errorText;
        private Button loadMoreButtonInstance;

        [SerializeField] private TMPro.TextMeshProUGUI statusText;

        [Header("Localization")]
        [SerializeField] private UnityEngine.Localization.LocalizedString foundLocalSkinsText;
        [SerializeField] private UnityEngine.Localization.LocalizedString loadingMoreText;
        [SerializeField] private UnityEngine.Localization.LocalizedString fetchingWebText;
        [SerializeField] private UnityEngine.Localization.LocalizedString errorApiText;
        [SerializeField] private UnityEngine.Localization.LocalizedString showingWebSkinsText;
        [SerializeField] private UnityEngine.Localization.LocalizedString loadingPreviewText;
        [SerializeField] private UnityEngine.Localization.LocalizedString btnLoadText;
        [SerializeField] private UnityEngine.Localization.LocalizedString btnDownloadText;
        [SerializeField] private UnityEngine.Localization.LocalizedString btnCancelText;
        [SerializeField] private UnityEngine.Localization.LocalizedString loadingLocalText;
        [SerializeField] private UnityEngine.Localization.LocalizedString loadedSuccessText;
        [SerializeField] private UnityEngine.Localization.LocalizedString downloadingText;
        [SerializeField] private UnityEngine.Localization.LocalizedString downloadCancelledText;
        [SerializeField] private UnityEngine.Localization.LocalizedString downloadedLoadingText;
        [SerializeField] private UnityEngine.Localization.LocalizedString failedLoadText;

        private SkinService skinService;
        private string selectedSkinName;
        private SkinData selectedWebSkin;
        private List<SkinItemView> currentItems = new List<SkinItemView>();
        private List<SkinData> webSkins = new List<SkinData>();
        
        private int currentWebPage = 1;
        private int totalWebSkins = 0;
        private CancellationTokenSource previewCts;
        private CancellationTokenSource downloadCts;
        private bool isDownloading = false;
        
        private bool isWebMode => webToggle != null && webToggle.isOn;
        private bool isRefreshing = false;

        private void Awake()
        {
            skinService = new SkinService();
            
            if (importButton) importButton.onClick.AddListener(OnImportClicked);
            if (loadButton) loadButton.onClick.AddListener(OnLoadClicked);
            if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
            if (closeButton) closeButton.onClick.AddListener(OnCloseClicked);
            
            if (localToggle) localToggle.onValueChanged.AddListener((val) => { if(val) RefreshList(); });
            if (webToggle) webToggle.onValueChanged.AddListener((val) => { if(val) RefreshList(); });
            if (searchField) searchField.onValueChanged.AddListener(OnSearchChanged);
            if (closePreviewButton) closePreviewButton.onClick.AddListener(ClosePreview);
            if (previewOverlayButton) previewOverlayButton.onClick.AddListener(ClosePreview);
            if (webDownloadButton) webDownloadButton.onClick.AddListener(OnLoadClicked);

            if (statusText != null) statusText.text = "";
            if (previewPlaceholder) previewPlaceholder.SetActive(true);
            if (previewImage) previewImage.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            RefreshList();
            UpdateButtonsState();
            ClearStatus();
            
            if (SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
            {
                SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToWebToggle();
            }
        }

        private void ClearStatus() => statusText.text = "";

        private async void RefreshList()
        {
            if (isRefreshing) return;
            isRefreshing = true;

            // Wait one frame to let UI initialize
            await Awaitable.NextFrameAsync();

            Debug.Log($"[SkinsLibraryWindow] Refreshing list. Target content: {listContent?.name}", listContent);
            foreach (var item in currentItems)
                if (item) Destroy(item.gameObject);
            currentItems.Clear();

            // Fetch skins
            if (isWebMode)
            {
                currentWebPage = 1;
                webSkins.Clear();
                await RefreshWebList(false);
                
                if (SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
                {
                    SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToSearch();
                }
            }
            else
            {
                await RefreshLocalList();
            }
            isRefreshing = false;
        }

        private async Awaitable RefreshLocalList()
        {
            var skins = await skinService.GetAvailableSkinsAsync();
            if (skins == null) return;
            
            if (statusText) 
            {
                if (foundLocalSkinsText != null && !foundLocalSkinsText.IsEmpty)
                {
                    foundLocalSkinsText.Arguments = new object[] { skins.Count };
                    statusText.text = foundLocalSkinsText.GetLocalizedString();
                }
                else
                {
                    statusText.text = $"Found {skins.Count} local skins";
                }
            }

            foreach (var skinName in skins)
            {
                var view = Instantiate(skinItemPrefab, listContent, false);
                if (!view) continue;
                
                view.gameObject.SetActive(true);
                view.transform.localScale = Vector3.one;

                var displayName = System.IO.Path.GetFileNameWithoutExtension(skinName);
                view.Setup(skinName, displayName, OnSkinSelected, OnSkinDoubleClicked);
                currentItems.Add(view);
            }
            
            UpdateButtonsState();
            UpdateLoadMoreButtonVisibility(); // Force hide in local mode
        }

        private async Awaitable RefreshWebList(bool append = false)
        {
            if (statusText)
            {
                if (append)
                    statusText.text = (loadingMoreText != null && !loadingMoreText.IsEmpty) ? loadingMoreText.GetLocalizedString() : "Loading more...";
                else
                    statusText.text = (fetchingWebText != null && !fetchingWebText.IsEmpty) ? fetchingWebText.GetLocalizedString() : "Fetching web skins...";
            }
            var query = searchField ? searchField.text : "";
            var response = await skinService.GetWebSkinsAsync(query, currentWebPage);
            
            if (response == null || response.items == null)
            {
                if (statusText) statusText.text = (errorApiText != null && !errorApiText.IsEmpty) ? errorApiText.GetLocalizedString() : "Error: Invalid API response";
                return;
            }

            if (!append) webSkins.Clear();
            webSkins.AddRange(response.items);
            totalWebSkins = response.total;

            if (statusText) 
            {
                if (showingWebSkinsText != null && !showingWebSkinsText.IsEmpty)
                {
                    showingWebSkinsText.Arguments = new object[] { webSkins.Count, totalWebSkins };
                    statusText.text = showingWebSkinsText.GetLocalizedString();
                }
                else
                {
                    statusText.text = $"Showing {webSkins.Count} of {totalWebSkins} skins";
                }
            }

            foreach (var skin in response.items)
            {
                var view = Instantiate(skinItemPrefab, listContent, false);
                if (!view) continue;

                view.gameObject.SetActive(true);
                view.transform.localScale = Vector3.one;

                view.Setup(skin.id, skin.title, OnSkinSelected, OnSkinDoubleClicked);
                currentItems.Add(view);
            }

            UpdateButtonsState();
            UpdateLoadMoreButtonVisibility();

            if (currentItems.Count > 0 && SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
            {
                SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToSelectSkin();
            }
        }

        public async void LoadNextPage()
        {
            if (isRefreshing || webSkins.Count >= totalWebSkins) return;
            
            isRefreshing = true;
            currentWebPage++;
            await RefreshWebList(true);
            isRefreshing = false;
        }

        private void UpdateLoadMoreButtonVisibility()
        {
            bool show = isWebMode && webSkins.Count < totalWebSkins;

            if (show)
            {
                if (loadMoreButtonInstance == null && loadMoreButtonPrefab != null)
                {
                    loadMoreButtonInstance = Instantiate(loadMoreButtonPrefab, listContent);
                    loadMoreButtonInstance.onClick.AddListener(LoadNextPage);
                }

                if (loadMoreButtonInstance != null)
                {
                    loadMoreButtonInstance.gameObject.SetActive(true);
                    loadMoreButtonInstance.transform.SetAsLastSibling();
                    loadMoreButtonInstance.transform.localScale = Vector3.one;
                }
            }
            else
            {
                if (loadMoreButtonInstance != null)
                {
                    loadMoreButtonInstance.gameObject.SetActive(false);
                }
            }
        }

        private void OnSearchChanged(string query)
        {
            // Simple debounce could be added here, but for now just refresh on change
            if (isWebMode) RefreshList();
        }

        private async void OnSkinSelected(string id)
        {
            if (isWebMode)
            {
                // Cancel previous preview loading
                previewCts?.Cancel();
                previewCts?.Dispose();
                previewCts = new CancellationTokenSource();

                selectedWebSkin = webSkins.Find(s => s.id == id);
                selectedSkinName = null;
                
                // Update visuals IMMEDIATELY
                UpdateSelectionVisuals();
                UpdateButtonsState();

                if (SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
                {
                    SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToDownload();
                }

                await UpdatePreview(previewCts.Token);
            }
            else
            {
                selectedSkinName = id;
                selectedWebSkin = null;
                if (previewImage) previewImage.gameObject.SetActive(false);
                if (previewPlaceholder) previewPlaceholder.SetActive(true);
                
                UpdateSelectionVisuals();
                UpdateButtonsState();
            }
        }

        private async Awaitable UpdatePreview(CancellationToken token)
        {
            if (selectedWebSkin == null) return;
            
            if (previewPlaceholder) previewPlaceholder.SetActive(false);
            if (previewImage)
            {
                // Reset image
                previewImage.gameObject.SetActive(true);
                previewImage.texture = null;
                previewImage.transform.localScale = Vector3.zero;
                if (previewOverlayButton) previewOverlayButton.gameObject.SetActive(true);

                string originalStatus = statusText ? statusText.text : "";
                if (statusText) statusText.text = (loadingPreviewText != null && !loadingPreviewText.IsEmpty) ? loadingPreviewText.GetLocalizedString() : "Loading preview...";

                try 
                {
                    // Disable buttons during animation
                    if (closePreviewButton) closePreviewButton.interactable = false;
                    if (webDownloadButton) webDownloadButton.interactable = false;

                    var texture = await skinService.GetTextureAsync(selectedWebSkin.thumbnail_url, token);
                    
                    if (token.IsCancellationRequested) return;

                    if (texture) 
                    {
                        previewImage.texture = texture;
                        // PrimeTween Animation (No bounce)
                        await Tween.Scale(previewImage.transform, 0f, 1f, duration: 0.5f, ease: Ease.OutQuad);
                    }

                    if (statusText) statusText.text = originalStatus;
                    
                    // Re-enable buttons
                    if (closePreviewButton) closePreviewButton.interactable = true;
                    if (webDownloadButton) webDownloadButton.interactable = true;
                }
                catch (System.OperationCanceledException)
                {
                    // Ignore cancellation
                }
            }
        }

        private async void ClosePreview()
        {
            previewCts?.Cancel();
            
            if (previewImage && previewImage.gameObject.activeSelf)
            {
                // Disable buttons during animation
                if (closePreviewButton) closePreviewButton.interactable = false;
                if (webDownloadButton) webDownloadButton.interactable = false;

                // Animate out
                await Tween.Scale(previewImage.transform, 1f, 0f, duration: 0.3f, ease: Ease.InQuad);
                previewImage.gameObject.SetActive(false);
            }

            if (previewPlaceholder) previewPlaceholder.SetActive(true);
            if (previewOverlayButton) previewOverlayButton.gameObject.SetActive(false);
            
            // Restore button state
            if (closePreviewButton) closePreviewButton.interactable = true;
            if (webDownloadButton) webDownloadButton.interactable = true;
        }

        private void UpdateSelectionVisuals()
        {
            var id = isWebMode ? (selectedWebSkin?.id) : selectedSkinName;
            foreach (var item in currentItems)
            {
                item.SetSelected(item.SkinName == id);
            }
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = isWebMode ? (selectedWebSkin != null) : !string.IsNullOrEmpty(selectedSkinName);
            
            if (loadButton)
            {
                // Hide main load button in Web mode
                loadButton.gameObject.SetActive(!isWebMode);
                loadButton.interactable = hasSelection;
                var text = loadButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text) text.text = (btnLoadText != null && !btnLoadText.IsEmpty) ? btnLoadText.GetLocalizedString() : "Load";
            }

            if (deleteButton)
            {
                // Hide main delete button in Web mode
                deleteButton.gameObject.SetActive(!isWebMode);
                deleteButton.interactable = hasSelection;
            }

            if (webDownloadButton)
            {
                // Only show download button in Web mode when a skin is selected
                webDownloadButton.gameObject.SetActive(isWebMode && selectedWebSkin != null);
                webDownloadButton.interactable = selectedWebSkin != null;
                
                if (webDownloadButtonText && isWebMode && selectedWebSkin != null)
                {
                    bool alreadyDownloaded = skinService.IsSkinDownloaded(selectedWebSkin.id);
                    string loadStr = (btnLoadText != null && !btnLoadText.IsEmpty) ? btnLoadText.GetLocalizedString() : "Load";
                    string downloadStr = (btnDownloadText != null && !btnDownloadText.IsEmpty) ? btnDownloadText.GetLocalizedString() : "Download";
                    string cancelStr = (btnCancelText != null && !btnCancelText.IsEmpty) ? btnCancelText.GetLocalizedString() : "Cancel";
                    webDownloadButtonText.text = isDownloading ? cancelStr : (alreadyDownloaded ? loadStr : downloadStr);
                }
            }

            if (importButton) importButton.gameObject.SetActive(!isWebMode);
            if (searchField) searchField.gameObject.SetActive(isWebMode);
        }

        private void OnSkinDoubleClicked(string id)
        {
            OnSkinSelected(id);
            OnLoadClicked();
        }

        private void OnImportClicked()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Skins", ".wsz", ".zip"));
            FileBrowser.SetDefaultFilter(".wsz");
            
            FileBrowser.ShowLoadDialog(async (paths) => {
                if (paths != null && paths.Length > 0)
                {
                    await skinService.ImportSkinAsync(paths[0]);
                    RefreshList();
                    string importedName = System.IO.Path.GetFileName(paths[0]);
                    selectedSkinName = importedName; // Set selected
                    // RefreshList will re-fetch and then check selectedSkinName.
                }
            }, 
            null, 
            FileBrowser.PickMode.Files, 
            false, 
            null, 
            null, 
            "Import Skin", 
            "Import");
        }

        private async void OnLoadClicked()
        {
            if (isWebMode)
            {
                if (selectedWebSkin == null) return;

                // Handle Cancel if already downloading
                if (isDownloading)
                {
                    downloadCts?.Cancel();
                    return;
                }

                // Check if already downloaded
                bool alreadyDownloaded = skinService.IsSkinDownloaded(selectedWebSkin.id);
                
                if (alreadyDownloaded)
                {
                    if (statusText) statusText.text = (loadingLocalText != null && !loadingLocalText.IsEmpty) ? loadingLocalText.GetLocalizedString() : "Loading local skin...";
                    var fileName = $"{selectedWebSkin.id}.wsz";
                    bool result = await skinService.LoadSkin(fileName);
                    if (result)
                    {
                        if (statusText) statusText.text = (loadedSuccessText != null && !loadedSuccessText.IsEmpty) ? loadedSuccessText.GetLocalizedString() : "Loaded successfully!";
                        ClosePreview();
                    }
                    else
                    {
                        string failStr = (failedLoadText != null && !failedLoadText.IsEmpty) ? failedLoadText.GetLocalizedString() : "Failed to load skin.";
                        await ShowErrorAndClose(failStr);
                    }
                    return;
                }

                // Start download
                isDownloading = true;
                string cancelBtnStr = (btnCancelText != null && !btnCancelText.IsEmpty) ? btnCancelText.GetLocalizedString() : "Cancel";
                if (webDownloadButtonText) webDownloadButtonText.text = cancelBtnStr;
                if (loadingSpinner) loadingSpinner.SetActive(true);
                if (statusText) statusText.text = (downloadingText != null && !downloadingText.IsEmpty) ? downloadingText.GetLocalizedString() : "Downloading...";
                
                if (SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
                {
                    SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToWait();
                }

                downloadCts = new CancellationTokenSource();

                try 
                {
                    var fileName = await skinService.DownloadWebSkinAsync(selectedWebSkin, downloadCts.Token);
                    
                    if (downloadCts.Token.IsCancellationRequested)
                    {
                        if (statusText) statusText.text = (downloadCancelledText != null && !downloadCancelledText.IsEmpty) ? downloadCancelledText.GetLocalizedString() : "Download cancelled.";
                        ClosePreview();
                        return;
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (statusText) statusText.text = (downloadedLoadingText != null && !downloadedLoadingText.IsEmpty) ? downloadedLoadingText.GetLocalizedString() : "Downloaded! Loading...";
                        bool result = await skinService.LoadSkin(fileName);
                        if (result)
                        {
                            if (statusText) statusText.text = (loadedSuccessText != null && !loadedSuccessText.IsEmpty) ? loadedSuccessText.GetLocalizedString() : "Loaded successfully!";
                            ClosePreview();
                            
                            if (SoftAware.PocketAmp.Tutorial.TutorialManager.Instance != null)
                            {
                                SoftAware.PocketAmp.Tutorial.TutorialManager.Instance.AdvanceToClose();
                            }
                        }
                        else
                        {
                            string failStr = (failedLoadText != null && !failedLoadText.IsEmpty) ? failedLoadText.GetLocalizedString() : "Failed to load skin.";
                            await ShowErrorAndClose(failStr);
                        }
                    }
                    else
                    {
                        await ShowErrorAndClose("Download failed.");
                    }
                }
                finally
                {
                    isDownloading = false;
                    string downloadBtnStr = (btnDownloadText != null && !btnDownloadText.IsEmpty) ? btnDownloadText.GetLocalizedString() : "Download";
                    if (webDownloadButtonText) webDownloadButtonText.text = downloadBtnStr;
                    if (loadingSpinner) loadingSpinner.SetActive(false);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(selectedSkinName)) return;
                
                if (statusText != null) statusText.text = (loadingLocalText != null && !loadingLocalText.IsEmpty) ? loadingLocalText.GetLocalizedString() : "Loading...";

                bool result = await skinService.LoadSkin(selectedSkinName);

                if (result)
                {
                    if (statusText != null) statusText.text = (loadedSuccessText != null && !loadedSuccessText.IsEmpty) ? loadedSuccessText.GetLocalizedString() : "Loaded successfully!";
                    RefreshList(); 
                }
                else
                {
                    if (statusText != null) statusText.text = (failedLoadText != null && !failedLoadText.IsEmpty) ? failedLoadText.GetLocalizedString() : "Failed to load skin.";
                }
            }
        }

        private async Awaitable ShowErrorAndClose(string message)
        {
            if (errorOverlay) errorOverlay.SetActive(true);
            if (errorText) errorText.text = message;
            
            await Awaitable.WaitForSecondsAsync(2f);
            
            if (errorOverlay) errorOverlay.SetActive(false);
            ClosePreview();
        }

        private async void OnDeleteClicked()
        {
             if (string.IsNullOrEmpty(selectedSkinName)) return;

             // TODO: Confirmation Dialog (as per architecture, but stripped down for now as IDialogService is not fully implemented yet)
             // Implementing direct delete for MVP as requested in task list.
             
             await skinService.DeleteSkinAsync(selectedSkinName);
             selectedSkinName = null;
             RefreshList();
        }

        private void OnCloseClicked()
        {
            // This assumes Main.cs controls visibility or self-disable
            gameObject.SetActive(false); 
            // OR call Main.CloseSkinsWindow() if wired up.
            // Since this component is ON the object that gets disabled, SetActive(false) works for self-closing.
            // But Main.cs might need to know state.
            // Ideally we call Main.CloseSkinsWindow().
            
            var main = FindAnyObjectByType<Main>(); // Inefficient but safe for now
            if (main != null)
            {
                main.OverlayWindowsController.CloseSkinsLibrary();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
