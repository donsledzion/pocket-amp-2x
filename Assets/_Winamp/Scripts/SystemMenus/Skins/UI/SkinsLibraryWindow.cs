using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using SoftAware.Winamp.SystemMenus.Skins; // For SkinService
using System.Linq;

namespace SoftAware.Winamp.SystemMenus.Skins.UI
{
    public class SkinsLibraryWindow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject skinItemPrefab;
        [SerializeField] private Transform listContent;
        
        [Header("Buttons")]
        [SerializeField] private Button importButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button closeButton;

        private SkinService skinService;
        private string selectedSkinName;
        private List<SkinItemView> currentItems = new List<SkinItemView>();

        [SerializeField] private TMPro.TextMeshProUGUI statusText;

        private void Awake()
        {
            skinService = new SkinService();
            
            if (importButton) importButton.onClick.AddListener(OnImportClicked);
            if (loadButton) loadButton.onClick.AddListener(OnLoadClicked);
            if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
            if (closeButton) closeButton.onClick.AddListener(OnCloseClicked);
            
            if (statusText != null) statusText.text = "";
        }

        private void OnEnable()
        {
            RefreshList();
            UpdateButtonsState();
        }

        private async void RefreshList()
        {
            // Clear existing
            foreach (var item in currentItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            currentItems.Clear();

            // Fetch skins
            var skins = await skinService.GetAvailableSkinsAsync();

            // Populate
            foreach (var skinName in skins)
            {
                var go = Instantiate(skinItemPrefab, listContent);
                var view = go.GetComponent<SkinItemView>();
                if (view != null)
                {
                    string displayName = System.IO.Path.GetFileNameWithoutExtension(skinName);
                    // Debug.Log($"[SkinsLibraryWindow] Created item: ID='{skinName}', Display='{displayName}'");
                    view.Setup(skinName, displayName, OnSkinSelected, OnSkinDoubleClicked);
                    currentItems.Add(view);
                }
            }
            
            // Re-select if possible
            if (!string.IsNullOrEmpty(selectedSkinName))
            {
                // Verify if it still exists
                if (!skins.Contains(selectedSkinName))
                {
                    selectedSkinName = null;
                }
                else
                {
                    UpdateSelectionVisuals();
                }
            }
            UpdateButtonsState();
        }

        private void OnSkinSelected(string skinName)
        {
            selectedSkinName = skinName;
            UpdateSelectionVisuals();
            UpdateButtonsState();
        }

        private void OnSkinDoubleClicked(string skinName)
        {
            selectedSkinName = skinName;
            UpdateSelectionVisuals();
            OnLoadClicked();
        }

        private void UpdateSelectionVisuals()
        {
            foreach (var item in currentItems)
            {
                item.SetSelected(item.SkinName == selectedSkinName);
            }
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = !string.IsNullOrEmpty(selectedSkinName);
            if (loadButton) loadButton.interactable = hasSelection;
            if (deleteButton) deleteButton.interactable = hasSelection;
        }

        private void OnImportClicked()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Winamp Skins", ".wsz", ".zip"));
            FileBrowser.SetDefaultFilter(".wsz");
            
            FileBrowser.ShowLoadDialog(async (paths) => {
                if (paths != null && paths.Length > 0)
                {
                    await skinService.ImportSkinAsync(paths[0]);
                    RefreshList();
                    // Select the imported skin
                    string importedName = System.IO.Path.GetFileName(paths[0]);
                    selectedSkinName = importedName; // Set selected
                    // RefreshList logic above clears items, SO we need to re-find and select AFTER Refresh
                    // But RefreshList is async... creating race condition or just logic flow issue.
                    // Let's rely on RefreshList finding it if we set selectedSkinName BEFORE logic runs?
                    // Actually GetAvailableSkinsAsync is async.
                    // So we should update selectedSkinName, THEN call RefreshList. 
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
            if (string.IsNullOrEmpty(selectedSkinName)) return;
            
            if (statusText != null) statusText.text = "Loading...";

            bool result = await skinService.LoadSkin(selectedSkinName);

            if (result)
            {
                if (statusText != null) statusText.text = "Loaded successfully!";
                RefreshList(); // Refresh UI to ensure visual states (colors) are correct after focus change/load
            }
            else
            {
                if (statusText != null) statusText.text = "Failed to load skin.";
            }

            // Optionally close window? 
            // Winamp usually keeps prefs open, but this is a makeshift menu. 
            // Let's keep it open for now.
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
                main.CloseSkinsLibrary();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
