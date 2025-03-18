namespace Career_Tracker_Backend.Services.InscriptionService
{
    public interface IInscriptionService
    {
        Task SyncEnrollmentsAsync(int courseId);
    }
}
