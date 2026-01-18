using System;

public static class PlayerStats
{
    public static int coins = 0;
    public static int score = 0;
    public static int lives = 3;
    public static float timeRemaining = 400.0f;
    public static bool SuperMarioPowerup { get; private set; }
    public static bool FirePowerup { get; private set; }

    public static event Action<bool> OnSuperMarioSet;
    public static event Action OnFireSet;

    static PlayerStats()
    {
        SoftReset();
    }

    public static void SoftReset()
    {
        coins = 0;
        score = 0;
        timeRemaining = 400.0f;
        SuperMarioPowerup = false;
        FirePowerup = false;
    }

    public static void SetSuperMarioPowerup(bool state)
    {
        SuperMarioPowerup = state;
        if (!SuperMarioPowerup)
        {
            FirePowerup = false;
        }
        OnSuperMarioSet?.Invoke(state);
    }

    public static void SetFirePowerup()
    {
        FirePowerup = true;
        OnFireSet?.Invoke();
    }
}
