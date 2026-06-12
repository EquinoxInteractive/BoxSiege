using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f; // Jumlah damage yang diberikan peluru

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Jika peluru mengenai musuh
        if (collision.CompareTag("Enemy"))
        {
            // Dapatkan komponen health dari musuh dan kurangi health-nya
            // Contoh: collision.GetComponent<EnemyHealth>().TakeDamage(damage);

            // Hancurkan peluru
            Destroy(gameObject);
        }
        // Jika peluru mengenai obstacle/dinding
        else if (collision.CompareTag("Ground") || collision.CompareTag("Ground"))
        {
            // Hancurkan peluru
            Destroy(gameObject);
        }
    }
}