using System.Collections.Generic;
using UnityEngine;
// using Autohand; // Không cần thư viện này nữa vì không dùng Grabbable
using Zef.Pool;
using DG.Tweening;

public class SkillCharging : MonoBehaviour
{
    [Header("Pool Settings")]
    public string swordPoolName = "Sword_Pool";
    public List<Transform> spawnPoints; // Kéo các điểm quanh người Player vào đây

    [Header("Targeting Logic")]
    [Tooltip("Kéo Boss hoặc mục tiêu cố định vào đây. Nếu để trống sẽ tự quét.")]
    public GameObject hardTarget; 
    
    [Tooltip("Layer để quét kẻ địch tự động")]
    public LayerMask autoScanLayer; 
    public float scanRadius = 10.0f; // Tăng radius lên chút để dễ tìm quái hơn
    public float scanDistance = 50f;

    [Header("Timing")]
    public float delayBeforeFly = 2.0f;

    [Header("Visual")]
    public GameObject chargeObject; // Hiệu ứng vòng tròn dưới chân hoặc quanh người (nếu có)
    public List<GameObject> fireworksInScene;

    // --- Private Variables ---
    private bool _isCharging = false;
    private List<GameObject> _activeSwords = new List<GameObject>();
    private Sequence _chargeSequence;
    private GameObject _finalTarget;

    private void Awake()
    {
        if (chargeObject) chargeObject.SetActive(false);
    }

    private void Update()
    {
        // LOGIC MỚI: Chỉ cần kiểm tra nút bấm, không cần kiểm tra cầm nắm
        if (CheckInputButtonA())
        {
            // Nếu đang giữ nút A
            if (!_isCharging)
            {
                StartChargingSequence();
            }
            
            // Cập nhật vị trí kiếm đi theo người chơi
            KeepSwordsAtSpawnPoints();
        }
        else
        {
            // Nếu nhả nút A
            if (_isCharging)
            {
                StopChargingSequence();
            }
        }
    }

    void StartChargingSequence()
    {
        _isCharging = true;
        if (chargeObject) chargeObject.SetActive(true);

        // 1. Xác định mục tiêu
        DetermineTarget();

        // 2. Sinh kiếm
        SpawnSwords();

        // 3. Đếm ngược rồi bắn tự động
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
        
        // Nhả nút trước khi bắn -> Hủy chiêu (Thu hồi kiếm)
        ReturnSwordsToPool();
    }

    void DetermineTarget()
    {
        _finalTarget = null;

        // Ưu tiên Hard Target
        if (hardTarget != null && hardTarget.activeInHierarchy)
        {
            _finalTarget = hardTarget;
            return;
        }

        // Tự động quét theo hướng nhìn của Camera
        Transform head = Camera.main.transform;
        if (PlayerController.Instance != null && PlayerController.Instance.head != null) 
            head = PlayerController.Instance.head;

        RaycastHit hit;
        if (Physics.SphereCast(head.position, scanRadius, head.forward, out hit, scanDistance, autoScanLayer))
        {
            _finalTarget = hit.collider.gameObject;
        }
    }

    void LaunchAllSwords()
    {
        Transform head = Camera.main.transform;
        if (PlayerController.Instance != null && PlayerController.Instance.head != null) 
            head = PlayerController.Instance.head;
        
        // Tính hướng bay mù (nếu không có target)
        Vector3 defaultDir = head.forward;
        defaultDir.y = 0; defaultDir.Normalize();
        Vector3 blindDir = (Quaternion.LookRotation(defaultDir) * Quaternion.Euler(-30, 0, 0)) * Vector3.forward;

        foreach (var swordObj in _activeSwords)
        {
            if (swordObj != null)
            {
                SwordMovement movement = swordObj.GetComponent<SwordMovement>();
                if (movement != null)
                {
                    movement.Launch(_finalTarget, blindDir);
                }
            }
        }

        // Xóa list quản lý để kiếm tự bay
        _activeSwords.Clear();
        _isCharging = false; // Reset trạng thái
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
                sword.transform.rotation = Quaternion.Euler(-90, 0, 0); // Mặc định cắm xuống đất
                sword.SetActive(true);
                
                var handler = sword.GetComponent<SwordCollisionHandler>();
                if(handler != null)
                {
                    handler.fireworkVisuals = fireworksInScene;
                }

                _activeSwords.Add(sword);
            }
        }
    }

    void KeepSwordsAtSpawnPoints()
    {
        // Giữ kiếm dính chặt vào các điểm SpawnPoints (lúc này là con của Player)
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