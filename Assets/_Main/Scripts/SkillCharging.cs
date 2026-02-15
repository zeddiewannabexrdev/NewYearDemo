using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;
using Zef.Pool;

public class SkillCharging : MonoBehaviour
{
    [Header("Pool Settings")]
    public string swordPoolName = "Sword_Pool";
    public List<Transform> spawnPoints;

    [Header("Charging Settings")]
    public float delayBeforeFly = 2.5f;
    public float flySpeed = 5.0f; // Tăng tốc độ bay cho rõ

    [Header("Visual Effects")]
    public GameObject chargeObject;

    // --- SAFETY GUARDS (QUAN TRỌNG) ---
    private float _cooldownTimer = 0f;
    private const float SPAWN_COOLDOWN = 0.5f; // Chỉ cho phép kích hoạt lại sau 0.5s

    // Internal Variables
    private Grabbable _grabbable;
    private bool _isCharging = false;
    private bool _isFlying = false;
    private List<GameObject> _activeSwords = new List<GameObject>();
    private Coroutine _chargeCoroutine;

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
        // Giảm timer cooldown
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        if (Grabbable.IsHeld())
        {
            bool inputPressed = CheckInputButtonA();

            // LOGIC KÍCH HOẠT (Có bảo vệ Cooldown)
            if (inputPressed)
            {
                // Chỉ bắt đầu nếu chưa charge VÀ hết thời gian chờ
                if (!_isCharging && _cooldownTimer <= 0)
                {
                    StartChargingSequence();
                }

                // Xử lý logic khi đang giữ nút
                if (_isCharging)
                {
                    if (_isFlying)
                    {
                        HandleSwordsFlying();
                    }
                    else
                    {
                        KeepSwordsAtSpawnPoints();
                    }
                }
            }
            else
            {
                // Nhả nút -> Reset
                if (_isCharging)
                {
                    StopChargingSequence();
                    // Đặt cooldown để tránh spam nút ngay lập tức
                    _cooldownTimer = SPAWN_COOLDOWN; 
                }
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
        _isFlying = false;

        if (chargeObject) chargeObject.SetActive(true);

        // Gọi hàm sinh kiếm (Chỉ chạy 1 lần duy nhất tại đây)
        SpawnSwords();

        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _chargeCoroutine = StartCoroutine(WaitAndFly());
        
        Debug.Log("Skill Started: Spawning Swords"); // Log để kiểm tra có bị spam không
    }

    void StopChargingSequence()
    {
        _isCharging = false;
        _isFlying = false;

        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        if (chargeObject) chargeObject.SetActive(false);

        ReturnSwordsToPool();
        
        Debug.Log("Skill Stopped: Clearing Swords");
    }

    IEnumerator WaitAndFly()
    {
        yield return new WaitForSeconds(delayBeforeFly);
        _isFlying = true; // Chuyển trạng thái sang bay
    }

    void SpawnSwords()
    {
        // QUAN TRỌNG: Dọn dẹp sạch sẽ trước khi sinh mới để tránh duplicate list
        ReturnSwordsToPool(); 

        if (PoolManager.Instance == null) return;

        foreach (var point in spawnPoints)
        {
            if (point == null) continue;

            // Lấy từ Pool
            GameObject sword = PoolManager.Instance.Get(swordPoolName, false);
            
            // Bảo vệ: Nếu Pool trả về null hoặc quá tải
            if (sword != null)
            {
                sword.transform.position = point.position;
                sword.transform.rotation = point.rotation;
                sword.SetActive(true);
                _activeSwords.Add(sword);
            }
        }
    }

    void KeepSwordsAtSpawnPoints()
    {
        // Tối ưu vòng lặp: Dùng for thay vì foreach để nhanh hơn
        for (int i = 0; i < _activeSwords.Count; i++)
        {
            if (_activeSwords[i] != null && i < spawnPoints.Count)
            {
                _activeSwords[i].transform.position = spawnPoints[i].position;
                _activeSwords[i].transform.rotation = spawnPoints[i].rotation;
            }
        }
    }

    void HandleSwordsFlying()
    {
        // Bay lên
        for (int i = 0; i < _activeSwords.Count; i++)
        {
            if (_activeSwords[i] != null)
            {
                _activeSwords[i].transform.Translate(Vector3.up * flySpeed * Time.deltaTime, Space.World);
            }
        }
    }

    void ReturnSwordsToPool()
    {
        for (int i = 0; i < _activeSwords.Count; i++)
        {
            if (_activeSwords[i] != null)
            {
                _activeSwords[i].SetActive(false);
            }
        }
        _activeSwords.Clear(); // Xóa sạch list tham chiếu
    }

    private bool CheckInputButtonA()
    {
        if (Z_VRInput.Instance == null) return false;
        return Z_VRInput.Instance.isAPressing;
    }
}