using UnityEngine;

public class GhostChase : GhostBehavior
{
    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        // Do nothing while the ghost is frightened
        if (node != null && enabled && !ghost.frightened.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                // Không cho quay đầu lại
                if (availableDirection == -ghost.movement.direction)
                    continue;

                Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                float distance = (ghost.target.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }

            // Nếu không còn hướng nào ngoài quay đầu, thì buộc phải quay đầu
            if (direction == Vector2.zero && node.availableDirections.Count > 0)
            {
                direction = -ghost.movement.direction;
            }

            ghost.movement.SetDirection(direction);
        }
    }

}
