using UnityEngine;
using System.Collections;

public class SwordController : MonoBehaviour
{
    [Header("Visual Assignments")]
    public GameObject swordVisual;
    public GameObject fireworkVisual; // Object chứa Particle System
    public Collider myCollider; 

    [Header("Settings")]
    public float flySpeed = 25f;
    public float rotateSpeed = 15f;
    public float lifeTime = 5f;
    public Vector3 modelCorrection = new Vector3(0, 90, 0);

    private GameObject _targetObject; 
    private bool _isFlying = false;
    private bool _hasHit = false;
    private Vector3 _fallbackDirection;

    private void OnEnable()
    {
        // Reset trạng thái
        if (swordVisual) swordVisual.SetActive(true);
        if (myCollider) myCollider.enabled = true;
        
        // Quan trọng: Tắt pháo hoa đi trước để lát bật lại
        if (fireworkVisual) fireworkVisual.SetActive(false);
        
        _isFlying = false;
        _hasHit = false;
        _targetObject = null;
        transform.SetParent(null);
        transform.localScale = Vector3.one; // Reset scale phòng trường hợp bị dính scale của cha cũ
    }

    public void Launch(GameObject target, Vector3 defaultDirection)
    {
        _targetObject = target;
        _fallbackDirection = defaultDirection;
        _isFlying = true;
        _hasHit = false;
        StartCoroutine(TimeoutRoutine());
    }

    private void Update()
    {
        if (!_isFlying || _hasHit) return;

        Vector3 moveDirection;

        if (_targetObject != null && _targetObject.activeInHierarchy)
        {
            // Bay vào tâm (cộng thêm 1 chút độ cao)
            Vector3 targetPoint = _targetObject.transform.position + Vector3.up * 0.5f; 
            moveDirection = (targetPoint - transform.position).normalized;
        }
        else
        {
            moveDirection = _fallbackDirection;
        }

        transform.position += moveDirection * flySpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * Quaternion.Euler(modelCorrection), rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isFlying || _hasHit) return;
        if (other.CompareTag("Player") || other.CompareTag("Sword")) return;

        HitAndExplode(other.transform);
    }

    void HitAndExplode(Transform hitObject)
    {
        _hasHit = true;
        _isFlying = false;

        // Gán làm con để di chuyển theo object bị đâm
        transform.SetParent(hitObject);

        StartCoroutine(ExplodeProcess());
    }

    IEnumerator ExplodeProcess()
    {
        // 1. Tắt hình kiếm & Collider
        if (swordVisual) swordVisual.SetActive(false);
        if (myCollider) myCollider.enabled = false;

        // 2. Bật Pháo Hoa và ÉP CHẠY (Fix lỗi không hiện)
        if (fireworkVisual) 
        {
            fireworkVisual.SetActive(true);
            
            // Tìm component ParticleSystem để ép nó chạy
            ParticleSystem ps = fireworkVisual.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();  // Dừng cũ
                ps.Clear(); // Xóa hạt cũ
                ps.Play();  // Chạy mới
            }
        }

        // 3. Chờ hiệu ứng chạy xong (ví dụ 2s)
        yield return new WaitForSeconds(2.0f);

        // 4. Tắt hoàn toàn
        gameObject.SetActive(false);
    }

    IEnumerator TimeoutRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!_hasHit && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
}