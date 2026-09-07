using UnityEngine;
using UnityEngine.AI;

public class golem : Enemy
{
    public bool flag = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

   public virtual void serang()
    {
        Debug.Log("Golem is attacking the player!");
    }
  }
