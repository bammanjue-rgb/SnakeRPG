using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource; // 효과음 재생기

    [Header("Audio Clips")]
    public AudioClip attackClip;     // 공격 소리
    public AudioClip dieClip;        // 몬스터 사망 소리
    public AudioClip eatClip;        // 음식 먹는 소리
    public AudioClip allyClip;       // 동료 구출 소리
    public AudioClip levelUpClip;    // 레벨업 소리
    public AudioClip hitClip;        // 플레이어 피격 소리
    public AudioClip gameOverClip;   // 게임 오버 소리

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot은 소리가 겹쳐도 끊기지 않고 겹쳐서 재생됨 (타격감에 필수)
            sfxSource.PlayOneShot(clip);
        }
    }
}