using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int hp = 100;
    public float ms = 2f;
    protected Transform player;

[Header("Pengaturan State Machine")]
[SerializeField] private float jarakDeteksi = 6f;
[SerializeField] private float jarakSerang = 1f;
[SerializeField] private float jarakBatas = 0.5f;
// State definitions for enemy behavior
private enum StateZombie { Idle, Patrol, Walk, Chase, Attack, Die }
private StateZombie currentState = StateZombie.Idle;
private float waktuSerangTerakhir;
    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
       

        periksaTransisi();

        // switch (currentState)
        // {
        //     case StateZombie.Idle:
        //         // Do nothing while idle
        //         break;
        //     case StateZombie.Patrol:
        //         perilakuPatroli();
        //         break;
        //     case StateZombie.Walk:
        //         // Could implement walk logic here
        //         break;
        //     case StateZombie.Chase:
        //         Kejar();
        //         break;
        //     case StateZombie.Attack:
        //         Serang();
        //         break;
        //     case StateZombie.Die:
        //         // Die handled in KenaDamage/Mati
        //         break;
        // }

    }

    public void Kejar()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            ms * Time.deltaTime
        );
    }

    public virtual void Serang()
    {
        Debug.Log("Enemy menyerang!");
    }
    void perilakuPatroli()
    {   
        Kejar();
        Debug.Log("Enemy sedang patroli.");
    }

    void perilakucheck()
    { 
        Debug.Log("Enemy sedang memeriksa sekitarnya.");
    }
     void periksaTransisi()
    {
        if (player == null)
        {
            currentState = StateZombie.Idle;
            return;
        }

        float jarak = jarakePlayer();

        if (jarak <= jarakSerang)
        {
            currentState = StateZombie.Attack;
        }
        else if (jarak <= jarakDeteksi)
        {
            currentState = StateZombie.Chase;
        }
        else
        {
            currentState = StateZombie.Patrol;
        }
    }

    private float jarakePlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }



     
    public void KenaDamage(int jumlah)
    {
        hp -= jumlah;
        Debug.Log($"{gameObject.name} kena damage {jumlah}, HP sisa: {hp}");

        if (hp <= 0)
        {
            Mati();
        }
    }

    private void Mati()
    {
        Debug.Log($"{gameObject.name} mati!");
        Destroy(gameObject);
    }
}

