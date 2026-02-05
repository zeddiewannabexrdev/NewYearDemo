using System.Collections;
using UnityEngine;
public class TestPool : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator IStart()
    {
        var obj = Zef.Pool.PoolManager.Instance.Get("cube");
        yield return new WaitForSeconds(3);
        obj.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Zef.Pool.PoolManager.Instance.GetAndAutoDeny("cube", 2);
        }
    }
}
