using UnityEngine;
using System;

public class BelajarDelegating : MonoBehaviour
{
    delegate void  BelajarDelegate();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       UjiDelegating1(); 
       UjiDelegating2();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UjiDelegating1()
    {
        BelajarDelegate panggilhallo = this.panggilhallo;
        panggilhallo();
    }

    void UjiDelegating2()
    {
        BelajarDelegate halo = panggilhallo;
        halo += panggilnama;
        halo();

    }

    void ujiDelegating3()
    {
        Action panggil = panggilhallo;
        panggil += panggilnama;
        panggil();
    }

    void panggilhallo()
    {
        Debug.Log("Hallo");
    }

    void panggilnama()
    {
        Debug.Log("Nama saya adalah John Doe");
    }
}
