using System;

namespace Xunkong.Widget.Hoyolab.Core
{
    /// <summary>
    /// 元素类型
    /// </summary>
    [Flags]
    public enum ElementType
    {

        /// <summary>
        /// 未知
        /// </summary>
        None = 0,

        /// <summary>
        /// 物理
        /// </summary>
        Phys = 1,

        /// <summary>
        /// 火
        /// </summary>
        Pyro = 2,

        /// <summary>
        /// 水
        /// </summary>
        Hydro = 4,

        /// <summary>
        /// 风
        /// </summary>
        Anemo = 8,

        /// <summary>
        /// 雷
        /// </summary>
        Electro = 16,

        /// <summary>
        /// 草
        /// </summary>
        Dendro = 32,

        /// <summary>
        /// 冰
        /// </summary>
        Cryo = 64,

        /// <summary>
        /// 岩
        /// </summary>
        Geo = 128,

    }
}