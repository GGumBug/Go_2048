using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Color[]         blockColors;
    [SerializeField]
    private Image           imageBlock;
    [SerializeField]
    private TextMeshProUGUI textBlockNumeric;

    private int             numeric;
    private bool            combine = false;

    public Node             Target { private set; get; }

    public bool             NeedDestroy { private set; get; } = false;

    public int Numeric
    {
        set
        {
            numeric = value;

            textBlockNumeric.text = value.ToString();

            // Math.Log(a, b) -> 밑이 b이고, 위가 a인 log값을 return합니다. 
            imageBlock.color = blockColors[(int)Mathf.Log(value, 2) - 1];
        }
        get => numeric;
    }

    public void Setup()
    {
        Numeric = Random.Range(0, 100) < 90 ? 2 : 4;

        StartCoroutine(OnScaleAnimation(Vector3.one * 0.5f, Vector3.one, 0.15f));
    }

    public void MoveToNode(Node to)
    {
        Target = to;
        combine = false;
    }

    public void CombineToNode(Node to)
    {
        Target = to;
        combine = true;
    }

    public void StartMove()
    {
        float moveTime = 0.1f;
        StartCoroutine(OnLocalMoveAnimation(Target.localPosition, moveTime, OnAfterMove));
    }

    private void OnAfterMove()
    {
        if (Target != null)
        {
            if (combine)
            {
                Target.placedBlock.Numeric *= 2;
                Target.placedBlock.StarPunchScale(Vector3.one*0.25f, 0.15f, OnAfterPunchScale);
                gameObject.SetActive(false);
            }
            else
            {
                Target = null;
            }
        }
    }

    public void StarPunchScale(Vector3 punch, float time, UnityAction action=null)
    {
        StartCoroutine(OnPunchScale(punch, time, action));
    }

    private void OnAfterPunchScale()
    {
        Target      = null;
        NeedDestroy = true;
    }

    private IEnumerator OnPunchScale(Vector3 punch, float time, UnityAction action)
    {
        Vector3 current = Vector3.one;

        yield return StartCoroutine(OnScaleAnimation(current, current+punch, time*0.5f));

        yield return StartCoroutine(OnScaleAnimation(current + punch, current, time * 0.5f));

        if (action != null) action.Invoke();
    }

    private IEnumerator OnScaleAnimation(Vector3 start, Vector3 end, float time)
    {
        float current = 0;
        float percent = 0;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localScale = Vector3.Lerp(start, end, percent);

            yield return null;
        }
    }

    private IEnumerator OnLocalMoveAnimation(Vector3 end, float time, UnityAction action)
    {
        float current = 0;
        float percent = 0;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localPosition = Vector3.Lerp(start, end, percent);

            yield return null;
        }

        if (action != null) action.Invoke();
    }
}
