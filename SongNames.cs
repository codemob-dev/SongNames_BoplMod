using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SongNames
{
    [BepInPlugin("com.codemob.songnames", "Song Names", "1.0.0")]
    public class SongNames : BaseUnityPlugin
    {
        public Harmony harmony;
        public static TextMeshProUGUI textComponent;
        public static GameObject overlayObject;
        public static string currentText = string.Empty;
        public static SongNames instance;
        private void Awake()
        {
            instance = this;
            harmony = new Harmony(Info.Metadata.GUID);
            harmony.PatchAll(typeof(SongNames));

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;


            var audioManager_startMenuMusic = typeof(AudioManager)
                .GetMethod("StartMenuMusic", BindingFlags.NonPublic | BindingFlags.Instance);
            var audioManager_startMenuMusic_prefix = typeof(SongNames)
                .GetMethod(nameof(AudioManager_StartMenuMusic));

            harmony.Patch(audioManager_startMenuMusic, prefix: new HarmonyMethod(audioManager_startMenuMusic_prefix));


            var audioManager_startMusic = typeof(AudioManager)
                .GetMethod("StartMusic", BindingFlags.NonPublic | BindingFlags.Instance);
            var audioManager_startMusic_prefix = typeof(SongNames)
                .GetMethod(nameof(AudioManager_StartMusic));

            harmony.Patch(audioManager_startMusic, prefix: new HarmonyMethod(audioManager_startMusic_prefix));
        }

        public static void AudioManager_StartMenuMusic(AudioManager __instance)
        {
            SetCurrentPlayingSong(__instance.menuSong);
        }

        public static void AudioManager_StartMusic(ref int songIndex, AudioManager __instance)
        {
            SetCurrentPlayingSong(((__instance.test_songs != null && __instance.test_songs.Length != 0) ? __instance.test_songs : __instance.songs)[songIndex]);
        }

        public static void SetCurrentPlayingSong(Song song)
        {
            string songName = song.name;
            songName = Regex.Replace(songName, "(?<=[a-z])[A-Z]", " $&");
            currentText = $"Currently playing: {songName}";
            textComponent.text = currentText;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (overlayObject != null)
            {
                Destroy(overlayObject);
            }
            // Create a new canvas
            overlayObject = new GameObject("song text :3");
            Canvas canvas = overlayObject.AddComponent<Canvas>();
            CanvasScaler scaler = overlayObject.AddComponent<CanvasScaler>();

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.current;

            canvas.sortingLayerName = "behind Walls Infront of everything else";
            canvas.sortingOrder = 1;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // create the version info GameObject and set the parent to the canvas
            GameObject infoText = new GameObject("SongText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textComponent = infoText.GetComponent<TextMeshProUGUI>();
            infoText.transform.SetParent(canvas.transform);

            textComponent.text = currentText;

            // change settings
            textComponent.font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
            textComponent.color = Color.Lerp(Color.blue, Color.black, 0.6f);
            textComponent.fontSize = 13;

            // Allow the text to be clicked through
            textComponent.raycastTarget = false;

            // Align to bottom right
            textComponent.alignment = TextAlignmentOptions.Top;

            // set the position of the text to the bottom right of the screen
            RectTransform rectTransform = infoText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(.5f, 1f);
            rectTransform.anchorMax = new Vector2(.5f, 1f);
            rectTransform.pivot = new Vector2(.5f, .5f);
            rectTransform.sizeDelta = new Vector2(1200, 0);
            rectTransform.anchoredPosition = new Vector2(0, -2);
        }
    }
}
