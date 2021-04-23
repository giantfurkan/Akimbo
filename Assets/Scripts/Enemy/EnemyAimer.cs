public class EnemyAimer : Aimer
{
    Player player;
    private void Awake()
    {
        player = GameManager.Clone;
    }
    public bool Aim()
    {
        bool success = false;
        minDistance = -1;
        success = IsVisible(player.transform);
        return success;
    }
}
