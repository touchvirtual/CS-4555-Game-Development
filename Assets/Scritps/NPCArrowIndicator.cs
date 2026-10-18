
using UnityEngine;

public class NPCArrowIndicator : MonoBehaviour
{
    public GameObject arrow;
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;

    private Vector3 startPosition;
    private bool arrowShown = false;

    private void Start()
    {
        if (arrow != null)
        {
            startPosition = arrow.transform.localPosition;
            arrow.SetActive(false);
        }
    }

    private void Update()
    {
        if (!arrowShown || arrow == null)
            return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        arrow.transform.localPosition =
            startPosition + Vector3.up * bob;
    }

    public void ShowArrow()
    {
        if (arrow == null)
            return;

        arrowShown = true;
        startPosition = arrow.transform.localPosition;
        arrow.SetActive(true);
    }

    public void HideArrow()
    {
        arrowShown = false;

        if (arrow != null)
            arrow.SetActive(false);
    }
}
