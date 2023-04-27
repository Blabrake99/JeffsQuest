public interface IDamageble
{
    int Health { get; set; }
    void Damage(int amount);
}