using System.Collections;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField] private Transform _point;
    [SerializeField] private ParticleSystem _waterVFX;
    [SerializeField] private ParticleSystem _waterBigSoftVFX;
    [SerializeField] private AudioSource _waterAudio;

    [SerializeField] private bool _isPlaying;
    [SerializeField] private bool _isSetup;

    public bool IsPlaying { get { return _isPlaying; } private set => _isPlaying = value; }
    public bool IsSetup => _isSetup;

    public void Play()
    {
        if (_isSetup) return;
        _isSetup = true;
        StartCoroutine(PlayCouroutine());
    }

    private IEnumerator PlayCouroutine()
    {
        _waterAudio.PlayScheduled(0);
        _waterVFX.Play();
        yield return new WaitForSeconds(_waterVFX.main.duration);
        _waterBigSoftVFX.Play();
        yield return new WaitForSeconds(.2f);
        IsPlaying = true;
        _isSetup = false;
    }

    public void Stop()
    {
        StartCoroutine(StopCouroutine());
    }

    public IEnumerator StopCouroutine()
    {
        _waterAudio.Stop();
        _waterBigSoftVFX.Stop();
        _waterVFX.Stop();
        yield return null;
        IsPlaying = false;
        _isSetup = false;
    }

    private void Update()
    {
        //if (!IsPlaying) return;
        //var cols = Physics.OverlapSphere(_point.position, _radius);
        //foreach (var col in cols)
        //{
        //    if(col.TryGetComponent<ITakeWaterable>(out var taker))
        //    {
        //        taker.OnTakeWater();
        //    }
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_point.position, _radius);
    }
}
