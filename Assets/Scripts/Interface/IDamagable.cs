using System.Collections;

public interface IDamagable
{
    void Hit(int damage);

    IEnumerator HitEffect();
}