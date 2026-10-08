using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlaneWar
{
    /// <summary>
    /// 纯图片拼装数字显示（支持 0-9 和小数点）
    /// 挂载在 Canvas 下的空物体上，配合 Horizontal Layout Group 使用
    /// </summary>
    public class PixelNumberDisplay : MonoBehaviour
    {
        [Header("按顺序拖入: num_0 到 num_9 以及 dot (共11个)")]
        public Sprite[] numberSprites = new Sprite[11];

        [Header("单个数字的尺寸")]
        public Vector2 digitSize = new Vector2(40, 60);

        private List<Image> _digitPool = new List<Image>();
        private string _currentValue = "";

        /// <summary>
        /// 设置显示的数字（支持小数，例如 "105.5"）
        /// </summary>
        public void SetNumber(string value)
        {
            if (_currentValue == value) return;
            _currentValue = value;

            // 1. 回收多余的数字（长度变短时）
            while (_digitPool.Count > value.Length)
            {
                Destroy(_digitPool[_digitPool.Count - 1].gameObject);
                _digitPool.RemoveAt(_digitPool.Count - 1);
            }

            // 2. 动态生成或更新数字
            for (int i = 0; i < value.Length; i++)
            {
                // 修改后的代码片段
                if (i >= _digitPool.Count)
                {
                    var go = new GameObject("Digit", typeof(Image));
                    go.transform.SetParent(transform, false);
                    var img = go.GetComponent<Image>();

                    // 1. 强制固定长宽
                    img.rectTransform.sizeDelta = digitSize;

                    // 2. 保持图片比例，防止被拉伸变形
                    img.preserveAspect = true;

                    // 3. 添加 LayoutElement 告诉 Layout Group 用这个固定尺寸
                    var layout = go.AddComponent<LayoutElement>();
                    layout.preferredWidth = digitSize.x;
                    layout.preferredHeight = digitSize.y;
                    layout.minWidth = digitSize.x;
                    layout.minHeight = digitSize.y;

                    _digitPool.Add(img);
                }

                int index = -1;
                if (value[i] >= '0' && value[i] <= '9') index = value[i] - '0';
                else if (value[i] == '.') index = 10;

                if (index >= 0 && index < numberSprites.Length && numberSprites[index] != null)
                {
                    _digitPool[i].sprite = numberSprites[index];
                    _digitPool[i].gameObject.SetActive(true);
                }
                else
                {
                    _digitPool[i].gameObject.SetActive(false);
                }
            }
        }

        // 便于 Inspector 调试，支持右键 Reset 时自动填充尺寸
        void Reset()
        {
            digitSize = new Vector2(40, 60);
        }
    }
}