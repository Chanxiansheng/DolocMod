//using BepInEx;
//using BepInEx.Configuration;
//using DolocTown;
//using DolocTown.UI;
//using HarmonyLib;
//using ModUtilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using RedSaw;
//using UnityEngine;
//using DolocTown.Config.NPC;
//using static NpcMapMarkers_Doloctown.NpcMapMarkers.NpcDataManager;


//// using ModSettingManagerForDolocTown.API;

//namespace NpcMapMarkers_Doloctown
//{
//    [BepInPlugin(ModGuid, ModName, ModVersion)]
//    public class NpcMapMarkers : BaseUnityPlugin
//    {
//        private const string ModGuid = "YourCompany.NpcMapMarkers_Doloctown";
//        private const string ModName = "NpcMapMarkers_Doloctown";
//        private const string ModVersion = "1.0";

//        private new static readonly ModLogger Logger = new ModLogger(ModName, isDebug: true);

//        //====================== 配置项字段（示例，可删改） ======================
//        private static ConfigEntry<bool> _toggleShowUnknownNpc;
//        private static ConfigEntry<string> _stringIconType;

//        private const string Section = ModName;

//        private void SetupConfig()
//        {
//            _toggleShowUnknownNpc = Config.Bind(
//                Section,
//                "显示未认识NPC",
//                false,
//                new ConfigDescription("显示未认识NPC", null, new ConfigurationManagerAttributes { Order = 0 })
//            );


//            _stringIconType = Config.Bind(
//                Section,
//                "图标类型",
//                "Simple",
//                new ConfigDescription("图标类型", 
//                    new AcceptableValueList<string>("Simple", "Avatar"), 
//                    new ConfigurationManagerAttributes { Order = -1 }
//                    )
//            );

//            //TryRegisterWithModSettingManager();
//        }

//        private void TryRegisterWithModSettingManager()
//        {
//            Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
//            if (apiType != null && apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null)
//            {
//                try
//                {
//                    Logger.Log("发现 ModSettingAPI，尝试注入设置项...");
//                    dynamic api = apiType.GetMethod("Create").Invoke(null, null);

//                    // 示例注册（取消注释并替换变量）
//                    // api.AddToggle(_toggleFeatureX);
//                    // api.AddSlider(_numericParameterY);
//                    // api.AddKeyBind(_keyActionZ);
//                }
//                catch (Exception ex)
//                {
//                    Logger.Log("设置注入失败：" + ex.Message, Debug.LogError);
//                }
//            }
//            else
//            {
//                Logger.Log("未找到 ModSettingAPI，跳过设置注入", Debug.LogWarning);
//            }
//        }

//        public void Awake()
//        {
//            try
//            {
//                SetupConfig();
//                Harmony.CreateAndPatchAll(typeof(NpcMapMarkers));
//                Logger.Log("MOD 启动并完成 Harmony 注入！");
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("初始化失败：" + ex, Debug.LogError);
//            }
//        }






//        public class MapMarkerManager
//        {
//            private readonly CityMapPanel cityMapPanel;
//            private readonly Traverse traverse_cityMapPanel;
//            private readonly ObjectPool<DolocNavigationButton> tipPool;
//            public MapMarkerManager(CityMapPanel Panel)
//            {
//                cityMapPanel = Panel;

//                traverse_cityMapPanel = Traverse.Create(cityMapPanel);

//                tipPool = traverse_cityMapPanel.Field<ObjectPool<DolocNavigationButton>>("tipPool").Value;
//            }
//            Color ColorFrom255(int r, int g, int b, float alpha = 1f)
//            {
//                return new Color(r / 255f, g / 255f, b / 255f, alpha);
//            }
//            void ResetTip(DolocNavigationButton tip)
//            {
//                // 清空图标
//                tip.iconImg.sprite = null;

//                // 重置颜色
//                tip.iconImg.color = Color.white;

//                // 重置缩放
//                tip.transform.localScale = Vector3.one;

//                // 重置 alpha
//                tip.alpha = 1f;

//                // 可选：重置大小、偏移、pivot 等
//                tip.iconSize = new Vector2(32, 32); // 你的默认大小

//                tip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
//            }
//            public void ConfigureMarker(DolocNavigationButton tip,NpcInfo npc)
//            {
//                ResetTip(tip);  //初始化 tip

//                tip.gameObject.SetActive(true);

//                // 随机所有偏移避免完全重叠
//                tip.position = npc.MapPosition + new Vector2(UnityEngine.Random.Range(-3f, 3f), 0f);


//                switch (_stringIconType.Value)
//                {
//                    case "Simple":
//                    {
//                        if (npc.HasKnownName)
//                        {
//                            tip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
//                            tip.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
//                            tip.iconImg.color = ColorFrom255(7, 97, 255);//蓝色
//                            tip.alpha = 0.8f; //透明度
//                            tip.position = tip.position + new Vector2(0f, -4f);//贴地
//                            tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>("支线任务标记"); 

//                            //tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>("地点图标-npc");支线任务
//                        }
//                        else
//                        {
//                            tip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
//                            tip.transform.localScale = new Vector3(0.45f, 0.55f, 1f);
//                            tip.iconImg.color = ColorFrom255(82, 93, 125);
//                            tip.alpha = 0.8f; //透明度
//                            tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>("ui_miniicon_default");
//                        }
//                        break;
//                    }
//                    case "Avatar":
//                    {
//                        var iconSpriteName = $"{npc.Name}_icon";
//                        tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>(iconSpriteName);
//                        tip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
//                        tip.iconSize = new Vector2(73f, 27f);
//                        tip.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
//                        tip.alpha = 0.8f; //透明度

//                        break;
//                    }
//                    default:
//                        Debug.LogWarning($"未知的图标类型: {_stringIconType.Value}");
//                        break;
//                }



//                //  创建 hover 文本;添加 hover 监听器
//                TextGroup textGroup = new TextGroup(npc.OriginTitle+ npc.Name, npc.Scene+" - "+npc.Status,npc.MapPosition.ToString("F2"));
//                tip.onPointerEnter.AddListener(delegate (int _)
//                {
//                    traverse_cityMapPanel.Property("latestHoveredObject").SetValue(tip.gameObject);
//                    traverse_cityMapPanel.Method("AdjustCursorPosition", new object[] { tip.position }).GetValue();

//                    tip.HoverText(textGroup, UIAlignmentType.TopMiddle, UIAlignmentType.BottomMiddle);
//                });
//                tip.onPointerExit.AddListener(delegate (int _)
//                {
//                    if (traverse_cityMapPanel.Property("latestHoveredObject").GetValue<GameObject>() == tip.gameObject)
//                    {
//                        traverse_cityMapPanel.Property("latestHoveredObject").SetValue(null);
//                    }
//                    tip.HideHoverBox();
//                });
//            }

//            public void RegisterMarker(List<NpcInfo> list)
//            {
//                var temCount = 0;

//                foreach (var npc in list)
//                {
//                    temCount++;
//                    Debug.Log(temCount);

//                    // 1. 从对象池中取一个新的导航按钮
//                    var tip = tipPool.Next;

//                    ConfigureMarker(tip, npc);

//                    // 调试
//                    Debug.LogWarning("*********************************");
//                    DolocAPI.QueryRoom(npc.Scene, out Room room);
//                    if (npc.IsAtVoidScene)
//                    {
//                        Debug.LogError("位于虚空");
//                    }
//                    Debug.Log($"[调试NPC] {npc.OriginTitle}-{npc.Name} in {room.RoomId}");

//                    MapArea mapArea;
//                    var roomAreaCache = Traverse.Create(cityMapPanel).Field<Dictionary<string, MapArea>>("roomAreaCache").Value;

//                    if (!roomAreaCache.TryGetValue(room.SceneRawName, out mapArea) && !roomAreaCache.TryGetValue(room.RoomId, out mapArea))
//                    {
//                        Debug.Log("条件失败：进入 roomAreaCache.TryGetValue");
//                        continue;
//                    }

//                    Debug.Log($"WorldPosition: {npc.WorldPosition},MapPosition: {npc.MapPosition}");

//                    var percent = room.Geometry.CalPosPercent(npc.WorldPosition);
//                    var uiPos = mapArea.GetPosByPercent(percent.x, percent.y);

//                    Debug.Log($@"
//                           转换后Percent: {percent}
//                           转换后 MapPosition: {uiPos}
//                           转换后 MapArea pos: {mapArea.position}
//                           MapArea size: {mapArea.size}");
//                    Debug.LogWarning("---------------------------------");

//                }
//                Debug.LogError($"当前NPC列表共{temCount} 个：");

//            }
//        }


//        [HarmonyPostfix,HarmonyPatch(typeof(MapPanel), "GetAndInitMap")]
//        private static void MapPanel_GetAndInitMap_Postfix(ref CityMapPanel __result)
//        {
//            var npcManager = DolocAPI.archiveHandle?.cityData.npcManager;
//            if (npcManager == null) return;

//            var npcs = npcManager.AllNpcs;

//            //Logger.Log(__result.MapId);  // 当前地图id string

//            var npcDataManager = new NpcDataManager(__result, npcs);

//            var npcList = npcDataManager.GetNpcListInCurrentMapId(_toggleShowUnknownNpc.Value);
//            npcDataManager.DebugNpcInfo(npcList); // 用于验证 npcList 正确性


//            var markerManager = new MapMarkerManager(__result);
//            markerManager.RegisterMarker(npcList);

//        }


//        // 
//        public class NpcDataManager
//        {
//            public List<Npc> OriginNpcList;
//            public CityMapPanel CityMapPanelInstance;
//            public List<NpcInfo> MyNpcList;
//            public string CurrentMapId;
//            public NpcDataManager(CityMapPanel cityMapPanel, IEnumerable<Npc> npcs)
//            {
//                CityMapPanelInstance = cityMapPanel;
//                CurrentMapId = CityMapPanelInstance.MapId;

//                OriginNpcList = npcs.ToList();
//                MyNpcList = new List<NpcInfo>();

//                foreach (var npc in OriginNpcList)
//                {
//                    var i = new NpcInfo(npc, CityMapPanelInstance);
//                    MyNpcList.Add(i);
//                }
//            }

//            // 调试所有NPC信息
//            public void DebugNpcInfo(List<NpcInfo> list)
//            {
//                foreach (var npc in list)
//                {
//                    string logMessage = $@"
//                    → NPC 名称: {npc.Name}
//                      NPC ID: {npc.NpcId}
//                      是否认识:{npc.HasKnownName}
//                      中文名:{npc.OriginTitle}
//                      原型name: {npc.ProtoName}
//                      场景: {npc.Scene}
//                      positionWS: {npc.WorldPosition}
//                      mapPos: {npc.MapPosition}
//                      状态信息: {npc.Status}
//                      shouldRunSchedule: {npc.HasSchedule}
//                      是否在虚空场景：{npc.IsAtVoidScene}
//                    ------------------------------";
//                    Logger.Log(logMessage);
//                }
//                Logger.Log($"当前NPC列表共{list.Count} 个：", Debug.LogWarning);
//            }


//            // 输出当前房间的列表(是否显示未认识NPC)
//            public List<NpcInfo> GetNpcListInCurrentMapId(bool isShowUnknownNpc = true)
//            {
//                var result = new List<NpcInfo>();

//                foreach (var npc in MyNpcList)
//                {
//                    if (!isShowUnknownNpc&&!npc.HasKnownName)
//                        continue;
                    
//                    if (!DolocAPI.QueryRoom(npc.Scene, out Room room))
//                        continue;

//                    if (room?.SceneConfig?.MapId != CurrentMapId)
//                        continue;

//                    result.Add(npc);
//                }
//                return result;
//            }

//            public class NpcInfo
//            {
//                public int NpcId;
//                public string Name;
//                public string OriginTitle;
//                public string Scene;
//                public string ProtoName;
//                public string Status;
//                public bool HasKnownName;
//                public bool HasSchedule;
//                public Vector2 WorldPosition;
//                public Vector2 MapPosition;

//                public bool IsAtVoidScene;

//                public NpcInfo(Npc npc, CityMapPanel cityMapPanel)
//                {
//                    NpcId = npc.NpcId;
//                    Name = npc.NpcName;
//                    OriginTitle = npc.OriginTitle;
//                    Scene = npc.sceneName;
//                    ProtoName = npc.proto?.Name ?? "null";
//                    HasKnownName = npc.hasKnownName;
//                    HasSchedule = npc.shouldRunSchedule;
//                    WorldPosition = npc.positionWS;
//                    //WorldPosition = npc.Renderer.positionWS;


//                    // 世界坐标转地图坐标
//                    cityMapPanel.GetMapPosByWorldPosition(Scene, WorldPosition, out Vector2 mapPos);
//                    MapPosition = mapPos;

//                    // 在干嘛
//                    //NpcStatusInfo = npc.NpcStatusInfo;
//                    Status = GetNpcTask(npc);

//                    // 是否处于虚空场景
//                    IsAtVoidScene = npc.IsAtVoidScene;
//                }

//                private string GetNpcTask(Npc npc)
//                {
//                    Type taskType = npc.controller.CurrentTaskType;
//                    string taskText;

//                    if (taskType == typeof(NpcTaskStreet))
//                    {
//                        taskText = "站街中..";
//                    }
//                    else if (taskType == typeof(NpcTaskMove))
//                    {
//                        taskText = "前往目标场景:" + Traverse.Create(npc).Field<string>("_currentJourneyInfo").Value;  // 如果是 private，可用 Harmony Traverse 访问
//                    }
//                    else
//                    {
//                        taskText = "暂无工作";
//                    }
//                    return taskText;
//                }
//            }

//        }
//    }
//}
