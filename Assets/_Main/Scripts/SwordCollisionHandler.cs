using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwordCollisionHandler : MonoBehaviour
{
    public GameObject swordVisual;
    public Collider myCollider;
    
    [HideInInspector] // Danh sách này sẽ được SkillCharging tự động điền vào
    public List<GameObject> fireworkVisuals;

    public float explosionDuration = 2.0f;
    private bool _hasExploded = false;

    private void OnEnable()
    {
        _hasExploded = false;
        if (swordVisual) swordVisual.SetActive(true);
        if (myCollider) myCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasExploded) return;
        if (other.CompareTag("Player") || other.CompareTag("Sword")) return;

        HandleCollision();
    }

    void HandleCollision()
    {
        _hasExploded = true;

        // Dừng di chuyển
        var movement = GetComponent<SwordMovement>();
        if (movement != null) movement.enabled = false;

        // Xử lý nổ
        // if (swordVisual) swordVisual.SetActive(false);
        // if (myCollider) myCollider.enabled = false;

        // KÍCH HOẠT DANH SÁCH PHÁO HOA NGOÀI
        if (fireworkVisuals != null && fireworkVisuals.Count > 0)
        {
            foreach (var fx in fireworkVisuals)
            {
                if (fx != null)
                {
                    // 1. Dịch chuyển pháo hoa tới chỗ kiếm đang đâm
                    // fx.transform.position = transform.position;
                    // fx.transform.rotation = transform.rotation;

                    // 2. Bật pháo hoa lên
                    fx.SetActive(true);

                    // 3. Nếu là Particle System thì ép nó chạy lại
                    // var ps = fx.GetComponent<ParticleSystem>();
                    // if (ps != null) { ps.Stop(); ps.Clear(); ps.Play(); }
                }
            }
        }

        //StartCoroutine(DisableAfterExplosion());
    }

    // IEnumerator DisableAfterExplosion()
    // {
    //     yield return new WaitForSeconds(explosionDuration);
        
    //     // Tắt toàn bộ pháo hoa trước khi thanh kiếm biến mất
    //     foreach (var fx in fireworkVisuals)
    //     {
    //         if (fx != null) fx.SetActive(false);
    //     }

    //     gameObject.SetActive(false);
    // }
}