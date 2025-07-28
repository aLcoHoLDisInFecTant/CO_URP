using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerBehavior : MonoBehaviour
{

    [SerializeField]
    public ScoreManager scoreManager;
    private float multiplier;
    [SerializeField] public ProgressBar progressBar;
    // Start is called before the first frame update
    void Start()
    {
        progressBar.currentPercent = 0;
        progressBar.invert = false;
        progressBar.restart = false;
        progressBar.isOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        multiplier = scoreManager.CombinedMultiplier;
        progressBar.currentPercent = Mathf.Clamp01(multiplier / 4f) * 100f;
        progressBar.isOn = true; // 开始驱动内部更新
    }
}
