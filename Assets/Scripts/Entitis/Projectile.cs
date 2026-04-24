using UnityEngine;

public class Projectile : MonoBehaviour
{
    private BaseEntity target;
    private float damage;
    private float speed = 5f;
    private Vector2 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    public void Initialize(BaseEntity targetEntity, float damageAmount)
    {
        target = targetEntity;
        damage = damageAmount;
    }

    void Update()
    {
        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 targetPos = target.transform.position;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider != null)
        {
            targetPos = targetCollider.bounds.center;
        }
        float totalDist = Mathf.Abs(targetPos.x - startPos.x);

        float nextX = Mathf.MoveTowards(transform.position.x, targetPos.x, speed * Time.deltaTime);

        float t = totalDist > 0 ? Mathf.Abs(nextX - startPos.x) / totalDist : 1f; //fancy

        float arc = Mathf.Sin(t * Mathf.PI) * 1.5f;
        float nextY = Mathf.Lerp(startPos.y, targetPos.y, t) + arc;

        Vector2 nextPosition = new Vector2(nextX, nextY);

        Vector2 direction = nextPosition - (Vector2)transform.position;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        transform.position = nextPosition;

        if (Mathf.Abs(transform.position.x - targetPos.x) < 0.2f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}