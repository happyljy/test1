using UnityEngine;

namespace PlaneWar
{
    public class InfiniteMapScroller : MonoBehaviour
    {
        [Header("地图各段 (把Grid拖进来，按从下到上的顺序)")]
        [SerializeField] private Transform[] mapParts;

        [Header("滚动速度")]
        [SerializeField] private float scrollSpeed = 2f;

        [Header("单张地图的高度 (世界单位)")]
        [SerializeField] private float mapHeight = 11.5f;

        private float _bottomBoundary;

        void Start()
        {
            if (mapParts == null || mapParts.Length == 0) return;

            // 计算底部边界：假设第一张地图在Y=0，那么它移出屏幕底部的阈值
            // 比如屏幕底部在 Y=-5，那么地图下到 -15 就可以重置了
            // 这里简单用 mapHeight 作为位移阈值
            _bottomBoundary = mapParts[0].position.y - mapHeight * 1.5f;
        }

        void Update()
        {
            // 游戏暂停时，地图停止滚动
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (Time.deltaTime <= 0f) return;

            float delta = scrollSpeed * Time.deltaTime;

            // 1. 所有地图向下移动
            for (int i = 0; i < mapParts.Length; i++)
            {
                mapParts[i].position += Vector3.down * delta;
            }

            // 2. 检查是否有地图移出了底部边界，移出则挪到最顶部
            for (int i = 0; i < mapParts.Length; i++)
            {
                if (mapParts[i].position.y <= _bottomBoundary)
                {
                    // 找到当前最高的地图Y值
                    float highestY = -Mathf.Infinity;
                    for (int j = 0; j < mapParts.Length; j++)
                    {
                        if (mapParts[j].position.y > highestY)
                            highestY = mapParts[j].position.y;
                    }

                    // 把移出屏幕的这张，放到最高的那张的上面
                    Vector3 newPos = mapParts[i].position;
                    newPos.y = highestY + mapHeight;
                    mapParts[i].position = newPos;
                }
            }
        }
    }
}