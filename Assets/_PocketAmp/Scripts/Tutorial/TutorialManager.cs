using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware.PocketAmp.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Alpaccino Setup")]
        [SerializeField] private AlpaccinoController alpaccinoPrefab;
        [SerializeField] private Transform alpaccinoParent;
        [SerializeField] private RectTransform introSpawnPoint;

        [Header("References")]
        [SerializeField] private RectTransform initialOptionsButton;
        [SerializeField] private RectTransform initialOptionsSpawnPoint;
        [SerializeField] private SoftAware.PocketAmp.SystemMenus.Skins.UI.SkinsLibraryWindow skinsWindow;
        [SerializeField] private string expectedSkinName;
        [SerializeField] private float startupDelay = 2.0f;

        [Header("Localization Texts")]
        [SerializeField] private LocalizedString textIntro1;
        [SerializeField] private LocalizedString textIntro2;
        [SerializeField] private LocalizedString textOops;
        [SerializeField] private LocalizedString textOptions;
        [SerializeField] private LocalizedString textSkinsLib;
        [SerializeField] private LocalizedString textWebToggle;
        [SerializeField] private LocalizedString textSearch;
        [SerializeField] private LocalizedString textSelectSkin;
        [SerializeField] private LocalizedString textDownload;
        [SerializeField] private LocalizedString textWait;
        [SerializeField] private LocalizedString textClose;

        private AlpaccinoController activeAlpaccino;
        private Dictionary<TutorialTargetType, TutorialTarget> targets = new Dictionary<TutorialTargetType, TutorialTarget>();
        
        public bool IsTutorialActive { get; private set; }
        private int currentStep = -2;

#if UNITY_EDITOR
        private const string EditorPrefKey = "PocketAmp_ShowTutorial";

        [MenuItem("PocketAmp/Show Tutorial")]
        public static void ToggleTutorialFlag()
        {
            bool currentState = EditorPrefs.GetBool(EditorPrefKey, false);
            EditorPrefs.SetBool(EditorPrefKey, !currentState);
            Debug.Log($"[TutorialManager] Force show tutorial is now: {!currentState}");
        }

        [MenuItem("PocketAmp/Show Tutorial", true)]
        public static bool ToggleTutorialFlagValidate()
        {
            Menu.SetChecked("PocketAmp/Show Tutorial", EditorPrefs.GetBool(EditorPrefKey, false));
            return true;
        }
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private async void Start()
        {
            if (skinsWindow != null)
            {
                skinsWindow.OnWindowOpened += AdvanceToWebToggle;
                skinsWindow.OnWebModeActivated += AdvanceToSearch;
                skinsWindow.OnWebSkinsLoaded += HandleSkinsLoaded;
                skinsWindow.OnPreviewLoaded += AdvanceToDownload;
                skinsWindow.OnDownloadStarted += AdvanceToWait;
                skinsWindow.OnSkinLoadedSuccessfully += AdvanceToClose;
            }

            await Awaitable.WaitForSecondsAsync(startupDelay);
            CheckAndStartTutorial();
        }

        private void OnDestroy()
        {
            if (skinsWindow != null)
            {
                skinsWindow.OnWindowOpened -= AdvanceToWebToggle;
                skinsWindow.OnWebModeActivated -= AdvanceToSearch;
                skinsWindow.OnWebSkinsLoaded -= HandleSkinsLoaded;
                skinsWindow.OnPreviewLoaded -= AdvanceToDownload;
                skinsWindow.OnDownloadStarted -= AdvanceToWait;
                skinsWindow.OnSkinLoadedSuccessfully -= AdvanceToClose;
            }
        }

        private void CheckAndStartTutorial()
        {
            bool forceStart = false;
#if UNITY_EDITOR
            forceStart = EditorPrefs.GetBool(EditorPrefKey, false);
#endif

            bool hasCompleted = PlayerPrefs.GetInt("Tutorial_Skin_Completed", 0) == 1;
            bool isDefaultSkin = SettingsManager.Instance != null && string.IsNullOrEmpty(SettingsManager.Instance.LastSkinPath); // Assuming empty or null is default skin

            if (forceStart || (!hasCompleted && isDefaultSkin))
            {
                StartTutorial();
            }
        }

        public void StartTutorial()
        {
            if (IsTutorialActive) return;
            Debug.Log("[TUTORIAL LOG] StartTutorial called. Tutorial is now ACTIVE.");
            IsTutorialActive = true;
            AdvanceToIntro1();
        }

        private void InstantiateAlpaccino()
        {
            if (alpaccinoPrefab != null)
            {
                activeAlpaccino = Instantiate(alpaccinoPrefab, alpaccinoParent != null ? alpaccinoParent : transform);
            }
        }

        private async void AdvanceToIntro1()
        {
            currentStep = -1;
            if (activeAlpaccino == null) InstantiateAlpaccino();
            if (activeAlpaccino != null)
            {
                string text = textIntro1 != null ? textIntro1.GetLocalizedString() : "";
                if (string.IsNullOrEmpty(text)) text = "Jestem Alpaccino i pomogę zmienić skórkę na coś ciekawszego.";
                activeAlpaccino.Show(null, introSpawnPoint, text, ArrowDirection.None);
            }
            await UnityEngine.Awaitable.WaitForSecondsAsync(4f);
            if (IsTutorialActive && currentStep == -1) AdvanceToIntro2();
        }

        private async void AdvanceToIntro2()
        {
            currentStep = 0;
            if (activeAlpaccino != null)
            {
                string text = textIntro2 != null ? textIntro2.GetLocalizedString() : "";
                if (string.IsNullOrEmpty(text)) text = "W każdej chwili możesz się mnie pozbyć przyciskiem poniżej.";
                activeAlpaccino.PointToTarget(null, introSpawnPoint, text, ArrowDirection.None);
            }
            await UnityEngine.Awaitable.WaitForSecondsAsync(4f);
            if (IsTutorialActive && currentStep == 0) AdvanceToOptions();
        }

        public void RegisterTarget(TutorialTarget target)
        {
            if (target == null || target.TargetType == TutorialTargetType.None) return;
            targets[target.TargetType] = target;
            Debug.Log($"[TUTORIAL LOG] Target Registered: {target.TargetType}");

            // If a target registers that we are waiting for, update immediately
            if (IsTutorialActive)
            {
                RefreshCurrentStep();
            }
        }

        private TutorialTargetType GetTargetTypeForStep(int step)
        {
            switch (step)
            {
                case 1: return TutorialTargetType.OptionsButton;
                case 2: return TutorialTargetType.SkinsLibraryButton;
                case 3: return TutorialTargetType.WebToggle;
                case 4: return TutorialTargetType.SearchField;
                case 5: return TutorialTargetType.FirstSkinItem;
                case 6: return TutorialTargetType.DownloadButton;
                case 7: return TutorialTargetType.DownloadButton;
                case 8: return TutorialTargetType.CloseButton;
                default: return TutorialTargetType.None;
            }
        }

        private async void RestartTutorialFromOops()
        {
            currentStep = -99;
            if (activeAlpaccino != null)
            {
                string text = textOops != null ? textOops.GetLocalizedString() : "";
                if (string.IsNullOrEmpty(text)) text = "Ups, chcesz spróbować jeszcze raz?";
                activeAlpaccino.Show(null, introSpawnPoint, text, ArrowDirection.None);
            }
            await UnityEngine.Awaitable.WaitForSecondsAsync(3.5f);
            if (IsTutorialActive && currentStep == -99) AdvanceToOptions();
        }

        public void UnregisterTarget(TutorialTarget target)
        {
            if (target != null && targets.ContainsKey(target.TargetType) && targets[target.TargetType] == target)
            {
                targets.Remove(target.TargetType);
                Debug.Log($"[TUTORIAL LOG] Target Unregistered: {target.TargetType}");
                
                if (IsTutorialActive && currentStep == 8 && target.TargetType == TutorialTargetType.CloseButton)
                {
                    Debug.Log("[TUTORIAL LOG] Ostatni cel (CloseButton) zniknął (okno zamknięte). Automatycznie kończę samouczek.");
                    Dismiss();
                }
                else if (IsTutorialActive && target.TargetType == GetTargetTypeForStep(currentStep))
                {
                    Debug.Log($"[TUTORIAL LOG] Wymagany cel {target.TargetType} zniknął na kroku {currentStep}. Restartuję samouczek.");
                    RestartTutorialFromOops();
                }
            }
        }

        private void GetTargetInfo(TutorialTargetType type, out RectTransform targetRect, out RectTransform spawnPoint)
        {
            targetRect = null;
            spawnPoint = null;

            if (type == TutorialTargetType.OptionsButton)
            {
                targetRect = initialOptionsButton;
                spawnPoint = initialOptionsSpawnPoint;
                return;
            }

            if (targets.TryGetValue(type, out var target))
            {
                targetRect = target.RectTransform;
                spawnPoint = target.SpawnPoint;
            }
        }

        public void Dismiss()
        {
            if (!IsTutorialActive) return;
            Debug.Log("[TUTORIAL LOG] Dismiss called. Ending tutorial.");

            IsTutorialActive = false;
            PlayerPrefs.SetInt("Tutorial_Skin_Completed", 1);
            PlayerPrefs.Save();

            if (activeAlpaccino != null)
            {
                activeAlpaccino.Dismiss(initialOptionsButton);
            }
        }

        private void RefreshCurrentStep()
        {
            switch (currentStep)
            {
                case 1: AdvanceToOptions(); break;
                case 2: AdvanceToSkinsLibraryButton(); break;
                case 3: AdvanceToWebToggle(); break;
                case 4: AdvanceToSearch(); break;
                case 5: AdvanceToSelectSkin(); break;
                case 6: AdvanceToDownload(); break;
                case 7: AdvanceToWait(); break;
                case 8: AdvanceToClose(); break;
            }
        }

        public void AdvanceToOptions()
        {
            if (!IsTutorialActive) return;
            currentStep = 1;
            Debug.Log($"[TUTORIAL LOG] AdvanceToOptions (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.OptionsButton, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textOptions.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Kliknij tutaj, aby otworzyć opcje!";
                activeAlpaccino.Show(rect, spawn, text, ArrowDirection.Up);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToOptions FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToSkinsLibraryButton()
        {
            if (!IsTutorialActive || currentStep > 2) return;
            currentStep = 2;
            Debug.Log($"[TUTORIAL LOG] AdvanceToSkinsLibraryButton (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.SkinsLibraryButton, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textSkinsLib.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Wybierz 'Open Skins Library'!";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Left);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToSkinsLibraryButton FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToWebToggle()
        {
            if (!IsTutorialActive || currentStep > 3) return;
            currentStep = 3;
            Debug.Log($"[TUTORIAL LOG] AdvanceToWebToggle (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.WebToggle, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textWebToggle.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Przełącz się na przeglądarkę skórek online!";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Up);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToWebToggle FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToSearch()
        {
            if (!IsTutorialActive || currentStep > 4) return;
            currentStep = 4;
            Debug.Log($"[TUTORIAL LOG] AdvanceToSearch (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.SearchField, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textSearch.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Wpisz tutaj np. 'retro', aby znaleźć coś fajnego!";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Up);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToSearch FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToSelectSkin(RectTransform actualTarget = null)
        {
            if (!IsTutorialActive || currentStep > 5) return;
            currentStep = 5;
            Debug.Log($"[TUTORIAL LOG] AdvanceToSelectSkin (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.FirstSkinItem, out var rect, out var spawn);
            
            var target = actualTarget != null ? actualTarget : rect;

            if (target != null && activeAlpaccino != null)
            {
                string text = textSelectSkin.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "O! Wybierz tę skórkę z listy!";
                activeAlpaccino.PointToTarget(target, spawn, text, ArrowDirection.Left);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToSelectSkin FAILED: target is null? {target == null}");
            }
        }

        private void HandleSkinsLoaded()
        {
            if (!IsTutorialActive || currentStep > 5) return;
            
            if (skinsWindow != null && string.IsNullOrEmpty(skinsWindow.SearchText))
            {
                Debug.Log("[TUTORIAL LOG] HandleSkinsLoaded: Otrzymano wynik, ale SearchText jest pusty. Zatrzymuję się na kroku wyszukiwarki.");
                return;
            }
            
            Debug.Log($"[TUTORIAL LOG] HandleSkinsLoaded: Otrzymano wynik. SearchText = '{(skinsWindow != null ? skinsWindow.SearchText : "")}'");
            
            RectTransform targetRect = null;
            if (skinsWindow != null && skinsWindow.CurrentItems.Count > 0)
            {
                var targetItem = skinsWindow.CurrentItems[0];
                if (!string.IsNullOrEmpty(expectedSkinName))
                {
                    bool found = false;
                    foreach (var item in skinsWindow.CurrentItems)
                    {
                        if (item.DisplayName != null && item.DisplayName.IndexOf(expectedSkinName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            targetItem = item;
                            found = true;
                            Debug.Log($"[TUTORIAL LOG] HandleSkinsLoaded: Znaleziono pasującą skórkę po nazwie: {item.DisplayName}");
                            break;
                        }
                    }
                    if (!found)
                    {
                        Debug.Log($"[TUTORIAL LOG] HandleSkinsLoaded: Na obecnej liście nie ma skórki pasującej do '{expectedSkinName}'. Celuję w pierwszą pozycję z brzegu.");
                    }
                }
                targetRect = targetItem.GetComponent<RectTransform>();
            }
            else
            {
                Debug.Log("[TUTORIAL LOG] HandleSkinsLoaded: Lista skórek jest pusta. Czekam dalej na krok 4.");
                return;
            }

            Canvas.ForceUpdateCanvases();
            AdvanceToSelectSkin(targetRect);
        }

        public void AdvanceToDownload()
        {
            if (!IsTutorialActive || currentStep > 6) return;
            currentStep = 6;
            Debug.Log($"[TUTORIAL LOG] AdvanceToDownload (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.DownloadButton, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textDownload.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Teraz kliknij Pobierz!";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Down);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToDownload FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToWait()
        {
            if (!IsTutorialActive || currentStep > 7) return;
            currentStep = 7;
            Debug.Log($"[TUTORIAL LOG] AdvanceToWait (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.DownloadButton, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textWait.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Pobieram... poczekaj chwilę!";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Down);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToWait FAILED: rect is null? {rect == null}");
            }
        }

        public void AdvanceToClose()
        {
            if (!IsTutorialActive || currentStep > 8) return;
            currentStep = 8;
            Debug.Log($"[TUTORIAL LOG] AdvanceToClose (Step {currentStep}) called.");
            
            GetTargetInfo(TutorialTargetType.CloseButton, out var rect, out var spawn);
            if (rect != null && activeAlpaccino != null)
            {
                string text = textClose.GetLocalizedString();
                if (string.IsNullOrEmpty(text)) text = "Świetnie! Twoja skórka została załadowana. Kliknij tu, by zamknąć okno.";
                activeAlpaccino.PointToTarget(rect, spawn, text, ArrowDirection.Up);
            }
            else
            {
                Debug.Log($"[TUTORIAL LOG] AdvanceToClose FAILED: rect is null? {rect == null}");
            }
        }
    }
}
