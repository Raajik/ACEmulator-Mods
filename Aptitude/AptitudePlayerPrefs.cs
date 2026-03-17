namespace Aptitude;

// Persisted per-character; used for opt-in and permanent lockout.
public class AptitudePlayerPrefs
{
    public bool Enabled { get; set; }
    public bool PermanentlyOptedOut { get; set; }
}
