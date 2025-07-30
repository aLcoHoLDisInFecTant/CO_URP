using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 视频数据结构，用于在Inspector中方便地将名称和视频片段关联
/// </summary>
[System.Serializable]
public class VideoData
{
    public string name;
    public VideoClip clip;
}
