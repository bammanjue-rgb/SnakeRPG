using UnityEngine;

// 인스펙터에서 프리팹과 크기를 세트로 관리하기 위한 클래스
[System.Serializable]
public class EffectEntry
{
    public GameObject prefab;
    public Vector3 scale = new Vector3(40, 40, 1); // 기본값
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("1. 고정형 이펙트 (제자리에서 터짐)")]
    public EffectEntry attackEffect;   // 공격
    public EffectEntry dieEffect;      // 사망
    public EffectEntry hitEffect;      // 피격
    public EffectEntry eatEffect;      // 섭취
    public EffectEntry allyEffect;     // 합류

    [Header("2. 추적형 이펙트 (캐릭터 따라다님)")]
    public EffectEntry levelUpEffect;  // 레벨업

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 📍 1. 고정형 재생: 특정 좌표에 생성
    public void PlayFixed(EffectEntry effect, Vector3 position)
    {
        if (effect == null || effect.prefab == null) return;

        GameObject go = Instantiate(effect.prefab, position, Quaternion.identity);
        
        // 인스펙터에서 설정한 크기 적용
        go.transform.localScale = effect.scale; 

        // 🔥 수명 자동 계산 및 삭제
        float lifeTime = GetAutoDestroyTime(go);
        Destroy(go, lifeTime); 
    }

    // 🏃 2. 추적형 재생: 타겟을 따라다님 (부모 크기 영향 무시)
    public void PlayFollow(EffectEntry effect, Transform target)
    {
        if (effect == null || effect.prefab == null || target == null) return;

        GameObject go = Instantiate(effect.prefab, target.position, Quaternion.identity);
        go.transform.SetParent(target); 
        
        // 위치를 부모의 정중앙으로
        go.transform.localPosition = Vector3.zero;

        // 🔥 절대 크기 유지 (부모가 커져도 이펙트는 설정한 크기 유지)
        Vector3 parentScale = target.lossyScale;
        float parentX = parentScale.x == 0 ? 1 : parentScale.x;
        float parentY = parentScale.y == 0 ? 1 : parentScale.y;

        Vector3 newLocalScale = new Vector3(
            effect.scale.x / parentX,
            effect.scale.y / parentY,
            1f 
        );
        go.transform.localScale = newLocalScale;

        // 🔥 수명 자동 계산 및 삭제
        float lifeTime = GetAutoDestroyTime(go);
        Destroy(go, lifeTime); 
    }

    // 🕒 이펙트의 재생 시간을 분석하여 수명을 결정하는 함수
    float GetAutoDestroyTime(GameObject go)
    {
        float duration = 2.0f; // 기본 안전장치 (분석 실패 시 2초)

        // 1. 스프라이트 애니메이션 (Animator) 확인
        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            // 애니메이터가 아직 초기화 안 됐을 수 있으므로 강제 업데이트
            anim.Update(0f); 
            
            // 현재 설정된 애니메이션 클립의 길이 가져오기
            // (주의: Loop Time이 꺼져 있어야 정확히 한 번 재생 후 삭제됨)
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.length > 0) duration = info.length;
        }
        // 2. 파티클 시스템 (ParticleSystem) 확인
        else 
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                duration = ps.main.duration;
                ps.Play();
            }
        }

        return duration;
    }
}