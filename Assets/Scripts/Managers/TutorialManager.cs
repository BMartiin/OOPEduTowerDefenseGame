using UnityEngine;
using System.Collections;
public class TutorialManager : MonoBehaviour
{
    [Header("TutorPanel")]
    public GameObject tutorPanel;
    private RectTransform rectTransform;

    [Header("UI dolgok")]
    public TMPro.TMP_Text bubbleText;

    public Vector2 hiddenPosition = new Vector2(1300, 0);
    public Vector2 visiblePosition = new Vector2(0,0);

    private void Awake()
    {
        rectTransform = tutorPanel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = hiddenPosition;
    }


    public void SwimIn()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(visiblePosition));

    }

    public void SwimOut()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(hiddenPosition));

    }

    private IEnumerator MovePanel(Vector2 targetPosition)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.anchoredPosition;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime/duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition=targetPosition;
    }

    public void ShowTutorial(string message)
    {
        bubbleText.text = message;
        SwimIn();
    }

}
