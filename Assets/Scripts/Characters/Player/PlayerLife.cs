using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            FindAnyObjectByType<GameManager>().GameOver();
        }
    }
}
