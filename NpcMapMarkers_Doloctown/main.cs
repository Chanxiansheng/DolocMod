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
using static NpcMapMarkers_Doloctown.NpcDataManager;

// using ModSettingManagerForDolocTown.API;

namespace NpcMapMarkers_Doloctown
{
    public static class ModLog
    {
        public static readonly ModLogger Logger = new ModLogger("NpcMapMarkers_Doloctown", isDebug: false);
    }

    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class NpcMapMarkers : BaseUnityPlugin
    {
        private const string ModGuid = "YourCompany.NpcMapMarkers_Doloctown";
        private const string ModName = "NpcMapMarkers_Doloctown";
        private const string ModVersion = "1.0";

        private static ConfigEntry<bool> _toggleShowUnknownNpc;
        private static ConfigEntry<string> _stringIconType;

        private const string Section = "NpcMapMarkers";

        private void SetupConfig()
        {
            _toggleShowUnknownNpc = Config.Bind(
                Section,
                "显示未认识NPC",
                false,
                new ConfigDescription("显示未认识NPC", null, new ConfigurationManagerAttributes { Order = 0 })
            );


            _stringIconType = Config.Bind(
                Section,
                "图标类型",
                "Avatar",
                new ConfigDescription("图标类型", 
                    new AcceptableValueList<string>("Avatar", "Simple"), 
                    new ConfigurationManagerAttributes { Order = -1 }
                    )
            );

            TryRegisterWithModSettingManager();
        }

        private void TryRegisterWithModSettingManager()
        {
            Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
            if (apiType != null && apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null)
            {
                try
                {
                    ModLog.Logger.Log("发现 ModSettingAPI，尝试注入设置项...");
                    dynamic api = apiType.GetMethod("Create")?.Invoke(null, null);

                    if (api == null) return;

                    api.AddToggle(_toggleShowUnknownNpc);
                    api.AddDropdown(_stringIconType, new AcceptableValueList<string>("Avatar", "Simple"));

                }
                catch (Exception ex)
                {
                    ModLog.Logger.Log("设置注入失败：" + ex.Message, Debug.LogError);
                }
            }
            else
            {
                ModLog.Logger.Log("未找到 ModSettingAPI，跳过设置注入", Debug.LogWarning);
            }
        }

        public void Awake()
        {
            try
            {
                SetupConfig();
                Harmony.CreateAndPatchAll(typeof(CityMapPanelPatches));
                ModLog.Logger.Log("MOD 启动并完成 Harmony 注入！");
            }
            catch (Exception ex)
            {
                ModLog.Logger.Log("初始化失败：" + ex, Debug.LogError);
            }
        }

        public static class CityMapPanelPatches
        {
            // 1. 池初始化
            [HarmonyPostfix, HarmonyPatch(typeof(CityMapPanel), "__Init")]
            private static void CityMapPanel_Init_Postfix(CityMapPanel __instance)
            {
                ObjectPoolManager.InitPool(__instance);
            }

            // 2. 打开地图注册
            [HarmonyPostfix, HarmonyPatch(typeof(CityMapPanel), "Register")]
            private static void CityMapPanel_Register_Postfix(CityMapPanel __instance)
            {
                ModLog.Logger.Log("打开地图" + __instance.MapId);  // 当前地图id string

                // 提取 NPC 列表
                var npcManager = DolocAPI.archiveHandle?.cityData.npcManager;
                if (npcManager == null) return;
                var npcs = npcManager.AllNpcs;
                var npcDataManager = new NpcDataManager(__instance, npcs);
                var npcList = npcDataManager.GetNpcListInCurrentMapId(_toggleShowUnknownNpc.Value);
                //npcDataManager.DebugNpcInfo(npcList); // 用于验证 npcList 正确性

                var pool = ObjectPoolManager.Get(__instance);

                var markerManager = new MapMarkerManager(__instance, pool, _stringIconType.Value);
                markerManager.RegisterMarker(npcList);
            }

            // 3. 关闭地图注销
            [HarmonyPostfix, HarmonyPatch(typeof(CityMapPanel), "UnRegister")]
            private static void CityMapPanel_UnRegister_Postfix(CityMapPanel __instance)
            {
                var pool = ObjectPoolManager.Get(__instance);
                pool?.RecycleAll();
            }
        }

    }

    public class MapMarkerManager
    {
        private readonly CityMapPanel _cityMapPanel;
        private readonly ObjectPool<DolocNavigationButton> _markerPool;
        private readonly string _iconType;

        public MapMarkerManager(CityMapPanel panel, ObjectPool<DolocNavigationButton> pool, string iconType)
        {
            _cityMapPanel = panel;

            _markerPool = pool;

            _iconType = iconType;
        }

        private Color ColorFrom255(int r, int g, int b, float alpha = 1f)
        {
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }
        private void ResetTip(DolocNavigationButton marker)
        {
            var rt = marker.GetComponent<RectTransform>();
            //重置锚点和位置
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(40, 40);
            

            // 限制 iconImg 尺寸
            var iconRt = marker.iconImg.GetComponent<RectTransform>();
            iconRt.sizeDelta = rt.sizeDelta;
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;  //居中

            // 清空图标
            marker.iconImg.sprite = null;
            // 重置颜色
            marker.iconImg.color = Color.white;
            marker.backgroundColor = Color.white;
            // 重置缩放
            marker.transform.localScale = Vector3.one;
            // 重置 alpha
            marker.alpha = 1f;
            //移除监听器
            marker.onPointerEnter.RemoveAllListeners();
            marker.onPointerExit.RemoveAllListeners();
        }
        public void ConfigureMarker(DolocNavigationButton marker, NpcInfo npc, bool isAbnormalPos)
        {
            //重置样式
            ResetTip(marker);  

            // 坐标
            marker.position = npc.MapPosition;
            // 随机偏移避免完全重叠 
            marker.position += new Vector2(UnityEngine.Random.Range(-3f, 3f), 0f);
            // 贴地
            marker.position += new Vector2(0f, -5f);
            // 透明度
            marker.alpha = 0.8f; 

            switch (_iconType)
            {
                case "Simple":
                    {
                        if (npc.HasKnownName)
                        {
                            marker.iconImg.sprite = DolocAPI.GetAsset<Sprite>("支线任务标记");
                            marker.iconSize = new Vector2(24f, 32f);
                            marker.iconImg.color = ColorFrom255(120, 166, 85);//绿色
                        }
                        else
                        {
                            marker.iconImg.sprite = DolocAPI.GetAsset<Sprite>("支线任务标记");
                            marker.iconSize = new Vector2(24f, 32f);
                            marker.iconImg.color = ColorFrom255(111, 111, 111);//灰色
                        }
                        break;
                    }
                case "Avatar":
                {
                        string iconSpriteName;
                        // 处理命名不一致
                        switch (npc.OriginTitle)
                        {
                            case "加百列":
                            {
                                iconSpriteName = "加百利-待机0";
                                break;
                            }
                            case "莉卡":
                            {
                                iconSpriteName = "莉卡贝奇-待机0";
                                break;
                            }
                            case "库玛桑":
                            {
                                iconSpriteName = "库马桑-待机0";
                                break;
                            }
                            case "洛维那":
                            {
                                iconSpriteName = "洛维耶-待机0";
                                break;
                            }
                            default:
                            {
                                iconSpriteName = $"{npc.OriginTitle}-待机0";
                                break;
                            }
                        }
                        marker.iconImg.sprite = DolocAPI.GetAsset<Sprite>(iconSpriteName);

                        //if (!marker.iconImg.sprite)
                        //{
                        //    marker.iconImg.sprite = DolocAPI.GetAsset<Sprite>($"{npc.Name}_icon");
                        //}
                        marker.iconSize = new Vector2(40f, 40f);
                        break;
                    }
                default:
                    ModLog.Logger.Log($"未知的图标类型: {_iconType}", Debug.LogWarning);
                    break;
            }

            //  创建 hover 文本;添加 hover 监听器
            string hoverTitle = npc.OriginTitle + npc.Name;
            string hoverInfo = npc.Scene;
            string hoverContent = npc.Status;

            if (isAbnormalPos)
            {
                hoverTitle += "(该角色坐标可能异常)";
            }

            TextGroup textGroup = new TextGroup(hoverTitle, hoverInfo, hoverContent);

            marker.onPointerEnter.AddListener(delegate (int _)
            {
                marker.HoverText(textGroup, UIAlignmentType.TopMiddle, UIAlignmentType.BottomMiddle);
            });
            marker.onPointerExit.AddListener(delegate (int _)
            {
                marker.HideHoverBox();
            });
        }

        public void RegisterMarker(List<NpcInfo> list)
        {
            var temCount = 0;
            // 先回收池中所有tip，隐藏全部，防止旧数据残留
            _markerPool.RecycleAll(); 

            foreach (var npc in list)
            {
                ModLog.Logger.Log((++temCount).ToString());
                ModLog.Logger.Log($"{npc.OriginTitle}-{npc.Name}");

                // 从对象池中取一个新的导航按钮
                var marker = _markerPool.Next;

                if (!marker)
                {
                    ModLog.Logger.Log("未能从池中获取 tip 对象！", Debug.LogError);
                    continue;
                }

                if (marker) marker.gameObject.SetActive(false);

                if (npc.IsAtVoidScene)
                {
                    _markerPool.Recycle(marker);
                    ModLog.Logger.Log("NPC位于虚空", Debug.LogError);
                    continue;
                }

                // 缓存异常排除
                DolocAPI.QueryRoom(npc.Scene, out Room room);
                MapArea mapArea;
                var roomAreaCache = Traverse.Create(_cityMapPanel).Field<Dictionary<string, MapArea>>("roomAreaCache").Value;

                if (!roomAreaCache.TryGetValue(room.SceneRawName, out mapArea) && !roomAreaCache.TryGetValue(room.RoomId, out mapArea))
                {
                    ModLog.Logger.Log($"{npc.OriginTitle}所在房间{room.RoomId}缓存异常", Debug.LogError);
                    continue;
                }

                // 判断npc坐标是否在四角
                var percent = room.Geometry.CalPosPercent(npc.WorldPosition);
                bool isCornerPos = 
                    (Mathf.Approximately(percent.x, 0f) && Mathf.Approximately(percent.y, 0f)) ||
                    (Mathf.Approximately(percent.x, 0f) && Mathf.Approximately(percent.y, 1f)) ||
                    (Mathf.Approximately(percent.x, 1f) && Mathf.Approximately(percent.y, 0f)) ||
                    (Mathf.Approximately(percent.x, 1f) && Mathf.Approximately(percent.y, 1f));

                // 世界坐标异常
                bool isZeroPos = Mathf.Approximately(npc.WorldPosition.x, 0f)&& Mathf.Approximately(npc.WorldPosition.y, 0f);

                // 坐标异常添加额外信息
                bool isAbnormalPos = isCornerPos || isZeroPos;


                ConfigureMarker(marker, npc, isAbnormalPos);


                marker.gameObject.SetActive(true);

                ModLog.Logger.Log($"WorldPosition: {npc.WorldPosition} - MapPosition: {npc.MapPosition} - 转换后Percent: {percent}");

            }
            ModLog.Logger.Log($"当前NPC列表共{temCount} 个：");
        }
    }

    // 
    public class NpcDataManager
    {
        private readonly List<Npc> _originNpcList;
        private readonly CityMapPanel _cityMapPanelInstance;
        private readonly List<NpcInfo> _npcInfos;
        private readonly string _currentMapId;
        public NpcDataManager(CityMapPanel cityMapPanel, IEnumerable<Npc> npcs)
        {
            _cityMapPanelInstance = cityMapPanel;
            _currentMapId = _cityMapPanelInstance.MapId;

            _originNpcList = npcs.ToList();
            _npcInfos = new List<NpcInfo>();

            foreach (var npc in _originNpcList)
            {
                var i = new NpcInfo(npc, _cityMapPanelInstance);
                _npcInfos.Add(i);
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
                      是否认识:{npc.HasKnownName}
                      中文名:{npc.OriginTitle}
                      原型name: {npc.ProtoName}
                      场景: {npc.Scene}
                      positionWS: {npc.WorldPosition}
                      mapPos: {npc.MapPosition}
                      状态信息: {npc.Status}
                      shouldRunSchedule: {npc.HasSchedule}
                      是否在虚空场景：{npc.IsAtVoidScene}
                    ------------------------------";
                ModLog.Logger.Log(logMessage);
            }
            ModLog.Logger.Log($"当前NPC列表共{list.Count} 个：", Debug.LogWarning);
        }


        // 输出当前房间的列表(是否显示未认识NPC)
        public List<NpcInfo> GetNpcListInCurrentMapId(bool isShowUnknownNpc = true)
        {
            var result = new List<NpcInfo>();

            foreach (var npc in _npcInfos)
            {
                if (!isShowUnknownNpc && !npc.HasKnownName)
                    continue;

                if (!DolocAPI.QueryRoom(npc.Scene, out Room room))
                    continue;

                if (room?.SceneConfig?.MapId != _currentMapId)
                    continue;

                result.Add(npc);
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

            public bool IsAtVoidScene;

            public NpcInfo(Npc npc, CityMapPanel cityMapPanel)
            {
                NpcId = npc.NpcId;
                Name = npc.NpcName;
                OriginTitle = npc.OriginTitle;
                Scene = npc.sceneName;
                ProtoName = npc.proto?.Name ?? "null";
                HasKnownName = npc.hasKnownName;
                HasSchedule = npc.shouldRunSchedule;
                WorldPosition = npc.positionWS;
                //WorldPosition = npc.Renderer.positionWS;


                // 世界坐标转地图坐标
                cityMapPanel.GetMapPosByWorldPosition(Scene, WorldPosition, out Vector2 mapPos);
                MapPosition = mapPos;

                // 在干嘛
                //NpcStatusInfo = npc.NpcStatusInfo;
                Status = GetNpcTask(npc);

                // 是否处于虚空场景
                IsAtVoidScene = npc.IsAtVoidScene;



            }

            private string GetNpcTask(Npc npc)
            {
                Type taskType = npc.controller.CurrentTaskType;
                string taskText;

                if (taskType == typeof(NpcTaskStreet))
                {
                    taskText = "站街中..";
                }
                else if (taskType == typeof(NpcTaskMove))
                {
                    taskText = "前往目标场景:" + Traverse.Create(npc).Field<string>("_currentJourneyInfo").Value;  // 如果是 private，可用 Harmony Traverse 访问
                }
                else
                {
                    taskText = "暂无工作";
                }
                return taskText;
            }
        }

    }
}
