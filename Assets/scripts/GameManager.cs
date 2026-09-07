using UnityEngine;
public class GameManager : MonoBehaviour
{
 public int totalKoin;
 private int koinTerkumpul = 0;
 void Start()
 {
 // TODO: hitung jumlah koin di scene saat mulai
 totalKoin = GameObject.FindGameObjectsWithTag("Coin").Length;
 Debug.Log("Total koin di scene: " + totalKoin);
 }
 public void AmbilKoin()
 {
 koinTerkumpul++;
 Debug.Log("Koin terkumpul: " + koinTerkumpul);
 // TODO: jika koinTerkumpul == totalKoin, panggil Menang()
 if (koinTerkumpul == totalKoin) Menang();
 }
 void Menang()
 {
 Debug.Log("KAMU MENANG!");
 }

 
}