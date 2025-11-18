using UnityEngine;

public interface IBsodaShooter
{
    public void ShootBsoda(ITM_BSODA bsoda, PlayerManager pm, Vector3 position, Quaternion rotation);
}