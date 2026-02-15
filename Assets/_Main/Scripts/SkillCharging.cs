using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Zef.Pool;
using DG.Tweening;

public class SkillCharging : MonoBehaviour
{
    [Header("Pool Settings")]
    public string swordPoolName = "Sword_Pool";
    public List<Transform> spawnPoints;

    [Header("Targeting Logic")]
    [Tooltip("Kéo một GameObject cụ thể vào đây để làm mục tiêu cố định (VD: Boss). Nếu để trống, sẽ tự quét.")]
    public GameObject hardTarget; 
    
    [Tooltip("Nếu Hard Target trống, sẽ quét layer này")]
    public LayerMask autoScanLayer; 
    public float scanRadius = 5.0f;
    public float scanDistance = 50f;

    [Header("Timing")]
    public float delayBeforeFly = 2.0f;

    [Header("Visual")]
    public GameObject chargeObject;

    private Grabbable _grabbable;
    private bool _isCharging = false;
    private List<GameObject> _activeSwords = new List<GameObject>();
    private Sequence _chargeSequence;
    
    // Lưu mục tiêu cuối cùng (là GameObject)
    private GameObject _finalTarget;

    public Grabbable Grabbable
    {
        get
        {
            if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
            return _grabbable;
        }
    }

    private void Awake()
    {
        if (chargeObject) chargeObject.SetActive(false);
    }

    private void Update()
    {
        if (Grabbable.IsHeld())
        {
            if (CheckInputButtonA())
            {
                if (!_isCharging) StartChargingSequence();
                KeepSwordsAtSpawnPoints();
            }
            else
            {
                if (_isCharging) StopChargingSequence();
            }
        }
        else
        {
            if (_isCharging) StopChargingSequence();
        }
    }

    void StartChargingSequence()
    {
        _isCharging = true;
        if (chargeObject) chargeObject.SetActive(true);

        // 1. Xác định mục tiêu ngay khi bắt đầu
        DetermineTarget();

        SpawnSwords();

        _chargeSequence?.Kill();
        _chargeSequence = DOTween.Sequence();
        _chargeSequence.AppendInterval(delayBeforeFly);
        _chargeSequence.AppendCallback(LaunchAllSwords);
    }

    void StopChargingSequence()
    {
        _isCharging = false;
        _chargeSequence?.Kill();
        if (chargeObject) chargeObject.SetActive(false);
        ReturnSwordsToPool();
    }

    // Logic xác định mục tiêu
    void DetermineTarget()
    {
        _finalTarget = null;

        // ƯU TIÊN 1: Nếu có Hard Target gán sẵn trong Inspector -> Dùng luôn
        if (hardTarget != null && hardTarget.activeInHierarchy)
        {
            _finalTarget = hardTarget;
            // Debug.Log("Using Hard Target: " + _finalTarget.name);
            return;
        }

        // ƯU TIÊN 2: Tự động quét (Auto Scan)
        Transform head = Camera.main.transform;
        if (PlayerController.Instance != null) head = PlayerController.Instance.head;

        RaycastHit hit;
        if (Physics.SphereCast(head.position, scanRadius, head.forward, out hit, scanDistance, autoScanLayer))
        {
            _finalTarget = hit.collider.gameObject; // Lấy GameObject từ Collider
            // Debug.Log("Auto Scanned Target: " + _finalTarget.name);
        }
    }

    void LaunchAllSwords()
    {
        Transform head = Camera.main.transform;
        if (PlayerController.Instance != null) head = PlayerController.Instance.head;
        
        Vector3 defaultDir = head.forward;
        defaultDir.y = 0; defaultDir.Normalize();
        Vector3 blindDir = (Quaternion.LookRotation(defaultDir) * Quaternion.Euler(-30, 0, 0)) * Vector3.forward;

        foreach (var swordObj in _activeSwords)
        {
            if (swordObj != null)
            {
                SwordController ctrl = swordObj.GetComponent<SwordController>();
                if (ctrl != null)
                {
                    // Truyền GameObject Target vào
                    ctrl.Launch(_finalTarget, blindDir);
                }
            }
        }

        _activeSwords.Clear();
        _isCharging = false;
        if (chargeObject) chargeObject.SetActive(false);
    }

    void SpawnSwords()
    {
        ReturnSwordsToPool();
        if (PoolManager.Instance == null) return;

        foreach (var point in spawnPoints)
        {
            if (point == null) continue;
            GameObject sword = PoolManager.Instance.Get(swordPoolName, false);

            if (sword != null)
            {
                sword.transform.position = point.position;
                sword.transform.rotation = Quaternion.Euler(-90, 0, 0);
                sword.SetActive(true);
                _activeSwords.Add(sword);
            }
        }
    }

    void KeepSwordsAtSpawnPoints()
    {
        for (int i = 0; i < _activeSwords.Count; i++)
        {
            if (i < spawnPoints.Count && spawnPoints[i] != null && _activeSwords[i] != null)
            {
                _activeSwords[i].transform.position = spawnPoints[i].position;
                _activeSwords[i].transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
        }
    }

    void ReturnSwordsToPool()
    {
        foreach (var sword in _activeSwords)
        {
            if (sword != null) sword.SetActive(false);
        }
        _activeSwords.Clear();
    }

    private bool CheckInputButtonA()
    {
        if (Z_VRInput.Instance == null) return false;
        return Z_VRInput.Instance.isAPressing;
    }
}