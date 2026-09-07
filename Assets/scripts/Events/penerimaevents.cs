using UnityEngine;

public class penerimaevents : MonoBehaviour
{
     private void OnEnable()
    {
        pemancarEvants.saattombolditekan += respon;
    }

    private void OnDisable()
    {
        pemancarEvants.saattombolditekan -= respon;
    }

    void respon()
    {
        Debug.Log("Tombol ditekan, event diterima!");
    }
}
