using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using erenshor_mob_buddy.Model;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace erenshor_mob_buddy
{
    [BepInPlugin(Metadata.MOD_GUID, Metadata.MOD_NAME, Metadata.MOD_VERSION)]
    public class Mod : BaseUnityPlugin
    {
        private static Mod Instance;
        private readonly Harmony harmony = new Harmony(Metadata.MOD_GUID);
        private static List<string> TrackedMobs = new List<string>();
        const float ScanTime = 2f;
        
        GameObject _madeUI;
        float Timer = 2.1f;

        public static bool ToggleTracking(string MobName)
        {
            MobName = MobName.ToLower();
            if (TrackedMobs.Contains(MobName))
            {
                TrackedMobs.Remove(MobName);
                Instance.RefreshUI();
                return false;
            }

            TrackedMobs.Add(MobName);
            Instance.RefreshUI();
            return true;
        }

        public static void ClearTracking()
        {
            TrackedMobs.Clear();
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            harmony.PatchAll();

            Instance = this;
            DontDestroyOnLoad(gameObject);
            RefreshUI();
        }

        public void Update()
        {
            Timer += Time.deltaTime;
            if (Timer >= ScanTime)
            {
                Timer = 0f;

                RefreshUI();
            }
        }

        void RefreshUI()
        {
            CleanUI();
            var mobs = Scan();
            BuildWindow(mobs.ToList());
        }

        void CleanUI()
        {
            if (_madeUI == null)
                return;

            Destroy(_madeUI);
        }

        //Most UI code lifted and modified from https://github.com/drizzlx/Erenshor-AdvancedAuctionHouse
        void BuildWindow(List<ScanResult> FoundMobs)
        {
            var canvasGo = new GameObject("MobBuddyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;

            var group = canvasGo.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = true;

            var panelGo = new GameObject("MobBuddyPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            panelGo.transform.SetParent(canvasGo.transform, false);

            var rect = panelGo.GetComponent<RectTransform>();

            float panelWidth = 200f;
            float panelHeight = 300f;

            rect.sizeDelta = new Vector2(panelWidth, panelHeight);
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);

            rect.anchoredPosition = new Vector2(-panelWidth / 2f, 0f);
            
            panelGo.GetComponent<RawImage>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            var layoutGroup = panelGo.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandWidth = true;   // Stretch width
            layoutGroup.childForceExpandHeight = false; // Don't force height (unless you want equal height buttons)
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 1f; // space between buttons
            layoutGroup.padding = new RectOffset(10, 10, 4, 4); // ← left, right, top, bottom

            foreach (var mobScan in FoundMobs)
                AddText(mobScan.MobName, layoutGroup.transform, mobScan.Found ? Color.yellow : Color.grey);

            _madeUI = canvasGo;
        }

        void AddText(string Text, Transform Parent, Color TextColor)
        {
            var textGo = new GameObject("MobBuddyEntry", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(Parent, false);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var txt = textGo.GetComponent<Text>();
            txt.text = Text;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.color = TextColor;
            txt.fontSize = 12;

            var txtOutline = txt.gameObject.AddComponent<Outline>();
            txtOutline.effectColor = Color.black;
            txtOutline.effectDistance = new Vector2(1f, 1f);
        }

        IEnumerable<ScanResult> Scan()
        {
            IEnumerable<string> AllMobs = NPCTable.LiveNPCs.Select((x) => x.NPCName.ToLower());
            return TrackedMobs.Select((tracked) => { return new ScanResult(tracked, AllMobs.Contains(tracked)); });
        }
    }
}
