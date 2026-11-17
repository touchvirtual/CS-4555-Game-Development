using UnityEngine;
using System.Collections;

public class BossAttackWarning : MonoBehaviour
{
    [Header("Warning Settings")]
    public GameObject warningZone;
    public float warningDuration = 1.5f;
    public float warningRadius = 4f;

    private bool isWarning;

    private void Start()
    {
        if (warningZone != null)
        {
            warningZone.SetActive(false);
            warningZone.transform.localScale =
                new Vector3(warningRadius * 2f, 0.02f, warningRadius * 2f);
        }
    }

    public IEnumerator ShowWarning()
    {
        if (isWarning)
            yield break;

        isWarning = true;

        if (warningZone != null)
            warningZone.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        if (warningZone != null)
            warningZone.SetActive(false);

        isWarning = false;
    }

    public bool IsWarning()
    {
        return isWarning;
    }
}