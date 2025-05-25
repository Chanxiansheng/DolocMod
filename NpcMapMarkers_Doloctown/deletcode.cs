using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcMapMarkers_Doloctown
{
    internal class Deletcode
    {

        //#region testCode
        //var tryNpc = npcDataManager.MyNpcList[8];

        //if (__result.GetMapPosByWorldPosition(tryNpc.Scene, tryNpc.WorldPosition, out Vector2 tryNpcmapPos))
        //{
        //    Logger.Log("进入注入图标环节", Debug.LogError);
        //    Logger.Log(tryNpcmapPos.ToString("F2"), Debug.LogError);
        //    // 确保对象池中有足够图标
        //    //tipPool.CheckCount();
        //    //tipPool.ActiveNext();
        //    // 获取一个原生图标组件（DolocNavigationButton）
        //    var tip = tipPool.Next;
        //    tip.gameObject.SetActive(true);

        //    // 设置图标位置
        //    tip.position = tryNpcmapPos;

        //    // 设置 sprite（可选）
        //    tip.iconImg.sprite = DolocAPI.GetAsset<Sprite>("ui_miniicon_default");
        //    //tip.iconImg.color = Color.red;

        //    // 禁用所有悬停提示等
        //    //tip.onPointerEnter.RemoveAllListeners();
        //    //tip.onPointerExit.RemoveAllListeners();
        //    //tip.onClick.RemoveAllListeners();
        //}
        //if (tipPool != null)
        //{
        //    var testTip = tipPool[0];
        //    Logger.Log($"原生图标坐标: {testTip.position}", Debug.LogWarning); // 这是真正应该给 anchoredPosition 的值范围
        //}
        //#endregion


        //    var marker = new GameObject($"NpcMarker_{tryNpc.NpcName}", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));

        //    var rt = marker.GetComponent<RectTransform>();
        //    //rt.SetParent(__result.transform, false);

        //    Transform tipRoot = Traverse.Create(__result).Field("tipRoot").GetValue<Transform>();

        //    if (tipRoot != null)
        //    {
        //        rt.SetParent(tipRoot, false);
        //    }
        //    else
        //    {
        //        Logger.Log("tipRoot 未找到，无法挂载地图图标", Debug.LogError);
        //    }

        //    marker.SetActive(true);
        //    rt.sizeDelta = new Vector2(30, 30);
        //    rt.anchoredPosition = tryNpcmapPos;

        //    var img = marker.GetComponent<UnityEngine.UI.Image>();
        //    img.color = Color.red; // 纯红色点
        //    img.enabled = true;

        //    // 可选：设置为圆形
        //    var sprite = DolocAPI.GetAsset<Sprite>("ui_miniicon_default");
        //    img.sprite = sprite;
        //    img.raycastTarget = false;



        //int npcId = npc.NpcId;
        //string name = npc.NpcName;
        //bool hasKnownName = npc.hasKnownName;  // 是否认识
        //string OriginTitle = npc.OriginTitle;  // 中文名
        //string scene = npc.sceneName;          // 所在场景


        //string proto = npc.proto?.Name;        // 原型


        //string status = npc.NpcStatusInfo?.ToString() ?? "null";

        //bool schedule = npc.shouldRunSchedule; // 是否激活日程

        //// 世界位置Render
        //string RenderworldPos = npc.Renderer != null
        //    ? npc.Renderer.transform.position.ToString("F2")
        //    : "无Renderer";

        //string positionWS = npc.positionWS.ToString("F2");

        //// 尝试转换坐标
        //__result.GetMapPosByWorldPosition(scene, npc.positionWS, out Vector2 mapPos);
    }
}
