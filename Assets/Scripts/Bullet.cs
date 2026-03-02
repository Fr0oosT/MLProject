using UnityEngine;

public class Bullet : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // StartCoroutine(DestroyAfterTime());
    }

    // Update is called once per frame
    void Update()
    {
    }

    // private System.Collections.IEnumerator DestroyAfterTime()
    // {
    //     yield return new WaitForSeconds(0.5f); // Adjust the time as needed
    //     Destroy(gameObject);
    // }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
