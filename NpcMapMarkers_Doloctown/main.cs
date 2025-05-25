using BepInEx;
using BepInEx.Configuration;
using DolocTown;
using DolocTown.UI;
using HarmonyLib;
using ModUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedSaw;
using UnityEngine;
using DolocTown.Config.NPC;


// using ModSettingManagerForDolocTown.API;

namespace NpcMapMarkers_Doloctown
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class NpcMapMarkers : BaseUnityPlugin
    {
        private const string ModGuid = "YourCompany.NpcMapMarkers_Doloctown";
        private const string ModName = "NpcMapMarkers_Doloctown";
        private const string ModVersion = "1.0";

        private new static readonly ModLogger Logger = new ModLogger(ModName, isDebug: true);

        //====================== 配置项字段（示例，可删改） ======================
        //private static ConfigEntry<bool> _toggleFeatureX;
        //private static ConfigEntry<int> _numericParameterY;
        //private static ConfigEntry<KeyboardShortcut> _keyActionZ;

        private const string Section = ModName;

        private void SetupConfig()
        {
            //_toggleFeatureX = Config.Bind(
            //    Section,
            //    "启用功能X",
            //    false,
            //    new ConfigDescription("启用某个核心功能", null, new ConfigurationManagerAttributes { Order = 0 })
            //);

            //var intRange = new AcceptableValueRange<int>(0, 100);
            //_numericParameterY = Config.Bind(
            //    Section,
            //    "参数Y（数值）",
            //    10,
            //    new ConfigDescription("该数值用于控制 Y 行为的强度或延迟等", intRange, new ConfigurationManagerAttributes { Order = -1 })
            //);

            //_keyActionZ = Config.Bind(
            //    Section,
            //    "触发操作Z的按键",
            //    new KeyboardShortcut(KeyCode.None),
            //    new ConfigDescription("用于触发某个特殊操作的快捷键", null, new ConfigurationManagerAttributes { Order = -2 })
            //);

            //TryRegisterWithModSettingManager();
        }

        private void TryRegisterWithModSettingManager()
        {
            Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
            if (apiType != null && apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null)
            {
                try
                {
                    Logger.Log("发现 ModSettingAPI，尝试注入设置项...");
                    dynamic api = apiType.GetMethod("Create").Invoke(null, null);

                    // 示例注册（取消注释并替换变量）
                    // api.AddToggle(_toggleFeatureX);
                    // api.AddSlider(_numericParameterY);
                    // api.AddKeyBind(_keyActionZ);
                }
                catch (Exception ex)
                {
                    Logger.Log("设置注入失败：" + ex.Message, Debug.LogError);
                }
            }
            else
            {
                Logger.Log("未找到 ModSettingAPI，跳过设置注入", Debug.LogWarning);
            }
        }

        public void Awake()
        {
            try
            {
                SetupConfig();
                Harmony.CreateAndPatchAll(typeof(NpcMapMarkers));
                Logger.Log("MOD 启动并完成 Harmony 注入！");
            }
            catch (Exception ex)
            {
                Logger.Log("初始化失败：" + ex, Debug.LogError);
            }
        }


        [HarmonyPostfix,HarmonyPatch(typeof(MapPanel), "GetAndInitMap")]
        private static void MapPanel_GetAndInitMap_Postfix(ref CityMapPanel __result)
        {
            var npcManager = DolocAPI.archiveHandle?.cityData.npcManager;
            if (npcManager == null) return;

            var npcs = npcManager.AllNpcs;

            //Logger.Log(__result.MapId);  // 当前地图id string


            //
            var npcDataManager = new NpcDataManager(__result, npcs);

            var npcList = npcDataManager.GetNpcListInCurrentMapId();
            //npcDataManager.DebugNpcInfo(npcList); // 用于验证 npcList 正确性


            var tipPool = Traverse.Create(__result).Field("tipPool").GetValue<ObjectPool<DolocNavigationButton>>();

            foreach (var npc in npcList)
            {
                // 1. 从对象池中取一个新的导航按钮
                var tip = tipPool.Next;
                tip.gameObject.SetActive(true);
                tip.position = npc.MapPosition;

                // 设置缩放
                tip.transform.localScale = Vector3.one * 0.75f;
                tip.iconImg.color = Color.green;

                // 创建 hover 文本
                TextGroup textGroup = new TextGroup(npc.OriginTitle, npc.MapPosition.ToString("F2"), string.Empty);


                var traverse = Traverse.Create(__result); // __instance 是 CityMapPanel 实例


                // 添加 hover 监听器
                tip.onPointerEnter.AddListener(delegate (int _)
                {
                    traverse.Property("latestHoveredObject").SetValue(tip.gameObject);
                    traverse.Method("AdjustCursorPosition", new object[] { tip.position }).GetValue();

                    tip.HoverText(textGroup, UIAlignmentType.TopMiddle, UIAlignmentType.BottomMiddle);
                });
                tip.onPointerExit.AddListener(delegate (int _)
                {
                    if (traverse.Property("latestHoveredObject").GetValue<GameObject>() == tip.gameObject)
                    {
                        traverse.Property("latestHoveredObject").SetValue(null);
                    }
                    tip.HideHoverBox();
                });

                // 设置 sprite（可选）
                //tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>("ui_miniicon_default");
                //tip.iconImg.color = Color.red;

                // 禁用所有悬停提示等
                //tip.onPointerEnter.RemoveAllListeners();
                //tip.onPointerExit.RemoveAllListeners();
                //tip.onClick.RemoveAllListeners();
                DolocAPI.QueryRoom(npc.Scene, out Room room);
                var percent = room.Geometry.CalPosPercent(npc.WorldPosition);
                Debug.Log($"[调试NPC] {npc.Name} in {room.RoomId}");
                Debug.Log($"WorldPos: {npc.WorldPosition}, Percent: {percent}");

                MapArea mapArea;
                var roomAreaCache = Traverse.Create(__result).Field<Dictionary<string, MapArea>>("roomAreaCache").Value;
                if (!roomAreaCache.TryGetValue(room.SceneRawName, out mapArea) && !roomAreaCache.TryGetValue(room.RoomId, out mapArea))
                {
                    return;
                }
                var uiPos = mapArea.GetPosByPercent(percent.x, percent.y);
                Debug.Log($"UI Pos: {uiPos}, MapArea pos: {mapArea.position}, size: {mapArea.size}");

            }


        }


        public class NpcDataManager
        {
            public List<Npc> OriginNpcList;
            public CityMapPanel CityMapPanelInstance;
            public List<NpcInfo> MyNpcList;
            public string CurrentMapId;
            public NpcDataManager(CityMapPanel cityMapPanel, IEnumerable<Npc> npcs)
            {
                CityMapPanelInstance = cityMapPanel;
                CurrentMapId = CityMapPanelInstance.MapId;

                OriginNpcList = npcs.ToList();
                MyNpcList = new List<NpcInfo>();

                foreach (var npc in OriginNpcList)
                {
                    var i = new NpcInfo(npc, CityMapPanelInstance);
                    MyNpcList.Add(i);
                }
            }

            // 调试所有NPC信息
            public void DebugNpcInfo(List<NpcInfo> list)
            {
                foreach (var npc in list)
                {
                    string logMessage = $@"
                    → NPC 名称: {npc.Name}
                      NPC ID: {npc.NpcId}
                      hasKnownName:{npc.HasKnownName}
                      OriginTitle:{npc.OriginTitle}
                      原型name: {npc.ProtoName}
                      场景: {npc.Scene}
                      positionWS: {npc.WorldPosition}
                      mapPos: {npc.MapPosition}
                      状态: {npc.Status}
                      shouldRunSchedule: {npc.HasSchedule}
                    ------------------------------";
                    Logger.Log(logMessage);
                }
                Logger.Log($"当前NPC列表共{list.Count} 个：", Debug.LogWarning);
            }
            // 输出当前房间的列表
            public List<NpcInfo> GetNpcListInCurrentMapId()
            {
                var result = new List<NpcInfo>();

                foreach (var npc in MyNpcList)
                {
                    if (DolocAPI.QueryRoom(npc.Scene, out Room room))
                    {
                        // room.SceneConfig.MapId 就是这个 Room 所属的 map 页面
                        if (room != null && room.SceneConfig.MapId == CurrentMapId)
                        {
                            result.Add(npc);
                        }
                    }
                }
                return result;
            }
            public class NpcInfo
            {
                public int NpcId;
                public string Name;
                public string OriginTitle;
                public string Scene;
                public string ProtoName;
                public string Status;
                public bool HasKnownName;
                public bool HasSchedule;
                public Vector2 WorldPosition;
                public Vector2 MapPosition;

                public NpcInfo(Npc npc, CityMapPanel cityMapPanel)
                {
                    NpcId = npc.NpcId;
                    Name = npc.NpcName;
                    OriginTitle = npc.OriginTitle;
                    Scene = npc.sceneName;
                    ProtoName = npc.proto?.Name ?? "null";
                    Status = npc.NpcStatusInfo?.ToString() ?? "null";
                    HasKnownName = npc.hasKnownName;
                    HasSchedule = npc.shouldRunSchedule;
                    WorldPosition = npc.positionWS;

                    cityMapPanel.GetMapPosByWorldPosition(Scene, WorldPosition, out Vector2 mapPos);
                    MapPosition = mapPos;
                }
            }
        }


    }
}
