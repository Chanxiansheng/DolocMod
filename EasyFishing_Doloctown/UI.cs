using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;


namespace UI_Doloctown
{

    // ------------自制消息框--------------
    public class DynamicMessageBox : MonoBehaviour
    {
        private GameObject messageBoxGO;
        private TextMeshProUGUI textComponent;
        private Image background;
        //private Sequence currentSequence;


        private Canvas targetCanvas;
        public void Show(string message, Color textColor)
        {
            if (messageBoxGO != null)
            {
                Hide();
            }

            targetCanvas = GameObject.Find("CanvasOverlay_Tips")?.GetComponent<Canvas>();
            if (targetCanvas == null)
            {
                // 备用canvas
                GameObject canvasGO = new GameObject("DynamicCanvas");
                targetCanvas = canvasGO.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 9999; //优先级
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

            }

            Debug.Log($"targetCanvas------:{targetCanvas.name}");

            // 2. 创建消息框背景
            messageBoxGO = new GameObject("MessageBox");
            background = messageBoxGO.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f); // 半透明黑色背景

            // 3. 创建文本
            var textGO = new GameObject("MessageText");
            textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = message;
            textComponent.fontSize = 24;
            textComponent.color = textColor;
            textComponent.alignment = TextAlignmentOptions.Center;

            // 设置字体
            Font unityFont = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == "DinkieBitmap-9px");
            Debug.Log($"unityFont------:{unityFont.name}");
            if (unityFont != null)
            {
                TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(unityFont);
                textComponent.font = tmpFont;
            }

            // 4. 设置父子关系
            messageBoxGO.transform.SetParent(targetCanvas.transform, false);
            textGO.transform.SetParent(messageBoxGO.transform, false);

            // 5. 设置布局
            RectTransform rectTransform = messageBoxGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100);
            rectTransform.anchoredPosition = new Vector2(0, 50);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = rectTransform.sizeDelta;
            textRect.anchoredPosition = Vector2.zero;

            // 6. 添加动画（可选）
            //currentSequence = DOTween.Sequence();
            //currentSequence.Append(background.DOColor(background.color.Alpha(0.7f), 0.5f));
            //currentSequence.Join(textComponent.DOColor(textColor.Alpha(1f), 0.5f));
        }
        public void Hide()
        {
            if (messageBoxGO != null)
            {
                // 可选：添加淡出动画
                //currentSequence.Append(background.DOColor(background.color.Alpha(0f), 0.5f));
                //currentSequence.Join(textComponent.DOColor(textComponent.color.Alpha(0f), 0.5f));
                //currentSequence.OnComplete(()  =>
                //{
                //    if (messageBoxGO != null)
                //        messageBoxGO.SetActive(false);
                //});
                messageBoxGO.SetActive(false);
            }
        }
    }


}