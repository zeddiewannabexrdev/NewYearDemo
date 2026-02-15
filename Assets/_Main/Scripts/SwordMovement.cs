using UnityEngine;
using System.Collections;

public class SwordMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float flySpeed = 25f;
    public float rotateSpeed = 15f;
    public float lifeTime = 5f; // Thời gian tự hủy nếu bay mãi không trúng
    public Vector3 modelCorrection = new Vector3(0, 90, 0);

    private GameObject _targetObject;
    private Vector3 _fallbackDirection;
    private bool _isFlying = false;

    // Hàm nhận lệnh bay (Gọi từ SkillCharging)
    public void Launch(GameObject target, Vector3 defaultDirection)
    {
        _targetObject = target;
        _fallbackDirection = defaultDirection;
        _isFlying = true;

        // Đếm ngược tự hủy nếu bắn trượt
        StartCoroutine(TimeoutRoutine());
    }

    private void OnDisable()
    {
        _isFlying = false;
        transform.SetParent(null); // Reset cha khi bị tắt
    }

    private void Update()
    {
        if (!_isFlying) return;

        Vector3 moveDirection;

        // Logic tìm đường
        if (_targetObject != null && _targetObject.activeInHierarchy)
        {
            Vector3 targetPoint = _targetObject.transform.position + Vector3.up * 0.5f;
            moveDirection = (targetPoint - transform.position).normalized;
        }
        else
        {
            moveDirection = _fallbackDirection;
        }

        // Di chuyển
        transform.position += moveDirection * flySpeed * Time.deltaTime;

        // Xoay hướng
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * Quaternion.Euler(modelCorrection), rotateSpeed * Time.deltaTime);
        }
    }

    IEnumerator TimeoutRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        // Nếu hết giờ mà vẫn còn sống (chưa va chạm) -> Tự tắt
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
}