using UnityEngine;
using System.Collections;

public class MovimientoSombra : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.left;
    float moveDistance = 1.5f;
    float moveDuration = 1.5f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

    }

    public void StartShadowMovement()
    {
        StartCoroutine(MoveAndDissapear());
    }

     IEnumerator MoveAndDissapear()
    {
        Vector3 targetPos = startPos + moveDirection.normalized * moveDistance;

        float time = 0f;

        while (time < moveDuration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, time / moveDuration);
            time += Time.deltaTime;
            yield return null;

        }
        transform.position = targetPos;

        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false);
    }


}
